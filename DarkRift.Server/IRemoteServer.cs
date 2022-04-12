/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Net;

namespace DarkRift.Server
{
#if PRO
    /// <summary>
    ///     Represents another server in the cluster.
    /// </summary>
    public interface IRemoteServer
    {
        /// <summary>
        ///     The state of the connection.
        /// </summary>
        ConnectionState ConnectionState { get; }

        /// <summary>
        ///     The group this connection belongs to.
        /// </summary>
        IServerGroup ServerGroup { get; }

        /// <summary>
        ///     The direction of this server connection.
        /// </summary>
        ServerConnectionDirection ServerConnectionDirection { get; }

        /// <summary>
        ///     The ID of the server.
        /// </summary>
        ushort ID { get; }

        /// <summary>
        ///     The host connected to.
        /// </summary>
        string Host { get; }

        /// <summary>
        ///     The port connected to.
        /// </summary>
        ushort Port { get; }

        /// <summary>
        ///     The endpoints of the connection.
        /// </summary>
        IEnumerable<IPEndPoint> RemoteEndPoints { get; }

        /// <summary>
        ///     Event fired when a message is received.
        /// </summary>
        event EventHandler<ServerMessageReceivedEventArgs> MessageReceived;

        /// <summary>
        ///     Event fired when the server connects.
        /// </summary>
        event EventHandler<ServerConnectedEventArgs> ServerConnected;

        /// <summary>
        ///     Event fired when the server disconnects.
        /// </summary>
        event EventHandler<ServerDisconnectedEventArgs> ServerDisconnected;


        /// <summary>
        ///     Returns the named endpoint on the remote server.
        /// </summary>
        /// <param name="name">The name of the endpoint.</param>
        /// <returns>The endpoint.</returns>
        IPEndPoint GetRemoteEndPoint(string name);

        /// <summary>
        ///     Sends a message to the remote server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="sendMode">The send mode to send the message with.</param>
        /// <returns>Whether the message was able to be sent.</returns>
        bool SendMessage(Message message, SendMode sendMode);
    }
#endif
}
