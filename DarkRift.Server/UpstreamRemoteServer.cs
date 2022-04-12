/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Client;
using DarkRift.Server.Metrics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace DarkRift.Server
{
#if PRO
    internal sealed class UpstreamRemoteServer : IRemoteServer, IDisposable
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
        public ServerConnectionDirection ServerConnectionDirection => ServerConnectionDirection.Upstream;

        /// <summary>
        ///     The connection to the remote server.
        /// </summary>
        /// <remarks>
        ///     Will change reference on reconnections. Currently this is not marked volatile as that is a very exceptional circumstance and at that point
        ///     was can likely tolerate just waiting for something else to synchronise caches later.
        /// </remarks>
        private NetworkClientConnection connection;

        /// <summary>
        ///     The remote server manager for the server.
        /// </summary>
        private readonly RemoteServerManager remoteServerManager;

        /// <summary>
        ///     The thread helper to use.
        /// </summary>
        private readonly DarkRiftThreadHelper threadHelper;

        /// <summary>
        ///     The server group we are part of.
        /// </summary>
        private readonly UpstreamServerGroup serverGroup;

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
        /// <param name="remoteServerManager">The remote server manager for the server.</param>
        /// <param name="id">The ID of the server.</param>
        /// <param name="host">The host connected to.</param>
        /// <param name="port">The port connected to.</param>
        /// <param name="group">The group the server belongs to.</param>
        /// <param name="threadHelper">The thread helper to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="metricsCollector">The metrics collector to use.</param>
        internal UpstreamRemoteServer(RemoteServerManager remoteServerManager, ushort id, string host, ushort port, UpstreamServerGroup group, DarkRiftThreadHelper threadHelper, Logger logger, MetricsCollector metricsCollector)
        {
            this.remoteServerManager = remoteServerManager;
            this.ID = id;
            this.Host = host;
            this.Port = port;
            this.serverGroup = group;
            this.threadHelper = threadHelper;
            this.logger = logger;

            messagesSentCounter = metricsCollector.Counter("messages_sent", "The number of messages sent to remote servers.", "group").WithTags(group.Name);
            messagesReceivedCounter = metricsCollector.Counter("messages_received", "The number of messages received from remote servers.", "group").WithTags(group.Name);
            messageReceivedEventTimeHistogram = metricsCollector.Histogram("message_received_event_time", "The time taken to execute the MessageReceived event.", "group").WithTags(group.Name);
            messageReceivedEventFailuresCounter = metricsCollector.Counter("message_received_event_failures", "The number of failures executing the MessageReceived event.", "group").WithTags(group.Name);
            serverConnectedEventTimeHistogram = metricsCollector.Histogram("remote_server_connected_event_time", "The time taken to execute the ServerConnected event.", "group").WithTags(group.Name);
            serverDisconnectedEventTimeHistogram = metricsCollector.Histogram("remote_server_disconnected_event_time", "The time taken to execute the ServerDisconnected event.", "group").WithTags(group.Name);
            serverConnectedEventFailuresCounter = metricsCollector.Counter("remote_server_connected_event_failures", "The number of failures executing the ServerConnected event.", "group").WithTags(group.Name);
            serverDisconnectedEventFailuresCounter = metricsCollector.Counter("remote_server_disconnected_event_failures", "The number of failures executing the ServerDisconnected event.", "group").WithTags(group.Name);
        }

        internal void Connect()
        {
            IEnumerable<IPAddress> addresses = Dns.GetHostEntry(Host).AddressList;

            // Try to connect to an IP address
            // TODO this might not reconnect to the same IP, break out option to prioritised last connected to.
            // TODO this will always try the same IP address, break out round robin option for load balancing
            this.connection = GetResultOfFirstSuccessfulInvocationOf(addresses, (address) =>
            {
               NetworkClientConnection c = serverGroup.GetConnection(address, Port);

                c.MessageReceived += MessageReceivedHandler;
                c.Disconnected += DisconnectedHandler;

                c.Connect();

                return c;
            });

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(remoteServerManager.ServerID);

                using (Message message = Message.Create((ushort)CommandCode.Identify, writer))
                {
                    message.IsCommandMessage = true;
                    SendMessage(message, SendMode.Reliable);
                }
            }

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

                        // TODO this seems bad, shouldn't we disconenct them?
                        logger.Error("A plugin encountered an error whilst handling the ServerConnected event. The server will still be connected. (See logs for exception)", e);
                    }

                    double time = (double)(Stopwatch.GetTimestamp() - startTimestamp) / Stopwatch.Frequency;
                    serverConnectedEventTimeHistogram.Report(time);
                }

                threadHelper.DispatchIfNeeded(DoServerConnectedEvent);
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
            return connection.GetRemoteEndPoint(name);
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
                    HandleCommand(message);
                else
                    HandleMessage(message, sendMode);
            }
        }

        /// <summary>
        ///     Handles a command received.
        /// </summary>
        /// <param name="message">The message that was received.</param>
        private void HandleCommand(Message message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                switch ((CommandCode)message.Tag)
                {
                    case CommandCode.Configure:
                        logger.Warning($"Server {ID} sent an unexpected command message.");
                        break;
                }
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
            serverGroup.DisconnectedHandler(connection, this, exception);

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
        ///     Invokes the given function with each element of the inbound data until an exception is not thrown.
        /// </summary>
        /// <typeparam name="T">The type of inbound data.</typeparam>
        /// <typeparam name="TResult">The type of data being returned.</typeparam>
        /// <param name="inbound">The data to test the function against.</param>
        /// <param name="function">The function to test each peice fo data against.</param>
        /// <returns>The first result.</returns>
        private TResult GetResultOfFirstSuccessfulInvocationOf<T, TResult>(IEnumerable<T> inbound, Func<T, TResult> function) where TResult : class
        {
            Exception lastException = null;
            foreach (T t in inbound)
            {
                try
                {
                    return function.Invoke(t);
                }
                catch (Exception e)
                {
                    // Do nothing
                    lastException = e;
                }
            }

            throw new InvalidOperationException("All values caused an exception to be thrown, last exception was:", lastException);
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
