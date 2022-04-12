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
    ///     Event args for changes to a groups's matchmaking state.
    /// </summary>
    /// <typeparam name="T">The type of object being matched.</typeparam>
    /// <remarks>
    ///     <c>Pro only.</c>
    /// </remarks>
    public class MatchmakingStateChangedEventArgs<T> : EventArgs
    {
        /// <summary>
        ///     The new state of the matchmaking operation.
        /// </summary>
        public MatchmakingState MatchmakingState { get; }

        /// <summary>
        ///     The sub groups the state change occured for.
        /// </summary>
        public IEnumerable<EntityGroup<T>> SubGroups { get; }

        /// <summary>
        ///     The entities the matchmaking state change occurred for.
        /// </summary>
        public IEnumerable<T> Entities { get; }

        /// <summary>
        ///     Creates new event args with the given state.
        /// </summary>
        /// <param name="matchmakingState">The new matchmaking state.</param>
        /// <param name="subGroups">The subgroups that the matchamking state change occured for.</param>
        /// <param name="entities">The entities the matchmaking state change occurred for.</param>
        public MatchmakingStateChangedEventArgs(MatchmakingState matchmakingState, IEnumerable<EntityGroup<T>> subGroups, IEnumerable<T> entities)
        {
            this.MatchmakingState = matchmakingState;
            this.SubGroups = subGroups;
            this.Entities = entities;
        }
    }
}
#endif
