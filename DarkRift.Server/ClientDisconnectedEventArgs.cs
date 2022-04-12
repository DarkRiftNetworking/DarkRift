/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace DarkRift.Server
{
    /// <summary>
    ///     Event arguments for <see cref="ClientManager.ClientDisconnected"/> events.
    /// </summary>
    public class ClientDisconnectedEventArgs : EventArgs
    {
        /// <summary>
        ///     The Client of the new client.
        /// </summary>
        public IClient Client { get; private set; }

        /// <summary>
        ///     The remote end point of the TCP connection to this client.
        /// </summary>
        [Obsolete("Use GetRemoteEndpoint(\"TCP\") instead")]
        public IPEndPoint RemoteTcpEndPoint => Client.GetRemoteEndPoint("tcp");

        /// <summary>
        ///     The remote end point of the UDP connection to this client.
        /// </summary>
        [Obsolete("Use GetRemoteEndpoint(\"UDP\") instead")]
        public IPEndPoint RemoteUdpEndPoint => Client.GetRemoteEndPoint("udp");

        /// <summary>
        ///     The collection of end points this client is connected to.
        /// </summary>
        public IEnumerable<IPEndPoint> RemoteEndPoints => Client.RemoteEndPoints;

        /// <summary>
        ///     If the disconnect was requested by a call to <see cref="Client.Disconnect"/>.
        /// </summary>
        public bool LocalDisconnect { get; }

        /// <summary>
        ///     The error that caused the disconnect.
        /// </summary>
        /// <remarks>
        ///     If <see cref="LocalDisconnect"/> is true this field will be set to a default value and 
        ///     should be ignored.
        ///     
        ///     If the contents of this property is <see cref="SocketError.SocketError"/> consider 
        ///     exploring <see cref="Exception"/> for a general exception that caused the disconnection 
        ///     instead.
        /// </remarks>
        public SocketError Error { get; }

        /// <summary>
        ///     The exception that caused the disconnection.
        /// </summary>
        /// <remarks>
        ///     If <see cref="LocalDisconnect"/> is true this field will be set to a default value and 
        ///     should be ignored.
        /// </remarks>
        public Exception Exception { get; }

        /// <summary>
        ///     Creates a new ClientDisconnectedEventArgs from the given data.
        /// </summary>
        /// <param name="clientConnection">The ClientConnection of the client.</param>
        /// <param name="localDisconnect">Whether it was a local call that caused the disconnection.</param>
        /// <param name="error">The error that caused the disconnect.</param>
        /// <param name="exception">The exception that caused the disconenct.</param>
        public ClientDisconnectedEventArgs(IClient clientConnection, bool localDisconnect, SocketError error, Exception exception)
        {
            this.Client = clientConnection;
            this.LocalDisconnect = localDisconnect;
            this.Exception = exception;
            this.Error = error;
        }

        /// <summary>
        ///     Gets the remote end point with the given name.
        /// </summary>
        /// <param name="name">The end point name.</param>
        /// <returns>The end point.</returns>
        public IPEndPoint GetRemoteEndPoint(string name)
        {
            return Client.GetRemoteEndPoint(name);
        }
    }
}
