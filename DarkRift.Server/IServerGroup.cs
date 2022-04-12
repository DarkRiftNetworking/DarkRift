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
    ///     Represents a group of servers in the cluster.
    /// </summary>
    public interface IServerGroup
    {
        /// <summary>
        ///     Gets a server by the server's ID.
        /// </summary>
        /// <param name="id">The ID of the server.</param>
        /// <returns>The server with that ID.</returns>
        IRemoteServer this[ushort id] { get; }
        
        /// <summary>
        ///     Event fired when a server joins.
        /// </summary>
        event EventHandler<ServerJoinedEventArgs> ServerJoined;

        /// <summary>
        ///     Event fired when a server leaves the group.
        /// </summary>
        event EventHandler<ServerLeftEventArgs> ServerLeft;

        /// <summary>
        ///     The number of servers currently in this group.
        /// </summary>
        int Count { get; }

        /// <summary>
        ///     The name of this server group.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     The visibility of the server.
        /// </summary>
        ServerVisibility Visibility { get; }

        /// <summary>
        ///     The connection direction to this group.
        /// </summary>
        ServerConnectionDirection Direction { get; }


        /// <summary>
        ///     Returns all remote servers in this group.
        /// </summary>
        /// <returns>The remote servers in this group.</returns>
        IRemoteServer[] GetAllRemoteServers();

        /// <summary>
        ///     Returns a specific remote server in this group by ID.
        /// </summary>
        /// <param name="id">The Id of the server.</param>
        /// <returns>The remote server.</returns>
        IRemoteServer GetRemoteServer(ushort id);
    }
#endif
}
