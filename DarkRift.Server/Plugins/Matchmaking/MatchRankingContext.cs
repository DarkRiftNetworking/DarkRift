/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

#if PRO
namespace DarkRift.Server.Plugins.Matchmaking
{
    /// <summary>
    ///     Provides additional information about a ranking operation.
    /// </summary>
    /// <typeparam name="T">The type of entities being ranked.</typeparam>
    /// <remarks>
    ///     <c>Pro only.</c>
    /// </remarks>
    public class MatchRankingContext<T>
    {
        /// <summary>
        ///     The threshold at which a ranking will not be considered at all.
        /// </summary>
        private float DiscardThreshold { get; }

        internal MatchRankingContext(float discardThreshold)
        {
            this.DiscardThreshold = discardThreshold;
        }
    }
}
#endif
