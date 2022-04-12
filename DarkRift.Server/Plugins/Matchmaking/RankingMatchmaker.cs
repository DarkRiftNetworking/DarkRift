/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

#if PRO
namespace DarkRift.Server.Plugins.Matchmaking
{
    /// <summary>
    ///     Assign entities into groups based on a ranking between them.
    /// </summary>
    /// <typeparam name="T">The type of object the groups are being formed for.</typeparam>
    /// <remarks>
    ///     <c>Pro only.</c>
    /// </remarks>
    public abstract class RankingMatchmaker<T> : Plugin, IMatchmaker<T>
    {
        /*
        * .--------------------------------------- !!! BEWARE !!! ---------------------------------------.
        * | This plugin DOES NOT abide by the usual thread safe rule other DarkRift plugins do. It uses  |
        * | threads regardless of whether others are or are not.                                         |
        * '----------------------------------------------------------------------------------------------'
        */

        /// <summary>
        ///     Holder for matches with other entities.
        /// </summary>
        private struct Match : IEquatable<Match>
        {
            public QueueGroup other;                //Rankings are Distributive so can be associated to a group
            public float value;

            public override int GetHashCode()
            {
                return other.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj is Match)
                    return other.Equals(((Match)obj).other);
                else
                    return false;
            }

            public bool Equals(Match otherMatch)
            {
                return other == otherMatch.other;
            }
        }

        /// <summary>
        ///     Information for a group in queue.
        /// </summary>
        private class QueueGroup
        {
            public RankingMatchmakerQueueTask<T> task;
            public LinkedList<Match> matches = new LinkedList<Match>();
        }

        /*
         * This matchmaker queues up enqueue and dequeue requests so that the collection of entities
         * doesn't change while the matchmaker is operating on the background thread. A snapshot of
         * the collection is stored for GetSuitabilityMetric so that it can still be invoked on 
         * whichever thread performed the enqueue.
         * 
         * This looks something like this:
         * 
         *                  |- Flush -|- Matchmake -|
         * outQueue      __________    ______________
         * inQueue       _______   \   ______________
         * queue         _______\___\________________
         * queueSnapshot _____________ \_____________
         * 
         * 1. InQueue is merged into queue
         * 2. OutQueue is merged into queue
         * 3. QueueSnapshot is taken from queue
         * 4. Matchmaking happens on queue
         */

        /// <summary>
        ///     The list of entities in queue. Should not be edited outside of the tick method, use <see cref="queueSnapshot"/> instead.
        /// </summary>
        /// <remarks>
        ///     A linked list is used here instead of a queue as we need to be able to remove at an arbitrary point in case of cancellation.
        ///     It's just a little more advanced.
        /// </remarks>
        private readonly LinkedList<QueueGroup> queue = new LinkedList<QueueGroup>();

        /// <summary>
        ///     The snapshot of the queue before each tick.
        /// </summary>
        private QueueGroup[] queueSnapshot = new QueueGroup[0];

        /// <summary>
        ///     The entities being enqueued next flush.
        /// </summary>
        private readonly Queue<QueueGroup> inQueue = new Queue<QueueGroup>();

        /// <summary>
        ///     The entity groups being dequeued next flush.
        /// </summary>
        private readonly Queue<RankingMatchmakerQueueTask<T>> outQueue = new Queue<RankingMatchmakerQueueTask<T>>();

        /// <summary>
        ///     The threshold at which a match will be immediately discarded.
        /// </summary>
        /// <remarks>
        ///     When the matchmaker analyzes the set of entities to find possible matches it
        ///     calculates the suitability metric for all entity pairs. If the score
        ///     returned by <see cref="GetSuitabilityMetric(T, T, MatchRankingContext{T})"/> 
        ///     is below the DiscardThreshold it will not be considered any further in 
        ///     trimming the search tree substantially.
        ///     
        ///     Increasing this value will improve performance and memory usage but may return 
        ///     worse results and cause longer wait times.
        /// </remarks>
        public float DiscardThreshold { get; }

        /// <summary>
        ///     The threshold at which a match will be immediately discarded.
        /// </summary>
        /// <remarks>
        ///     When the matchmaker analyzes the set of entities to find possible matches it
        ///     calculates the average suitability metric between each group (or group of one for 
        ///     individuals). If the average score calculated is below the GroupDiscardThreshold 
        ///     it will not be considered any further in computation therefore trimming the search tree substantially.
        ///     
        ///     Increasing this value will improve performance and memory usage but may return 
        ///     worse results and cause longer wait times.
        /// </remarks>
        public float GroupDiscardThreshold { get; }

        /// <summary>
        ///     The number of entities to collate to form a room.
        /// </summary>
        public int EntitiesPerGroup { get; }

        /// <summary>
        ///     The time in milliseconds between full searches are carried out.
        /// </summary>
        public int TickPeriod { get; }

        /// <inheritdoc />
        public event EventHandler<GroupFormedEventArgs<T>> GroupFormed;

        /// <summary>
        ///     The timer invoking <see cref="RankingMatchmaker{T}.Tick(object, System.Timers.ElapsedEventArgs)"/>.
        /// </summary>
        private System.Timers.Timer timer;

        /// <summary>
        ///     Creates a new RankingMatchmaker/>.
        /// </summary>
        /// <param name="pluginLoadData">The data to load with.</param>
        public RankingMatchmaker(PluginLoadData pluginLoadData)
            : base (pluginLoadData)
        {
            if (float.TryParse(pluginLoadData.Settings["discardThreshold"], out float discardThreshold))
            {
                if (discardThreshold >= 0 && discardThreshold <= 1)
                {
                    this.DiscardThreshold = discardThreshold;
                }
                else
                {
                    this.DiscardThreshold = 1;
                    Logger.Error("Discard threshold was outside of the [0-1] range. Using a value of 1 instead.");
                }
            }
            else
            {
                this.DiscardThreshold = 1;
                Logger.Error("Discard threshold not parsable to a float value, using a value of 1 instead.");
            }

            if (float.TryParse(pluginLoadData.Settings["groupDiscardThreshold"], out float groupDiscardThreshold))
            {
                if (groupDiscardThreshold >= 0 && groupDiscardThreshold <= 1)
                {
                    this.GroupDiscardThreshold = groupDiscardThreshold;
                }
                else
                {
                    this.GroupDiscardThreshold = 1;
                    Logger.Error("Group discard threshold was outside of the [0-1] range. Using a value of 1 instead.");
                }
            }
            else
            {
                this.GroupDiscardThreshold = 1;
                Logger.Error("Group discard threshold not parsable to a float value, using a value of 1 instead.");
            }

            if (int.TryParse(pluginLoadData.Settings["entitiesPerGroup"], out int entitiesPerGroup))
            {
                this.EntitiesPerGroup = entitiesPerGroup;
            }
            else
            {
                Logger.Fatal("Entities per group not parsable to an int value.");
            }

            if (int.TryParse(pluginLoadData.Settings["tickPeriod"], out int tickPeriod) && tickPeriod > 0)
            {
                this.TickPeriod = tickPeriod;
            }
            else
            {
                this.TickPeriod = 500;
                Logger.Error("Tick period not parsable to an int value, using a value of 500ms instead.");
            }
        }

        /// <summary>
        ///     Method invoked when the server is loaded.
        /// </summary>
        /// <param name="args">The event args.</param>
#if PRO
        protected 
#endif
            internal override void Loaded(LoadedEventArgs args)
        {
            Logger.Trace("Setting up timer with " + TickPeriod + "ms period.");

            //Create recurring tick
            timer = new System.Timers.Timer(TickPeriod);
            timer.Elapsed += Tick;
            timer.Start();

            base.Loaded(args);
        }
        
        /// <summary>
        ///     Returns a suitability metric for the given entity pair.
        /// </summary>
        /// <param name="entity1">The first entity.</param>
        /// <param name="entity2">The second entity.</param>
        /// <param name="context">Additional information about the ranking.</param>
        /// <returns>A value between 0 and 1 indicating the suitability where 1 is the perfect match for each other and 0 is the worst possible match.</returns>
        public abstract float GetSuitabilityMetric(T entity1, T entity2, MatchRankingContext<T> context);

        /// <inheritdoc/>
        public IMatchmakerQueueTask<T> Enqueue(T entity, EventHandler<MatchmakingStateChangedEventArgs<T>> callback = null)
        {
            return EnqueueGroup(new EntityGroup<T>() { entity }, callback);
        }

        /// <inheritdoc/>
        public IMatchmakerQueueTask<T> EnqueueGroup(IEnumerable<T> entities, EventHandler<MatchmakingStateChangedEventArgs<T>> callback = null)
        {
            return EnqueueGroup(new EntityGroup<T>(entities), callback);
        }

        /// <inheritdoc/>
        public IMatchmakerQueueTask<T> EnqueueGroup(EntityGroup<T> entities, EventHandler<MatchmakingStateChangedEventArgs<T>> callback = null)
        {
            RankingMatchmakerQueueTask<T> task = new RankingMatchmakerQueueTask<T>(this, entities, callback);
            QueueGroup queueGroup = new QueueGroup { task = task };

            MatchRankingContext<T> context = new MatchRankingContext<T>(DiscardThreshold);

            //Look at all other entities in queue and rate them
            lock (queueSnapshot)
                AddMatchesToQueueGroup(queueGroup, queueSnapshot, context);

            lock (inQueue)
            {
                //Rate those to be added in future
                AddMatchesToQueueGroup(queueGroup, inQueue, context);
                
                //Add to in queue for next tick/flush
                inQueue.Enqueue(queueGroup);
            }

            return task;
        }

        /// <summary>
        ///     Calculates and adds matches to the given group.
        /// </summary>
        /// <param name="group">The group to calculate for.</param>
        /// <param name="groups">The groups to calculate against.</param>
        /// <param name="context">The context for the matchmaker.</param>
        private void AddMatchesToQueueGroup(QueueGroup group, IEnumerable<QueueGroup> groups, MatchRankingContext<T> context)
        {
            foreach (QueueGroup other in groups)
            {
                float outerAcc = 0;
                bool failed = false;
                foreach (T entity in group.task.Entities)
                {
                    float acc = 0;
                    foreach (T otherEntity in other.task.Entities)
                    {
                        float value = GetSuitabilityMetric(entity, otherEntity, context);

                        //Check individual discard threshold, if not enough ignore the group
                        if (value < DiscardThreshold)
                        {
                            failed = true;
                            break;
                        }

                        acc += value;
                    }

                    //If we've already failed then just skip the reset
                    if (failed)
                        break;

                    outerAcc += acc / other.task.Entities.Count;
                }

                //Already failed, don't add match 
                if (failed)
                    continue;
                    
                outerAcc /= group.task.Entities.Count;

                //If a good enough match then add to our list of posible matches
                if (outerAcc >= GroupDiscardThreshold)
                    group.matches.AddLast(new Match { other = other, value = outerAcc });
            }
        }

        /// <summary>
        ///     Cancels a given task
        /// </summary>
        /// <param name="task">The task to cancel.</param>
        internal void Cancel(RankingMatchmakerQueueTask<T> task)
        {
            //Add to out queue for next tick/flush
            lock (outQueue)
                outQueue.Enqueue(task);
        }

        /// <summary>
        ///     Performs a full search.
        /// </summary>
        /// <param name="sender">The timer that invoked the tick.</param>
        /// <param name="args">The event arguments provided by the timer.</param>
        /// <remarks>
        ///     !!! BEWARE !!!
        ///     This executes on a separate thread regardless of the threadsafe settings in DarkRift!
        /// </remarks>
        private void Tick(object sender, System.Timers.ElapsedEventArgs args)
        {
            //Crude way to stop a tick occuring while already searching without losing the 
            //regularity of the timer ticks. We have a lock on queue anyway so it doesn't 
            //matter if somehow a tick does get through, it'll just wait it's turn. Also, 
            //the user would have to have set the timer period very small as well so really 
            //it's their fault...
            timer.Elapsed -= Tick;

            PerformFullSearch();

            timer.Elapsed += Tick;
        }

        /// <summary>
        ///     Attempts to match all clients in the queue.
        /// </summary>
        /// <remarks>
        ///     This will block flush all in/out queues and perform a full search. If a search is 
        ///     already in progress this method will block until it completes. Normallly you would 
        ///     perform searches from the timers already setup rather than calling this method 
        ///     directly.
        /// </remarks>
        public void PerformFullSearch()
        {
            IEnumerable<QueueGroup> inserted, removed;
            IEnumerable<IEnumerable<QueueGroup>> groupsFormed;
            lock (queue)
            {
                //Flush in queue before processing
                inserted = FlushInQueue();

                //Flush out queue after in queue so any cancellations take effect in time
                removed = FlushOutQueue();

                //Take snapshot of queue
                SnapshotQueue();
                
                //Perform search
                groupsFormed = DoFullSearch();
            }

            //Inform those inserted
            InformInsertedOrRemoved(inserted, MatchmakingState.Queued);

            //Inform those removed
            InformInsertedOrRemoved(removed, MatchmakingState.Cancelled);

            //Inform our new groups!
            InformGroupsFormed(groupsFormed);

            // TODO add metrics for queued, cancelled, groups formed, queue time, average match score
        }

        /// <summary>
        ///     Flushes all entities from the in queue into the matchmaker.
        /// </summary>
        /// <returns>The groups inserted into the queue.</returns>
        private IEnumerable<QueueGroup> FlushInQueue()
        {
            Queue<QueueGroup> toInform = new Queue<QueueGroup>();

            lock (inQueue)
            {
                while (inQueue.Count > 0)
                {
                    QueueGroup queueGroup = inQueue.Dequeue();

                    //Copy suitability metrics into the other entities
                    foreach (Match match in queueGroup.matches)
                        match.other.matches.AddLast(new Match { other = queueGroup, value = match.value });

                    queue.AddLast(queueGroup);

                    toInform.Enqueue(queueGroup);
                }
            }

            return toInform;
        }

        /// <summary>
        ///     Flushes all the entities in the out queue from the matchmaker.
        /// </summary>
        /// <returns>The groups removed from the queue.</returns>
        private IEnumerable<QueueGroup> FlushOutQueue()
        {
            Queue<QueueGroup> toInform = new Queue<QueueGroup>();

            lock (outQueue)
            {
                while (outQueue.Count > 0)
                {
                    RankingMatchmakerQueueTask<T> task = outQueue.Dequeue();

                    //Get group and check it exists
                    QueueGroup queueGroup = queue.FirstOrDefault((g) => g.task == task);
                    if (queueGroup != null)
                    {
                        //Remove the group
                        queue.Remove(queueGroup);

                        //Remove all matches it had so we don't consider it in matchmaking
                        foreach (QueueGroup other in queue)
                            other.matches.Remove(new Match { other = queueGroup });

                        //If removed, inform it that it was cancelled
                        toInform.Enqueue(queueGroup);
                    }
                }
            }

            return toInform;
        }

        /// <summary>
        ///     Takes a snapshot of the queue for the enqueue method to use.
        /// </summary>
        private void SnapshotQueue()
        {
            queueSnapshot = queue.ToArray();
        }

        /// <summary>
        ///     Attempts to match all clients in the queue
        /// </summary>
        private IEnumerable<IEnumerable<QueueGroup>> DoFullSearch()
        {
            Queue<IEnumerable<QueueGroup>> groupsFormed = new Queue<IEnumerable<QueueGroup>>();

            LinkedListNode<QueueGroup> next = queue.First;
            while (next != null)
            {
                IEnumerable<QueueGroup> group = PerformSearch(new QueueGroup[] { next.Value });

                if (group != null)
                {
                    //Remove them from queue
                    foreach (QueueGroup subGroup in group)
                    {
                        //Remove the entity
                        queue.Remove(subGroup);

                        //Remove all matches it had so we don't consider it in future matchmaking
                        foreach (Match match in subGroup.matches)
                            match.other.matches.Remove(new Match { other = subGroup });
                    }

                    groupsFormed.Enqueue(group);
                }

                next = next.Next;
            }

            return groupsFormed;
        }

        /// <summary>
        ///     Informs those who were inserted or removed from the queue.
        /// </summary>
        /// <param name="toInform">The groups enqueued to inform.</param>
        /// <param name="newState">The new state of the groups.</param>
        private void InformInsertedOrRemoved(IEnumerable<QueueGroup> toInform, MatchmakingState newState)
        {
            foreach (QueueGroup queueGroup in toInform)
                queueGroup.task.SetMatchmakingState(newState, new EntityGroup<T>[1] { queueGroup.task.Entities }, queueGroup.task.Entities);
        }

        /// <summary>
        ///     Informs the those in the groups that have been formed about the groups.
        /// </summary>
        /// <param name="toInform">The groups formed.</param>
        private void InformGroupsFormed(IEnumerable<IEnumerable<QueueGroup>> toInform)
        {
            foreach (IEnumerable<QueueGroup> group in toInform)
            {
                //Inform individual groups
                IEnumerable<T> entities = group.SelectMany(g => g.task.Entities);
                IEnumerable<EntityGroup<T>> subGroups = group.Select(p => p.task.Entities);

                //Inform all entities they've been cancelled
                foreach (QueueGroup queueGroup in group)
                    queueGroup.task.SetMatchmakingState(MatchmakingState.Success, subGroups, entities);

                //Invoke group formed callback
                GroupFormedEventArgs<T> args = new GroupFormedEventArgs<T>(subGroups, entities);
                ThreadHelper.DispatchIfNeeded(() =>
                {
                    GroupFormed?.Invoke(this, args);
                });
            }
        }

        /// <summary>
        ///     Performs a search starting at the given entity.
        /// </summary>
        /// <param name="startGroup">The starting group.</param>
        private IEnumerable<QueueGroup> PerformSearch(IEnumerable<QueueGroup> startGroup)
        {
            Stack<IEnumerable<QueueGroup>> toTry = new Stack<IEnumerable<QueueGroup>>();
            toTry.Push(startGroup);

            while (toTry.Count > 0)
            {
                //Pop next off
                IEnumerable<QueueGroup> group = toTry.Pop();

                //Count number of entities currently in the group
                int currentCount = group.Sum(g => g.task.Entities.Count);

                //If we have a complete group return it
                if (currentCount == EntitiesPerGroup)
                    return group;

                //Calculate the matches available for this group
                IEnumerable<Match> matches = group                  //TODO 2 Possibly able to use previous frame's match list here
                    .SelectMany(m => m.matches)                     //Get all group-group matches available
                    .Where(m => currentCount + m.other.task.Entities.Count <= EntitiesPerGroup)    //Ensure this match doesn't take us over the room capacity
                    .Where(m => !group.Contains(m.other))           //Remove matches to entity groups already in the group
                    .GroupBy(m => m.other)                          //Put matches into groups about the same entity group
                    .Where(g => g.Count() == group.Count())         //Remove any matches that have been discarded by any member (i.e. score was less than one of the discard thresholds)
                    .Select((g) => new Match { other = g.First().other, value = g.Sum(m => m.value) })      //Take match groups to a single group-entity group match object using summation to calculate new score
                    .OrderBy(m => -m.value);                        //Order by ascending as we're pushing onto a stack so the order will be reversed!

                //Iterate over the matches
                foreach (Match match in matches)
                {
                    //Recurse with new group
                    IEnumerable<QueueGroup> newGroup = group.Union(new QueueGroup[] { match.other });
                    toTry.Push(newGroup);
                }
            }
            
            //Out of elements so no matches found a group, return failure
            return null;
        }

        /// <summary>
        ///     Disposes this matchmaker.
        /// </summary>
        /// <param name="disposing">Whether the matchmaker is being disposed or not.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                timer.Dispose();

            base.Dispose(disposing);
        }
    }
}
#endif
