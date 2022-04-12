/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;

namespace DarkRift.Server
{
#if PRO
    /// <summary>
    ///     Event arguments for the <see cref="IServerGroup.ServerLeft"/> event.
    /// </summary>
    public class ServerLeftEventArgs : EventArgs
    {
        /// <summary>
        ///     The remote server that left.
        /// </summary>
        public IRemoteServer RemoteServer { get; }

        /// <summary>
        ///     The ID of the remote server that left.
        /// </summary>
        public ushort ID { get; }

        /// <summary>
        ///     The server group the new server left.
        /// </summary>
        public IServerGroup ServerGroup { get; }

        /// <summary>
        ///     Creates new event args.
        /// </summary>
        /// <param name="remoteServer">The server that left.</param>
        /// <param name="id">The ID of the server that left.</param>
        /// <param name="serverGroup">The group the server left.</param>
        public ServerLeftEventArgs(IRemoteServer remoteServer, ushort id, IServerGroup serverGroup)
        {
            this.RemoteServer = remoteServer;
            this.ID = id;
            this.ServerGroup = serverGroup;
        }
    }
#endif
}
