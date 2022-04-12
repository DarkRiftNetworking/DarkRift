/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace DarkRift.Client
{
    /// <summary>
    ///     Arguments for disconnection events.
    /// </summary>
    /// <remarks>
    ///     There are currently no members to this class, it is here for future use.
    /// </remarks>
    public sealed class DisconnectedEventArgs : EventArgs
    {
        /// <summary>
        ///     If the disconnect was requested by a call to <see cref="DarkRiftClient.Disconnect"/>.
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
        ///     Creates a new DisconnectedEventArgs object.
        /// </summary>
        /// <param name="localDisconnect">Whether it was a local call that caused the disconnection.</param>
        /// <param name="error">The error that caused the disconnect.</param>
        /// <param name="exception">The exception that caused the disconnect.</param>
        internal DisconnectedEventArgs(bool localDisconnect, SocketError error, Exception exception)
        {
            this.LocalDisconnect = localDisconnect;
            this.Error = error;
            this.Exception = exception;
        }
    }
}
