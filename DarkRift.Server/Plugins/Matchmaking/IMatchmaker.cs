/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;

#if PRO
namespace DarkRift.Server.Plugins.Matchmaking
{
    /// <summary>
    ///     Base interface for all matchmaker implementations.
    /// </summary>
    /// <typeparam name="T">The type of entity the matchmaker operates with.</typeparam>
    /// <remarks>
    ///     <c>Pro only.</c>
    /// </remarks>
    public interface IMatchmaker<T>
    {
        /// <summary>
        ///     Event invoked when a group of entities has been formed.
        /// </summary>
        event EventHandler<GroupFormedEventArgs<T>> GroupFormed;
        
        /// <summary>
        ///     Enqueues an entity into the matchmaker.
        /// </summary>
        /// <param name="entity">The entity to enqueue.</param>
        /// <param name="callback">The callback to make when the matchmnaking state changes for this entity, null to omit.</param>
        IMatchmakerQueueTask<T> Enqueue(T entity, EventHandler<MatchmakingStateChangedEventArgs<T>> callback = null);

        /// <summary>
        ///     Enqueues a group of entities into the matchmaker.
        /// </summary>
        /// <param name="entities">The entities to enqueue.</param>
        /// <param name="callback">The callback to make when the matchmnaking state changes for this entity, null to omit.</param>
        /// <remarks>
        ///     Groups of entities enqueued using this method will be guaranteed to be placed in a room together 
        ///     (provided there are not more in the group than <see cref="RankingMatchmaker{T}.EntitiesPerGroup"/>) even if they would 
        ///     normally rank each other under the <see cref="RankingMatchmaker{T}.DiscardThreshold"/>).
        /// </remarks>
        IMatchmakerQueueTask<T> EnqueueGroup(EntityGroup<T> entities, EventHandler<MatchmakingStateChangedEventArgs<T>> callback = null);

        /// <summary>
        ///     Enqueues a group of entities into the matchmaker.
        /// </summary>
        /// <param name="entities">The entities to enqueue.</param>
        /// <param name="callback">The callback to make when the matchmnaking state changes for this entity, null to omit.</param>
        /// <remarks>
        ///     Groups of entities enqueued using this method will be guaranteed to be placed in a room together 
        ///     (provided there are not more in the group than <see cref="RankingMatchmaker{T}.EntitiesPerGroup"/>) even if they would 
        ///     normally rank each other under the <see cref="RankingMatchmaker{T}.DiscardThreshold"/>).
        /// </remarks>
        IMatchmakerQueueTask<T> EnqueueGroup(IEnumerable<T> entities, EventHandler<MatchmakingStateChangedEventArgs<T>> callback = null);
        
        /// <summary>
        ///     Attempts to match all clients in the queue
        /// </summary>
        /// <remarks>
        ///     This will block flush all in/out queues and perform a full search. If a search is 
        ///     already in progress this method will block until it completes. Normallly you would 
        ///     perform searches from the timers already setup rather than calling this method 
        ///     directly.
        /// </remarks>
        void PerformFullSearch();
    }
}
#endif
