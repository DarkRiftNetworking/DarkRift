/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Net;

using System.Threading;
using System.Net.Sockets;
using DarkRift.Dispatching;
using System.Diagnostics;
using DarkRift.DataStructures;
using DarkRift.Server.Metrics;

namespace DarkRift.Server
{
    /// <inheritDoc />
    internal sealed class Client : IClient, IDisposable
    {
        /// <inheritdoc/>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

#if PRO
        /// <inheritdoc/>
        public event EventHandler<StrikeEventArgs> StrikeOccured;
#endif

        /// <inheritdoc/>
        public ushort ID { get; }

        /// <inheritdoc/>
        public IPEndPoint RemoteTcpEndPoint => connection.GetRemoteEndPoint("tcp");

        /// <inheritdoc/>
        public IPEndPoint RemoteUdpEndPoint => connection.GetRemoteEndPoint("udp");

        /// <inheritdoc/>
        [Obsolete("Use Client.ConnectionState instead.")]
        public bool IsConnected => connection.ConnectionState == ConnectionState.Connected;

        /// <inheritdoc/>
        public ConnectionState ConnectionState => connection.ConnectionState;

        /// <inheritdoc/>
        public byte Strikes {
            get => (byte)Thread.VolatileRead(ref strikes);
#if PRO
            set => Interlocked.Exchange(ref strikes, value);
#endif

        }

        private int strikes;

        /// <inheritdoc/>
#if PRO
        public
#else
        internal
#endif
            DateTime ConnectionTime { get; }

        /// <inheritdoc/>
#if PRO
        public
#else
        internal
#endif
        uint MessagesSent => (uint)Thread.VolatileRead(ref messagesSent);

        private int messagesSent;

        /// <inheritdoc/>
#if PRO
        public
#else
        internal
#endif
        uint MessagesPushed => (uint)Thread.VolatileRead(ref messagesPushed);

        private int messagesPushed;

        /// <inheritdoc/>
#if PRO
        public
#else
        internal
#endif
        uint MessagesReceived => (uint)Thread.VolatileRead(ref messagesReceived);

        private int messagesReceived;

        /// <inheritdoc/>
        public IEnumerable<IPEndPoint> RemoteEndPoints => connection.RemoteEndPoints;

        /// <inheritdoc/>
        public RoundTripTimeHelper RoundTripTime { get; }

        /// <summary>
        ///     The connection to the client.
        /// </summary>
        private readonly NetworkServerConnection connection;

        /// <summary>
        ///     The client manager in charge of this client.
        /// </summary>
        private readonly ClientManager clientManager;

        /// <summary>
        ///     The thread helper this client will use.
        /// </summary>
        private readonly DarkRiftThreadHelper threadHelper;

        /// <summary>
        ///     The logger this client will use.
        /// </summary>
        private readonly Logger logger;

#if PRO
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
#endif

#if PRO
        /// <summary>
        ///     Creates a new client connection with a given global identifier and the client they are connected through.
        /// </summary>
        /// <param name="connection">The connection we handle.</param>
        /// <param name="id">The ID we've been assigned.</param>
        /// <param name="clientManager">The client manager in charge of this client.</param>
        /// <param name="threadHelper">The thread helper this client will use.</param>
        /// <param name="logger">The logger this client will use.</param>
        /// <param name="metricsCollector">The metrics collector this client will use.</param>
        internal static Client Create(NetworkServerConnection connection, ushort id, ClientManager clientManager, DarkRiftThreadHelper threadHelper, Logger logger, MetricsCollector metricsCollector)
#else
        /// <summary>
        ///     Creates a new client connection with a given global identifier and the client they are connected through.
        /// </summary>
        /// <param name="connection">The connection we handle.</param>
        /// <param name="id">The ID we've been assigned.</param>
        /// <param name="clientManager">The client manager in charge of this client.</param>
        /// <param name="threadHelper">The thread helper this client will use.</param>
        /// <param name="logger">The logger this client will use.</param>
        internal static Client Create(NetworkServerConnection connection, ushort id, ClientManager clientManager, DarkRiftThreadHelper threadHelper, Logger logger)
#endif
        {
#if PRO
            Client client = new Client(connection, id, clientManager, threadHelper, logger, metricsCollector);
#else
            Client client = new Client(connection, id, clientManager, threadHelper, logger);
#endif

            client.SendID();

            return client;
        }

#if PRO
        /// <summary>
        ///     Creates a new client connection with a given global identifier and the client they are connected through.
        /// </summary>
        /// <param name="connection">The connection we handle.</param>
        /// <param name="id">The ID assigned to this client.</param>
        /// <param name="clientManager">The client manager in charge of this client.</param>
        /// <param name="threadHelper">The thread helper this client will use.</param>
        /// <param name="logger">The logger this client will use.</param>
        /// <param name="metricsCollector">The metrics collector this client will use.</param>
        private Client(NetworkServerConnection connection, ushort id, ClientManager clientManager, DarkRiftThreadHelper threadHelper, Logger logger, MetricsCollector metricsCollector)
#else
        /// <summary>
        ///     Creates a new client connection with a given global identifier and the client they are connected through.
        /// </summary>
        /// <param name="connection">The connection we handle.</param>
        /// <param name="id">The ID we've been assigned.</param>
        /// <param name="clientManager">The client manager in charge of this client.</param>
        /// <param name="threadHelper">The thread helper this client will use.</param>
        /// <param name="logger">The logger this client will use.</param>
        private Client(NetworkServerConnection connection, ushort id, ClientManager clientManager, DarkRiftThreadHelper threadHelper, Logger logger)
#endif
        {
            this.connection = connection;
            this.ID = id;
            this.clientManager = clientManager;
            this.threadHelper = threadHelper;
            this.logger = logger;

            // TODO make a UTC version of this as this is local date time
            this.ConnectionTime = DateTime.Now;

            connection.MessageReceived = HandleIncomingDataBuffer;
            connection.Disconnected = Disconnected;

            //TODO make configurable
            this.RoundTripTime = new RoundTripTimeHelper(10, 10);

#if PRO
            messagesSentCounter = metricsCollector.Counter("messages_sent", "The number of messages sent to clients.");
            messagesReceivedCounter = metricsCollector.Counter("messages_received", "The number of messages received from clients.");
            messageReceivedEventTimeHistogram = metricsCollector.Histogram("message_received_event_time", "The time taken to execute the MessageReceived event.");
            messageReceivedEventFailuresCounter = metricsCollector.Counter("message_received_event_failures", "The number of failures executing the MessageReceived event.");
#endif
        }

        /// <summary>
        ///     Sends the client their ID.
        /// </summary>
        private void SendID()
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(ID);

                using (Message command = Message.Create((ushort)CommandCode.Configure, writer))
                {
                    command.IsCommandMessage = true;
                    PushBuffer(command.ToBuffer(), SendMode.Reliable);

#if PRO
                    // Make sure we trigger the sent metric still
                    messagesSentCounter.Increment();
#endif
                }
            }
        }

        /// <summary>
        /// Starts this client's connecting listening for messages.
        /// </summary>
        internal void StartListening()
        {
            connection.StartListening();
        }

        /// <inheritdoc/>
        public bool SendMessage(Message message, SendMode sendMode)
        {
            //Send frame
            if (!PushBuffer(message.ToBuffer(), sendMode))
                return false;

            if (message.IsPingMessage)
                RoundTripTime.RecordOutboundPing(message.PingCode);

            //Increment counter
            Interlocked.Increment(ref messagesSent);
#if PRO
            messagesSentCounter.Increment();
#endif

            return true;
        }

        /// <inheritdoc/>
        public bool Disconnect()
        {
            if (!connection.Disconnect())
                return false;

            clientManager.HandleDisconnection(this, true, SocketError.Disconnecting, null);

            return true;
        }

        /// <summary>
        ///     Disconnects the connection without invoking events for plugins.
        /// </summary>
        internal bool DropConnection()
        {
            clientManager.DropClient(this);

            return connection.Disconnect();
        }

        /// <inheritdoc/>
        public IPEndPoint GetRemoteEndPoint(string name)
        {
            return connection.GetRemoteEndPoint(name);
        }

        /// <summary>
        ///     Handles a remote disconnection.
        /// </summary>
        /// <param name="error">The error that caused the disconnection.</param>
        /// <param name="exception">The exception that caused the disconnection.</param>
        private void Disconnected(SocketError error, Exception exception)
        {
            clientManager.HandleDisconnection(this, false, error, exception);
        }

        /// <summary>
        ///     Handles data that was sent from this client.
        /// </summary>
        /// <param name="buffer">The buffer that was received.</param>
        /// <param name="sendMode">The method data was sent using.</param>
        internal void HandleIncomingDataBuffer(MessageBuffer buffer, SendMode sendMode)
        {
            //Add to received message counter
            Interlocked.Increment(ref messagesReceived);
#if PRO
            messagesReceivedCounter.Increment();
#endif

            Message message;
            try
            {
                message = Message.Create(buffer, true);
            }
            catch (IndexOutOfRangeException)
            {
                Strike(StrikeReason.InvalidMessageLength, "The message received was not long enough to contain the header.", 5);
                return;
            }

            try
            {
                HandleIncomingMessage(message, sendMode);
            }
            finally
            {
                message.Dispose();
            }
        }

        /// <summary>
        ///     Handles messages that were sent from this client.
        /// </summary>
        /// <param name="message">The message that was received.</param>
        /// <param name="sendMode">The method data was sent using.</param>
        internal void HandleIncomingMessage(Message message, SendMode sendMode)
        {
            //Discard any command messages sent from the client since they shouldn't send them
            if (message.IsCommandMessage)
            {
                Strike(StrikeReason.InvalidCommand, "Received a command message from the client. Clients should not sent commands.", 5);

                return;
            }

            //Record any ping acknowledgements
            if (message.IsPingAcknowledgementMessage)
            {
                try
                {
                    RoundTripTime.RecordInboundPing(message.PingCode);
                }
                catch (KeyNotFoundException)
                {
                    Strike(StrikeReason.UnidentifiedPing, "Received a ping acknowledgement for a ping code that does not exist. This may be because too many sent pings were unanswered at the same time and so some were dropped before this response was returned.", 1);
                }
            }

            // Get another reference to the message so 1. we can control the backing array's lifecycle and thus it won't get disposed of before we dispatch, and
            // 2. because the current message will be disposed of when this method returns.
            Message messageReference = message.Clone();

            void DoMessageReceived()
            {
                MessageReceivedEventArgs args = MessageReceivedEventArgs.Create(
                    messageReference,
                    sendMode,
                    this
                );

#if PRO
                long startTimestamp = Stopwatch.GetTimestamp();
#endif
                try
                {
                    MessageReceived?.Invoke(this, args);
                }
                catch (Exception e)
                {
                    logger.Error("A plugin encountered an error whilst handling the MessageReceived event.", e);

#if PRO
                    messageReceivedEventFailuresCounter.Increment();
#endif
                    return;
                }
                finally
                {
                    // Now we've executed everything, dispose the message reference and release the backing array!
                    args.Dispose();
                    messageReference.Dispose();
                }

#if PRO
                double time = (double)(Stopwatch.GetTimestamp() - startTimestamp) / Stopwatch.Frequency;
                messageReceivedEventTimeHistogram.Report(time);
#endif
            }

            //Inform plugins
            threadHelper.DispatchIfNeeded(DoMessageReceived);
        }

        /// <summary>
        ///     Pushes a buffer to the client.
        /// </summary>
        /// <param name="buffer">The buffer to push.</param>
        /// <param name="sendMode">The method to send the data using.</param>
        /// <returns>Whether the send was successful.</returns>
        private bool PushBuffer(MessageBuffer buffer, SendMode sendMode)
        {
            if (!connection.SendMessage(buffer, sendMode))
                return false;

            Interlocked.Increment(ref messagesPushed);

            return true;
        }
        
#region Strikes

#if PRO
        /// <inheritdoc/>
        public void Strike(string message = null)
        {
            Strike(StrikeReason.PluginRequest, message, 1);
        }

        /// <inheritdoc/>
        public void Strike(string message = null, int weight = 1)
        {
            Strike(StrikeReason.PluginRequest, message, weight);
        }
#endif

        /// <summary>
        ///     Informs plugins and adds a strike to this client's record.
        /// </summary>
        /// <param name="reason">The reason for the strike.</param>
        /// <param name="message">A message describing the reason for the strike.</param>
        /// <param name="weight">The number of strikes this accounts for.</param>
        internal void Strike(StrikeReason reason, string message, int weight)
        {
#if PRO
            EventHandler<StrikeEventArgs> handler = StrikeOccured;
            if (handler != null)
            {
                StrikeEventArgs args = new StrikeEventArgs(reason, message, weight);

                void DoInvoke()
                {
                    try
                    {
                        handler.Invoke(this, args);
                    }
                    catch (Exception e)
                    {
                        logger.Error("A plugin encountered an error whilst handling the StrikeOccured event. The strike will stand. (See logs for exception)", e);
                    }
                }

                void AfterInvoke(ActionDispatcherTask t)
                {
                    if (t == null || t.Exception == null)
                    {
                        if (args.Forgiven)
                            return;
                    }

                    EnforceStrike(reason, message, args.Weight);
                }

                threadHelper.DispatchIfNeeded(DoInvoke, AfterInvoke);
            }
            else
            {
                EnforceStrike(reason, message, weight);
            }
#else
            EnforceStrike(reason, message, weight);
#endif
        }

        /// <summary>
        ///     Adds a strike to this client's record.
        /// </summary>
        /// <param name="reason">The reason for the strike.</param>
        /// <param name="message">A message describing the reason for the strike.</param>
        /// <param name="weight">The number of strikes this accounts for.</param>
        private void EnforceStrike(StrikeReason reason, string message, int weight)
        {
            int newValue = Interlocked.Add(ref strikes, weight);

            logger.Trace($"Client received strike of weight {weight} for {reason}{(message == null ? "" : ": " + message)}.");

            if (newValue >= clientManager.MaxStrikes)
            {
                Disconnect();

                logger.Info($"Client was disconnected as the total weight of accumulated strikes exceeded the allowed number ({newValue}/{clientManager.MaxStrikes}).");
            }
        }
#endregion

        /// <summary>
        ///     Disposes of this client.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

#pragma warning disable CS0628
        protected void Dispose(bool disposing)
        {
            if (disposing)
                connection.Dispose();
        }
#pragma warning restore CS0628
    }
}
