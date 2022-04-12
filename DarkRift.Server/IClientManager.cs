/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Net;

namespace DarkRift.Server
{
    /// <summary>
    ///     Interface for the connection manager handling connections for the server.
    /// </summary>
    public interface IClientManager
    {
        /// <summary>
        ///     The address he server is listening on.
        /// </summary>
        IPAddress Address { get; }

        /// <summary>
        ///     The IP version that the server is listening on.
        /// </summary>
        [Obsolete("Use Address.Family instead.")]
        IPVersion IPVersion { get; }

        /// <summary>
        ///     The port the server is listening on.
        /// </summary>
        ushort Port { get; }

        /// <summary>
        ///     Invoked when a client connects to the server.
        /// </summary>
        event EventHandler<ClientConnectedEventArgs> ClientConnected;

        /// <summary>
        ///     Invoked when a client disconnects from the server.
        /// </summary>
        event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;

        /// <summary>
        ///     Returns the number of clients currently connected.
        /// </summary>
        int Count { get; }

        /// <summary>
        ///     The number of strikes a client can get before they are kicked.
        /// </summary>
        byte MaxStrikes { get; }

        /// <summary>
        ///     Returns all clients connected to this server.
        /// </summary>
        /// <returns>An array of clients on the server.</returns>
        IClient[] GetAllClients();

        /// <summary>
        ///     Returns the client with the given ID.
        /// </summary>
        /// <param name="id">The global ID of the client.</param>
        /// <returns>The client.</returns>
        IClient this[ushort id] { get; }

        /// <summary>
        ///     Returns the client with the given ID.
        /// </summary>
        /// <param name="id">The global ID of the client.</param>
        /// <returns>The client.</returns>
        IClient GetClient(ushort id);
    }
}
