/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace DarkRift.Server
{
#if PRO
    internal sealed class PendingDownstreamRemoteServer
    {
        /// <summary>
        ///     The connection to the remote server.
        /// </summary>
        /// <remarks>
        ///     Will change reference on reconnections. Currently this is not marked volatile as that is a very exceptional circumstance and at that point
        ///     was can likely tolerate just waiting for something else to synchronise caches later.
        /// </remarks>
        internal NetworkServerConnection Connection { get; }

        /// <summary>
        ///     Delegate invoked if the remote server identifies itself.
        /// </summary>
        public Action<PendingDownstreamRemoteServer, ushort> Ready { get; }

        /// <summary>
        ///     Delegate invoked if the connection is dropped.
        /// </summary>
        public Action<PendingDownstreamRemoteServer> Dropped { get; }

        /// <summary>
        ///     Queue of messages accumulated before the server identifed itself and was assigned to the correct remote server.
        /// </summary>
        private readonly Queue<QueuedMessage> queuedMessages = new Queue<QueuedMessage>();

        /// <summary>
        ///     Timer used to timeout connections.
        /// </summary>
        private readonly System.Threading.Timer timer;

        /// <summary>
        ///     The logger to use.
        /// </summary>
        private readonly Logger logger;

        /// <summary>
        ///     Whether the connection has been identified/dropped yet. Locked on timer.
        /// </summary>
        private bool completed;

        /// <summary>
        ///     Holds a message that received before the connection was assigned to a remote server.
        /// </summary>
        public struct QueuedMessage
        {
            /// <summary>
            ///     The message received.
            /// </summary>
            public Message Message { get; set;  }

            /// <summary>
            ///     The send mode used.
            /// </summary>
            public SendMode SendMode { get; set; }
        }

        /// <summary>
        ///     Creates a new remote server.
        /// </summary>
        /// <param name="connection">The connection to the server.</param>
        /// <param name="timeoutMs">The number of milliseconds to wait before timing out.</param>
        /// <param name="ready">Delegate invoked if the connection is dropped.</param>
        /// <param name="dropped">Delegate invoked if the remote server identifies itself.</param>
        /// <param name="logger">The logger to use.</param>
        internal PendingDownstreamRemoteServer(NetworkServerConnection connection, int timeoutMs, Action<PendingDownstreamRemoteServer, ushort> ready, Action<PendingDownstreamRemoteServer> dropped, Logger logger)
        {
            this.Connection = connection;
            this.Ready = ready;
            this.Dropped = dropped;
            this.logger = logger;

            connection.MessageReceived += MessageReceivedHandler;
            connection.Disconnected += DisconnectedHandler;

            // Wait until we get a message with their ID
            timer = new System.Threading.Timer((_) => DropConnection(), null, timeoutMs, Timeout.Infinite);
        }

        /// <summary>
        /// Retreives the messages that have been queued while this server was pending.
        /// </summary>
        /// <returns>The queued messages.</returns>
        internal List<QueuedMessage> GetQueuedMessages()
        {
            lock (queuedMessages)
                return queuedMessages.ToList();
        }

        /// <summary>
        ///     Callback for when data is received.
        /// </summary>
        /// <param name="buffer">The data recevied.</param>
        /// <param name="sendMode">The SendMode used to send the data.</param>
        private void MessageReceivedHandler(MessageBuffer buffer, SendMode sendMode)
        {
            using (Message message = Message.Create(buffer, true))
            {
                if (message.IsCommandMessage)
                {
                    HandleCommand(message);
                }
                else
                {
                    lock (queuedMessages)
                        queuedMessages.Enqueue(new QueuedMessage { Message = message.Clone(), SendMode = sendMode });
                }
            }
        }

        /// <summary>
        ///     Handles a command received.
        /// </summary>
        /// <param name="message">The message that was received.</param>
        private void HandleCommand(Message message)
        {
            switch ((CommandCode)message.Tag)
            {
                case CommandCode.Identify:
                    HandleIdentification(message);
                    break;

                default:
                    logger.Warning("Pending server sent a command message before identifiying itself. The connection was dropped.");

                    DropConnection();
                    break;
            }
        }

        /// <summary>
        ///     Handles an identification command message.
        /// </summary>
        /// <param name="message">The message received.</param>
        private void HandleIdentification(Message message)
        {
            Complete();

            ushort id;
            using (DarkRiftReader reader = message.GetReader())
                id = reader.ReadUInt16();

            Ready.Invoke(this, id);
        }

        private void DisconnectedHandler(SocketError error, Exception exception)
        {
            Complete();

            Dropped.Invoke(this);
        }

        /// <summary>
        ///     Disconnects the connection without calling back to the client manager.
        /// </summary>
        private bool DropConnection()
        {
            if (!Complete())
                return false;

            Dropped.Invoke(this);

            return Connection.Disconnect();
        }

        /// <summary>
        /// Marks this pending connection as no longer pending.
        /// </summary>
        /// <returns>True, if it was able to complete.</returns>
        private bool Complete()
        {
            lock (timer)
            {
                if (completed)
                    return false;

                completed = true;
                timer.Dispose();
            }

            return true;
        }

        private bool disposed = false;

        /// <summary>
        ///     Disposes of the connection.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Handles disposing of the connection.
        /// </summary>
        /// <param name="disposing"></param>
#pragma warning disable CS0628
        protected void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                disposed = true;

                if (Connection != null)
                    Connection.Dispose();
            }
        }
#pragma warning restore CS0628
    }
#endif
}
