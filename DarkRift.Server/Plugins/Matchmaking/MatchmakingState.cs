/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

#if PRO
namespace DarkRift.Server.Plugins.Matchmaking
{
    /// <summary>
    ///     The state of a matchmaking operation by an <see cref="IMatchmaker{T}"/>.
    /// </summary>
    /// <remarks>
    ///     <c>Pro only.</c>
    /// </remarks>
    public enum MatchmakingState
    {
        /// <summary>
        ///     Indicates the matchmaking operation has not yet been passed to the matchmaker.
        /// </summary>
        Pending,

        /// <summary>
        ///     Indicates the matchmaking operation is queued in the matchmaker.
        /// </summary>
        Queued,

        /// <summary>
        ///     Indicates the matchmakign operation succeeded.
        /// </summary>
        Success,

        /// <summary>
        ///     Indicates the matchmaking operation was cancelled.
        /// </summary>
        Cancelled
    }
}
#endif
