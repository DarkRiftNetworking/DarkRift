/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;

#if PRO
namespace DarkRift.Server.Plugins.Matchmaking
{
    /// <summary>
    ///     Represents currently queued entities in a matchmaker.
    /// </summary>
    /// <typeparam name="T">The type of objects used as entities.</typeparam>
    /// <remarks>
    ///     <c>Pro only.</c>
    /// </remarks>
    public interface IMatchmakerQueueTask<T>
    {
        /// <summary>
        ///     The event handler for state changes with this group.
        /// </summary>
        EventHandler<MatchmakingStateChangedEventArgs<T>> Callback { get; }

        /// <summary>
        ///     The state of the entities in the matchmaker.
        /// </summary>
        MatchmakingState MatchmakingState { get; }

        /// <summary>
        ///     The matchmaker this task belongs to.
        /// </summary>
        IMatchmaker<T> Matchmaker { get; }

        /// <summary>
        ///     The entities in this matchmaking group.
        /// </summary>
        EntityGroup<T> Entities { get; }

        /// <summary>
        ///     Attempts to cancel matchmaking for this group.
        /// </summary>
        /// <remarks>
        ///     If successfully cancelled, <see cref="Callback"/> will be invoked with <see cref="MatchmakingState.Cancelled"/>.
        /// </remarks>
        void Cancel();
    }
}
#endif
