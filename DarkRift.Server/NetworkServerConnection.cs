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

namespace DarkRift.Server
{
    /// <summary>
    ///     Base class handling a connection to a client.
    /// </summary>
    public abstract class NetworkServerConnection : IDisposable
    {
        /// <summary>
        ///     The state of this connection.
        /// </summary>
        public abstract ConnectionState ConnectionState { get; }

        /// <summary>
        ///     The collection of end points this connection is connected to.
        /// </summary>
        public abstract IEnumerable<IPEndPoint> RemoteEndPoints { get; }
        
        /// <summary>
        ///     The action to call when a message is received.
        /// </summary>
        internal Action<MessageBuffer, SendMode> MessageReceived { get; set; }

        /// <summary>
        ///     The action to call when the connection is remotely disconnected.
        /// </summary>
        internal Action<SocketError, Exception> Disconnected { get; set; }

        /// <summary>
        ///     Get's an end point of the remote client.
        /// </summary>
        public abstract IPEndPoint GetRemoteEndPoint(string name);
        
        /// <summary>
        ///     The client related to this server connection.
        /// </summary>
        internal Client Client { get; set; }

        /// <summary>
        ///     Handles a buffer being received. 
        /// </summary>
        /// <param name="message">The message received.</param>
        /// <param name="mode">The <see cref="SendMode"/> used to send the data.</param>
        protected void HandleMessageReceived(MessageBuffer message, SendMode mode)
        {
            MessageReceived?.Invoke(message, mode);
        }

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
        ///     Begins listening for data.
        /// </summary>
        public abstract void StartListening();

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

        // TODO might be good to have a Drop() method that calls Disconnect by default but is overridable if the listener wants to

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

        /// <summary>
        ///     Applies a strike on the associated client.
        /// </summary>
        /// <param name="message">An optional message describing the strike.</param>
        /// <param name="weight">The number of strikes this accounts for.</param>
        protected void Strike(string message = null, int weight = 1)
        {
            if (Client != null)
                Client.Strike(StrikeReason.ConnectionRequest, message, weight);
        }
        
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        ///     Disposes of the server connection.
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
        ///     Disposes of this server connection.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
