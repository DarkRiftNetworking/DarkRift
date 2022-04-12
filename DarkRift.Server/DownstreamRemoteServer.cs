/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Server.Metrics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace DarkRift.Server
{
#if PRO
    internal sealed class DownstreamRemoteServer : IRemoteServer, IDisposable
    {
        /// <inheritdoc />
        public event EventHandler<ServerMessageReceivedEventArgs> MessageReceived;

        /// <inheritdoc />
        public event EventHandler<ServerConnectedEventArgs> ServerConnected;

        /// <inheritdoc />
        public event EventHandler<ServerDisconnectedEventArgs> ServerDisconnected;

        /// <inheritdoc />
        public ConnectionState ConnectionState => connection?.ConnectionState ?? ConnectionState.Disconnected;

        /// <inheritdoc />
        public IEnumerable<IPEndPoint> RemoteEndPoints => connection?.RemoteEndPoints ?? new IPEndPoint[0];

        /// <inheritdoc />
        public IServerGroup ServerGroup => serverGroup;


        /// <inheritdoc />
        public ushort ID { get; }

        /// <inheritdoc />
        public string Host { get; }

        /// <inheritdoc />
        public ushort Port { get; }

        /// <inheritdoc />
        public ServerConnectionDirection ServerConnectionDirection => ServerConnectionDirection.Downstream;

        /// <summary>
        ///     The connection to the remote server.
        /// </summary>
        /// <remarks>
        ///     Will change reference on reconnections. Currently this is not marked volatile as that is a very exceptional circumstance and at that point
        ///     was can likely tolerate just waiting for something else to synchronise caches later.
        /// </remarks>
        private NetworkServerConnection connection;

        /// <summary>
        ///     The server group we are part of.
        /// </summary>
        private readonly DownstreamServerGroup serverGroup;

        /// <summary>
        ///     The thread helper to use.
        /// </summary>
        private readonly DarkRiftThreadHelper threadHelper;

        /// <summary>
        ///     The logger to use.
        /// </summary>
        private readonly Logger logger;

        /// <summary>
        ///     Counter metric of the number of messages sent.
        /// </summary>
        private readonly ICounterMetric messagesSentCounter;

        /// <summary>
        ///     Counter metric of the number of messages received.
        /// </summary>
        private readonly ICounterMetric messagesReceivedCounter;

        /// <summary>
        ///     Histogram metric of the time taken to execute the <see cref="MessageReceived"/> event.
        /// </summary>
        private readonly IHistogramMetric messageReceivedEventTimeHistogram;

        /// <summary>
        ///     Counter metric of failures executing the <see cref="MessageReceived"/> event.
        /// </summary>
        private readonly ICounterMetric messageReceivedEventFailuresCounter;

        /// <summary>
        ///     Histogram metric of time taken to execute the <see cref="ServerConnected"/> event.
        /// </summary>
        private readonly IHistogramMetric serverConnectedEventTimeHistogram;

        /// <summary>
        ///     Histogram metric of time taken to execute the <see cref="ServerDisconnected"/> event.
        /// </summary>
        private readonly IHistogramMetric serverDisconnectedEventTimeHistogram;

        /// <summary>
        ///     Counter metric of failures executing the <see cref="ServerConnected"/> event.
        /// </summary>
        private readonly ICounterMetric serverConnectedEventFailuresCounter;

        /// <summary>
        ///     Counter metric of failures executing the <see cref="ServerDisconnected"/> event.
        /// </summary>
        private readonly ICounterMetric serverDisconnectedEventFailuresCounter;

        /// <summary>
        ///     Creates a new remote server.
        /// </summary>
        /// <param name="id">The ID of the server.</param>
        /// <param name="host">The host connected to.</param>
        /// <param name="port">The port connected to.</param>
        /// <param name="group">The group the server belongs to.</param>
        /// <param name="threadHelper">The thread helper to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="metricsCollector">The metrics collector to use.</param>
        internal DownstreamRemoteServer(ushort id, string host, ushort port, DownstreamServerGroup group, DarkRiftThreadHelper threadHelper, Logger logger, MetricsCollector metricsCollector)
        {
            this.ID = id;
            this.Host = host;
            this.Port = port;
            this.serverGroup = group;
            this.threadHelper = threadHelper;
            this.logger = logger;

            messagesSentCounter = metricsCollector.Counter("messages_sent", "The number of messages sent to remote servers.");
            messagesReceivedCounter = metricsCollector.Counter("messages_received", "The number of messages received from remote servers.");
            messageReceivedEventTimeHistogram = metricsCollector.Histogram("message_received_event_time", "The time taken to execute the MessageReceived event.");
            messageReceivedEventFailuresCounter = metricsCollector.Counter("message_received_event_failures", "The number of failures executing the MessageReceived event.");
            serverConnectedEventTimeHistogram = metricsCollector.Histogram("remote_server_connected_event_time", "The time taken to execute the ServerConnected event.", "group").WithTags(group.Name);
            serverDisconnectedEventTimeHistogram = metricsCollector.Histogram("remote_server_disconnected_event_time", "The time taken to execute the ServerDisconnected event.", "group").WithTags(group.Name);
            serverConnectedEventFailuresCounter = metricsCollector.Counter("remote_server_connected_event_failures", "The number of failures executing the ServerConnected event.", "group").WithTags(group.Name);
            serverDisconnectedEventFailuresCounter = metricsCollector.Counter("remote_server_disconnected_event_failures", "The number of failures executing the ServerDisconnected event.", "group").WithTags(group.Name);
        }

        /// <summary>
        ///     Sets the connection being used by this remote server.
        /// </summary>
        /// <param name="pendingServer">The connection to switch to.</param>
        internal void SetConnection(PendingDownstreamRemoteServer pendingServer)
        {
            if (connection != null)
            {
                connection.MessageReceived -= MessageReceivedHandler;
                connection.Disconnected -= DisconnectedHandler;
            }

            connection = pendingServer.Connection;

            // Switch out message received handler from the pending server
            connection.MessageReceived = MessageReceivedHandler;
            connection.Disconnected = DisconnectedHandler;

            EventHandler<ServerConnectedEventArgs> handler = ServerConnected;
            if (handler != null)
            {
                void DoServerConnectedEvent()
                {
                    long startTimestamp = Stopwatch.GetTimestamp();

                    try
                    {
                        handler?.Invoke(this, new ServerConnectedEventArgs(this));
                    }
                    catch (Exception e)
                    {
                        serverConnectedEventFailuresCounter.Increment();

                        logger.Error("A plugin encountered an error whilst handling the ServerConnected event. The server will still be connected. (See logs for exception)", e);
                    }

                    double time = (double)(Stopwatch.GetTimestamp() - startTimestamp) / Stopwatch.Frequency;
                    serverConnectedEventTimeHistogram.Report(time);
                }

                threadHelper.DispatchIfNeeded(DoServerConnectedEvent);
            }

            // Handle all messages that had queued
            foreach (PendingDownstreamRemoteServer.QueuedMessage queuedMessage in pendingServer.GetQueuedMessages())
            {
                HandleMessage(queuedMessage.Message, queuedMessage.SendMode);

                queuedMessage.Message.Dispose();
            }
        }

        /// <summary>
        ///     Sends a message to the server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="sendMode">How the message should be sent.</param>
        /// <returns>Whether the send was successful.</returns>
        public bool SendMessage(Message message, SendMode sendMode)
        {
            bool success = connection?.SendMessage(message.ToBuffer(), sendMode) ?? false;
            if (success)
                messagesSentCounter.Increment();

            return success;
        }

        /// <summary>
        ///     Gets the endpoint with the given name.
        /// </summary>
        /// <param name="name">The name of the endpoint.</param>
        /// <returns>The end point.</returns>
        public IPEndPoint GetRemoteEndPoint(string name)
        {
            return connection?.GetRemoteEndPoint(name);
        }

        /// <summary>
        ///     Callback for when data is received.
        /// </summary>
        /// <param name="buffer">The data recevied.</param>
        /// <param name="sendMode">The SendMode used to send the data.</param>
        private void MessageReceivedHandler(MessageBuffer buffer, SendMode sendMode)
        {
            messagesReceivedCounter.Increment();

            using (Message message = Message.Create(buffer, true))
            {
                if (message.IsCommandMessage)
                    logger.Warning($"Server {ID} sent us a command message unexpectedly. This server may be configured to expect clients to connect.");

                HandleMessage(message, sendMode);
            }
        }

        /// <summary>
        ///     Handles a message received.
        /// </summary>
        /// <param name="message">The message that was received.</param>
        /// <param name="sendMode">The send mode the emssage was received with.</param>
        private void HandleMessage(Message message, SendMode sendMode)
        {
            // Get another reference to the message so 1. we can control the backing array's lifecycle and thus it won't get disposed of before we dispatch, and
            // 2. because the current message will be disposed of when this method returns.
            Message messageReference = message.Clone();

            void DoMessageReceived()
            {
                ServerMessageReceivedEventArgs args = ServerMessageReceivedEventArgs.Create(message, sendMode, this);

                long startTimestamp = Stopwatch.GetTimestamp();

                try
                {
                    MessageReceived?.Invoke(this, args);
                }
                catch (Exception e)
                {
                    messageReceivedEventFailuresCounter.Increment();

                    logger.Error("A plugin encountered an error whilst handling the MessageReceived event. (See logs for exception)", e);
                }
                finally
                {
                    // Now we've executed everything, dispose the message reference and release the backing array!
                    messageReference.Dispose();
                    args.Dispose();
                }

                double time = (double)(Stopwatch.GetTimestamp() - startTimestamp) / Stopwatch.Frequency;
                messageReceivedEventTimeHistogram.Report(time);
            }

            //Inform plugins
            threadHelper.DispatchIfNeeded(DoMessageReceived);
        }

        /// <summary>
        /// Called when the connection is lost.
        /// </summary>
        /// <param name="error">The socket error that ocurred</param>
        /// <param name="exception">The exception that ocurred.</param>
        private void DisconnectedHandler(SocketError error, Exception exception)
        {
            serverGroup.DisconnectedHandler(this, exception);

            EventHandler<ServerDisconnectedEventArgs> handler = ServerDisconnected;
            if (handler != null)
            {
                void DoServerDisconnectedEvent()
                {
                    long startTimestamp = Stopwatch.GetTimestamp();

                    try
                    {
                        handler?.Invoke(this, new ServerDisconnectedEventArgs(this, error, exception));
                    }
                    catch (Exception e)
                    {
                        serverDisconnectedEventFailuresCounter.Increment();

                        logger.Error("A plugin encountered an error whilst handling the ServerDisconnected event. (See logs for exception)", e);
                    }

                    double time = (double)(Stopwatch.GetTimestamp() - startTimestamp) / Stopwatch.Frequency;
                    serverDisconnectedEventTimeHistogram.Report(time);
                }

                threadHelper.DispatchIfNeeded(DoServerDisconnectedEvent);
            }
        }

        /// <summary>
        ///     Disconnects the connection without calling back to the server manager.
        /// </summary>
        internal bool DropConnection()
        {
            return connection.Disconnect();
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

                if (connection != null)
                    connection.Dispose();
            }
        }
#pragma warning restore CS0628
    }
#endif
}
