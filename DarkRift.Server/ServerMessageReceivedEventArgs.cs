/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;

namespace DarkRift.Server
{
#if PRO
    /// <summary>
    ///     Arguments for the <see cref="IRemoteServer.MessageReceived"/> event.
    /// </summary>
    public class ServerMessageReceivedEventArgs : EventArgs, IDisposable
    {
        /// <summary>
        ///     The method the data was sent using.
        /// </summary>
        public SendMode SendMode { get; private set; }

        /// <summary>
        ///     The server the message was received from.
        /// </summary>
        public IRemoteServer RemoteServer { get; private set; }

        /// <summary>
        ///     The server group the message was received from.
        /// </summary>
        public IServerGroup ServerGroup => RemoteServer.ServerGroup;

        /// <summary>
        ///     The tag the message was sent with.
        /// </summary>
        public ushort Tag => message.Tag;

        /// <summary>
        ///     The message received.
        /// </summary>
        private Message message;

        /// <summary>
        ///     Whether this args object is currently in an object pool waiting or not.
        /// </summary>
        private volatile bool isCurrentlyLoungingInAPool;

        /// <summary>
        ///     Creates a new args object for the <see cref="IRemoteServer.MessageReceived"/> event.
        /// </summary>
        /// <param name="message">The message received.</param>
        /// <param name="sendMode">The send mode the message was received with.</param>
        /// <param name="remoteServer">The server the message was received from.</param>
        public static ServerMessageReceivedEventArgs Create(Message message, SendMode sendMode, IRemoteServer remoteServer)
        {
            ServerMessageReceivedEventArgs messageReceivedEventArgs = ServerObjectCache.GetServerMessageReceivedEventArgs();

            messageReceivedEventArgs.message = message;
            messageReceivedEventArgs.SendMode = sendMode;
            messageReceivedEventArgs.RemoteServer = remoteServer;

            messageReceivedEventArgs.isCurrentlyLoungingInAPool = false;

            return messageReceivedEventArgs;
        }


        /// <summary>
        ///     Creates a new args object for the <see cref="IRemoteServer.MessageReceived"/> event.
        /// </summary>
        internal ServerMessageReceivedEventArgs()
        {

        }

        /// <summary>
        ///     Gets the message received.
        /// </summary>
        /// <returns>An new instance of the message received.</returns>
        public Message GetMessage()
        {
            return message.Clone();
        }

        /// <summary>
        ///     Recycles this object back into the pool.
        /// </summary>
        public void Dispose()
        {
            ServerObjectCache.ReturnServerMessageReceivedEventArgs(this);
            isCurrentlyLoungingInAPool = true;
        }

        /// <summary>
        ///     Finalizer so we can inform the cache system we were not recycled correctly.
        /// </summary>
        ~ServerMessageReceivedEventArgs()
        {
            if (!isCurrentlyLoungingInAPool)
                ServerObjectCacheHelper.ServerMessageReceivedEventArgsWasFinalized();
        }
    }
#endif
}
