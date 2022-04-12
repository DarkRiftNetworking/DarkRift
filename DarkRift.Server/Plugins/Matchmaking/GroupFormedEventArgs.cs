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
    ///     Event args for the <see cref="IMatchmaker{T}.GroupFormed"/> event.
    /// </summary>
    /// <remarks>
    ///     <c>Pro only.</c>
    /// </remarks>
    public class GroupFormedEventArgs<T> : EventArgs
    {
        /// <summary>
        ///     The groups of entities combined to form this group.
        /// </summary>
        public IEnumerable<EntityGroup<T>> SubGroups { get; }

        /// <summary>
        ///     The group formed as a single list of entities.
        /// </summary>
        public IEnumerable<T> Group { get; }

        /// <summary>
        ///     Constructor for the event args.
        /// </summary>
        /// <param name="subGroups">The groups queued that compose the formed group.</param>
        /// <param name="group">The individual entities in the group.</param>
        public GroupFormedEventArgs(IEnumerable<EntityGroup<T>> subGroups, IEnumerable<T> group)
        {
            this.SubGroups = subGroups;
            this.Group = group;
        }
    }
}
#endif
