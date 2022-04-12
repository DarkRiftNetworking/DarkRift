/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace DarkRift.Server
{
#if PRO
    /// <summary>
    ///     Event arguments for <see cref="IRemoteServer.ServerDisconnected"/> events.
    /// </summary>
    public class ServerDisconnectedEventArgs : EventArgs
    {
        /// <summary>
        ///     The remote server that disconnected.
        /// </summary>
        public IRemoteServer RemoteServer { get; private set; }

        /// <summary>
        ///     The collection of end points this server was connected to.
        /// </summary>
        private IEnumerable<IPEndPoint> RemoteEndPoints => RemoteServer.RemoteEndPoints;

        /// <summary>
        ///     The error that caused the disconnect.
        /// </summary>
        /// <remarks>
        ///     If the contents of this property is <see cref="SocketError.SocketError"/> consider 
        ///     exploring <see cref="Exception"/> for a general exception that caused the disconnection 
        ///     instead.
        /// </remarks>
        public SocketError Error { get; }

        /// <summary>
        ///     The exception that caused the disconnection.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        ///     Creates a new ServerDisconnectedEventArgs from the given data.
        /// </summary>
        /// <param name="remoteServer">The RemoteServer that disconnected.</param>
        /// <param name="error">The error that caused the disconnect.</param>
        /// <param name="exception">The exception that caused the disconenct.</param>
        public ServerDisconnectedEventArgs(IRemoteServer remoteServer, SocketError error, Exception exception)
        {
            this.RemoteServer = remoteServer;
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
            return RemoteServer.GetRemoteEndPoint(name);
        }
    }
#endif
}
