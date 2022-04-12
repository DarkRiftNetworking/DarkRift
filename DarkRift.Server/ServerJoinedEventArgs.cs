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
    ///     Event arguments for the <see cref="IServerGroup.ServerJoined"/> event.
    /// </summary>
    public class ServerJoinedEventArgs : EventArgs
    {
        /// <summary>
        ///     The remote server that joined. Null if we do not connect to this group.
        /// </summary>
        public IRemoteServer RemoteServer { get; }

        /// <summary>
        ///     The ID of the remote server that joined.
        /// </summary>
        public ushort ID { get; }

        /// <summary>
        ///     The server group the new server joined.
        /// </summary>
        public IServerGroup ServerGroup { get; }

        /// <summary>
        ///     Creates new event args.
        /// </summary>
        /// <param name="remoteServer">The server that joined.</param>
        /// <param name="id">The ID of the server that joined.</param>
        /// <param name="serverGroup">The group the server joined.</param>
        public ServerJoinedEventArgs(IRemoteServer remoteServer, ushort id, IServerGroup serverGroup)
        {
            this.RemoteServer = remoteServer;
            this.ID = id;
            this.ServerGroup = serverGroup;
        }
    }
#endif
}
