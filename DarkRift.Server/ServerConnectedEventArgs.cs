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
    ///     Event arguments for the <see cref="IRemoteServer.ServerConnected"/> event.
    /// </summary>
    public class ServerConnectedEventArgs : EventArgs
    {
        /// <summary>
        ///     The <see cref="IRemoteServer"/> object for the server that connected.
        /// </summary>
        public IRemoteServer RemoteServer { get; private set; }

        /// <summary>
        ///     The collection of end points this server is connected to.
        /// </summary>
        private IEnumerable<IPEndPoint> RemoteEndPoints => RemoteServer.RemoteEndPoints;

        /// <summary>
        ///     Creates a new ServerConnectedEventArgs from the given data.
        /// </summary>
        /// <param name="remoteServer">The <see cref="IRemoteServer"/> that connected.</param>
        public ServerConnectedEventArgs(IRemoteServer remoteServer)
        {
            this.RemoteServer= remoteServer;
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
