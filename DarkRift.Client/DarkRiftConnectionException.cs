/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Net.Sockets;
using System.Runtime.Serialization;

namespace DarkRift.Client
{
    /// <summary>
    /// Exception thrown when a DarkRift client is unable to establish a connection to the server.
    /// </summary>
    /// <remarks>
    ///     This exception is here to provide more information about the operation the client
    ///     was performing during at the point of connection failure rather than a non-descript
    ///     <see cref="SocketException"/>. In general you should prefer catching a
    ///     <see cref="SocketException"/> over this as not all failures may be emitted as a
    ///     <see cref="DarkRiftConnectionException"/>.
    /// </remarks>
    // TODO DR3 detach from SocketException
    [Serializable]
    internal class DarkRiftConnectionException : SocketException
    {
        public override string Message { get; }

        /// <summary>
        /// The inner <see cref="SocketException"/> that caused this exception.
        /// </summary>
        public SocketException InnerSocketException { get; }

        /// <summary>
        /// Creates a new <see cref="DarkRiftConnectionException"/>.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The <see cref="SocketException"/> that caused this exception.</param>
        public DarkRiftConnectionException(string message, SocketException innerException) : base(innerException.ErrorCode)
        {
            Message = message;
            InnerSocketException = innerException;
        }

        /// <summary>
        /// Creates a new <see cref="DarkRiftConnectionException"/>.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="socketError">The <see cref="SocketError"/> that caused this exception.</param>
        public DarkRiftConnectionException(string message, SocketError socketError) : base((int)socketError)
        {
            Message = message;
        }

        protected DarkRiftConnectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
