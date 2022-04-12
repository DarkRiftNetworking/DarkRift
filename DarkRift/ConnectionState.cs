/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace DarkRift
{
    /// <summary>
    ///     Indicated the current state of the connection between server and client.
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>
        ///     The server and client are disconnected.
        /// </summary>
        Disconnected,

        /// <summary>
        ///     The server and client are establishing a connection.
        /// </summary>
        /// <remarks>
        ///     This state might not be used by a connection and may be skipped over.
        /// </remarks>
        Connecting,
        
        /// <summary>
        ///     The server and client are connected and can send and receive data.
        /// </summary>
        Connected,

        /// <summary>
        ///     The server and client are closing the connection.
        /// </summary>
        /// <remarks>
        ///     This state might not be used by a connection and may be skipped over.
        /// </remarks>
        Disconnecting,

        /// <summary>
        ///     The server and client were connected but a fault has broken the connection. The entities are trying to reconnect.
        /// </summary>
        /// <remarks>
        ///     This state might not be used by a connection.
        /// </remarks>
        Interrupted
    }
}