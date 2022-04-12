/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DarkRift.Client
{
    /// <summary>
    ///     A connection to a remote server.
    /// </summary>
    public abstract class NetworkClientConnection : IDisposable
    {
        /// <summary>
        ///     Delegate for handling messages.
        /// </summary>
        /// <param name="messageBuffer">The message buffer received.</param>
        /// <param name="sendMode">The send mode the message was received with.</param>
        public delegate void MessageReceviedHandler(MessageBuffer messageBuffer, SendMode sendMode);

        /// <summary>
        ///     Delegate for handling disconnections.
        /// </summary>
        /// <param name="socketError">The socket error that caused the disconnection.</param>
        /// <param name="exception">The exception that caused the disconnection.</param>
        public delegate void DisconnectedHandler(SocketError socketError, Exception exception);

        /// <summary>
        ///     The method called when a message has been received.
        /// </summary>
        public MessageReceviedHandler MessageReceived { get; set; }

        /// <summary>
        ///     The method called when this connection is disconnected.
        /// </summary>
        public DisconnectedHandler Disconnected { get; set; }

        /// <summary>
        ///     The state of the connection.
        /// </summary>
        public abstract ConnectionState ConnectionState { get; }

        /// <summary>
        ///     The endpoints of the connection.
        /// </summary>
        public abstract IEnumerable<IPEndPoint> RemoteEndPoints { get; }

        /// <summary>
        ///     Creates a new client connection.
        /// </summary>
        public NetworkClientConnection()
        {
            
        }

        /// <summary>
        ///     Connects to a remote device.
        /// </summary>
        public abstract void Connect();

        /// <summary>
        ///     Sends a message using the appropriate protocol.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="sendMode">How the message should be sent.</param>
        /// <returns>Whether the send was successful.</returns>
        /// <remarks>
        ///     <see cref="MessageBuffer"/> is an IDisposable type and therefore once you are done 
        ///     using it you should call <see cref="MessageBuffer.Dispose"/> to release resources.
        ///     Not doing this will result in memnory leaks.
        /// </remarks>
        public virtual bool SendMessage(MessageBuffer message, SendMode sendMode)
        {
            if (sendMode == SendMode.Reliable)
                return SendMessageReliable(message);
            else
                return SendMessageUnreliable(message);
        }

        /// <summary>
        ///     Sends a message reliably.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <returns>Whether the send was successful.</returns>
        /// <remarks>
        ///     <see cref="MessageBuffer"/> is an IDisposable type and therefore once you are done 
        ///     using it you should call <see cref="MessageBuffer.Dispose"/> to release resources.
        ///     Not doing this will result in memnory leaks.
        /// </remarks>
        public abstract bool SendMessageReliable(MessageBuffer message);

        /// <summary>
        ///     Sends a message unreliably.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <returns>Whether the send was successful.</returns>
        /// <remarks>
        ///     <see cref="MessageBuffer"/> is an IDisposable type and therefore once you are done 
        ///     using it you should call <see cref="MessageBuffer.Dispose"/> to release resources.
        ///     Not doing this will result in memnory leaks.
        /// </remarks>
        public abstract bool SendMessageUnreliable(MessageBuffer message);

        /// <summary>
        ///     Disconnects this client from the remote host.
        /// </summary>
        /// <returns>Whether the disconnect was successful.</returns>
        public abstract bool Disconnect();

        /// <summary>
        ///     Gets the endpoint with the given name.
        /// </summary>
        /// <param name="name">The name of the endpoint.</param>
        /// <returns>The end point.</returns>
        public abstract IPEndPoint GetRemoteEndPoint(string name);

        /// <summary>
        ///     Handles a buffer being received. 
        /// </summary>
        /// <param name="message">The message received.</param>
        /// <param name="mode">The <see cref="SendMode"/> used to send the data.</param>
        protected void HandleMessageReceived(MessageBuffer message, SendMode mode)
        {
            MessageReceived?.Invoke(message, mode);
        }

        /*
         * To ensure compatibility with older SocketError Disconnected event parameters we 
         * need to provide SocketErrors where possible which can make this a pain in the neck.
         */

        /// <summary>
        ///     Handles a disconnection.
        /// </summary>
        protected void HandleDisconnection()
        {
            Disconnected?.Invoke(SocketError.Success, null);
        }

        /// <summary>
        ///     Handles a disconnection.
        /// </summary>
        /// <param name="error">The error that describes the cause of disconnection.</param>
        protected void HandleDisconnection(SocketError error)
        {
            //Not all socket errors make sense to have an exception really
            if (error == SocketError.Success || error == SocketError.Disconnecting)
                Disconnected?.Invoke(error, null);
            else
                Disconnected?.Invoke(error, new SocketException((int)error));
        }

        /// <summary>
        ///     Handles a disconnection.
        /// </summary>
        /// <param name="exception">An exception that describes the cause of disconnection.</param>
        protected void HandleDisconnection(Exception exception)
        {
            //Make sure socket exceptions expose socket error code
            if (exception is SocketException)
                Disconnected?.Invoke(((SocketException)exception).SocketErrorCode, exception);
            else
                Disconnected?.Invoke(SocketError.SocketError, exception);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        ///     Disposes of the client connection.
        /// </summary>
        /// <param name="disposing">Whether the object is bing disposed or not.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                }

                disposedValue = true;
            }
        }

        /// <summary>
        ///     Disposes of this client connection.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
