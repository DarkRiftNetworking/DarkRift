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
    ///     A matchmmaker task for the <see cref="RankingMatchmaker{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of objects being used as entities.</typeparam>
    /// <remarks>
    ///     <c>Pro only.</c>
    /// </remarks>
    internal class RankingMatchmakerQueueTask<T> : IMatchmakerQueueTask<T>
    {
        /// <inheritdoc/>
        public EventHandler<MatchmakingStateChangedEventArgs<T>> Callback { get; }

        /// <inheritdoc/>
        public IMatchmaker<T> Matchmaker => matchmaker;

        /// <inheritdoc/>
        public MatchmakingState MatchmakingState { get; internal set; }

        /// <inheritdoc/>
        public EntityGroup<T> Entities { get; }

        /// <summary>
        ///     The ranking matchmater in use.
        /// </summary>
        private RankingMatchmaker<T> matchmaker;
        
        internal RankingMatchmakerQueueTask(RankingMatchmaker<T> matchmaker, EntityGroup<T> entities, EventHandler<MatchmakingStateChangedEventArgs<T>> callback)
        {
            this.matchmaker = matchmaker;
            this.Entities = entities;
            this.Callback = callback;
            this.MatchmakingState = MatchmakingState.Pending;
        }

        /// <inheritdoc/>
        public void Cancel()
        {
            matchmaker.Cancel(this);
        }

        /// <summary>
        ///     Sets the matchmaking state and invokes the callback.
        /// </summary>
        /// <param name="newState">The new state.</param>
        /// <param name="subGroups">The subgroups now involved.</param>
        /// <param name="entities">The entities now involved.</param>
        internal void SetMatchmakingState(MatchmakingState newState, IEnumerable<EntityGroup<T>> subGroups, IEnumerable<T> entities)
        {
            MatchmakingState = newState;

            matchmaker.ThreadHelper.DispatchIfNeeded(() =>
            {
                Callback?.Invoke(this, new MatchmakingStateChangedEventArgs<T>(newState, subGroups, entities));
            });
        }
    }
}
#endif
