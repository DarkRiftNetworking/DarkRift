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

namespace DarkRift.Server
{
    /// <summary>
    ///     Event arguments for the <see cref="ClientManager.ClientConnected"/> event.
    /// </summary>
    public class ClientConnectedEventArgs : EventArgs
    {
        /// <summary>
        ///     The <see cref="Client"/> object for the new client.
        /// </summary>
        public IClient Client { get; private set; }

        /// <summary>
        ///     The remote end point of the TCP connection to this client.
        /// </summary>
        [Obsolete("Use GetRemoteEndpoint(\"TCP\") instead")]
        public IPEndPoint RemoteTcpEndPoint => Client.RemoteTcpEndPoint;

        /// <summary>
        ///     The remote end point of the UDP connection to this client.
        /// </summary>
        [Obsolete("Use GetRemoteEndpoint(\"UDP\") instead")]
        public IPEndPoint RemoteUdpEndPoint => Client.RemoteUdpEndPoint;

        /// <summary>
        ///     The collection of end points this client is connected to.
        /// </summary>
        public IEnumerable<IPEndPoint> RemoteEndPoints => Client.RemoteEndPoints;

        /// <summary>
        ///     Creates a new ClientConnectedEventArgs from the given data.
        /// </summary>
        /// <param name="clientConnection">The ClientConnection of the new client.</param>
        public ClientConnectedEventArgs(IClient clientConnection)
        {
            this.Client = clientConnection;
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
