/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;

namespace DarkRift.Server
{
#if PRO
    /// <summary>
    ///     Internal varient of IServerGroup that allows servers to join and leave.
    /// </summary>
    internal interface IModifiableServerGroup : IServerGroup, IDisposable
    {
        /// <summary>
        ///     Handles a new server joining this group.
        /// </summary>
        /// <param name="id">The id of the server.</param>
        /// <param name="host">The host to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        /// <param name="properties">The additional properties to connect with.</param>
        void HandleServerJoin(ushort id, string host, ushort port, IDictionary<string, string> properties);

        /// <summary>
        ///     Handles a server leaving this group.
        /// </summary>
        /// <param name="id">The ID of the server leaving.</param>
        void HandleServerLeave(ushort id);
    }
#endif
}
