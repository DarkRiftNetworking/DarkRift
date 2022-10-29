/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Dispatching;
using DarkRift.Server.Metrics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DarkRift.Server.Plugins.Listeners.Bichannel
{
    /// <summary>
    ///     A connection to a remote cliente and handles TCP and UDP channels.
    /// </summary>
    internal sealed class BichannelServerConnection : NetworkServerConnection
    {
        /// <summary>
        ///     Is this client able to send or not.
        /// </summary>
        public bool CanSend
        {
            get
            {
                lock (myLock)
                    return lockedCanSend;
            }
        }

        /// <summary>
        ///     Is this client currently listening for messages or not.
        /// </summary>
        public bool IsListening
        {
            get
            {
                lock (myLock)
                    return lockedIsListening;
            }
        }

        /// <summary>
        ///     The end point of the remote client on TCP.
        /// </summary>
        public IPEndPoint RemoteTcpEndPoint { get; }

        /// <summary>
        ///     The end point of the remote client on UDP.
        /// </summary>
        public IPEndPoint RemoteUdpEndPoint { get; }
        
        /// <summary>
        ///     Whether Nagel's algorithm should be disabled or not.
        /// </summary>
        public bool NoDelay {
            get => tcp.Socket.NoDelay;
            set => tcp.Socket.NoDelay = value;
        }

        /// <summary>
        ///     The token used to authenticate this user's UDP connection.
        /// </summary>
        public long AuthToken { get; }

        /// <inheritdoc/>
        public override ConnectionState ConnectionState => CanSend ? ConnectionState.Connected : ConnectionState.Disconnected;

        /// <inheritdoc/>
        public override IEnumerable<IPEndPoint> RemoteEndPoints => new IPEndPoint[2] { RemoteTcpEndPoint, RemoteUdpEndPoint };

        /// <summary>
        ///     The listener used in UDP communication.
        /// </summary>
        private readonly BichannelListenerBase networkListener;

        /// <summary>
        /// Counter for the number of bytes sent via TCP by the listener.
        /// </summary>
        private readonly ICounterMetric bytesSentCounterTcp;

        /// <summary>
        /// Counter for the number of bytes sent via UDP by the listener.
        /// </summary>
        private readonly ICounterMetric bytesSentCounterUdp;

        /// <summary>
        /// Counter for the number of bytes received via TCP by the listener.
        /// </summary>
        private readonly ICounterMetric bytesReceivedCounterTcp;

        /// <summary>
        /// Counter for the number of bytes received via UDP by the listener.
        /// </summary>
        private readonly ICounterMetric bytesReceivedCounterUdp;

        private bool lockedIsListening;
        private bool lockedCanSend;
        private readonly object myLock = new object();

        private readonly SynchronousTcpSocket tcp;

        internal BichannelServerConnection(Socket tcpSocket, BichannelListenerBase networkListener, IPEndPoint udpEndPoint, long authToken, MetricsCollector metricsCollector)
        {
            this.tcp = new SynchronousTcpSocket(tcpSocket, UnregisterAndDisconnect, HandleTcpMessage);
            this.networkListener = networkListener;
            this.RemoteTcpEndPoint = (IPEndPoint)tcpSocket.RemoteEndPoint;
            this.RemoteUdpEndPoint = udpEndPoint;
            this.AuthToken = authToken;

            //Mark connected to allow sending
            lockedCanSend = true;

            TaggedMetricBuilder<ICounterMetric> bytesSentCounter = metricsCollector.Counter("bytes_sent", "The number of bytes sent to clients by the listener.", "protocol");
            TaggedMetricBuilder<ICounterMetric> bytesReceivedCounter = metricsCollector.Counter("bytes_received", "The number of bytes received from clients by the listener.", "protocol");
            bytesSentCounterTcp = bytesSentCounter.WithTags("tcp");
            bytesSentCounterUdp = bytesSentCounter.WithTags("udp");
            bytesReceivedCounterTcp = bytesReceivedCounter.WithTags("tcp");
            bytesReceivedCounterUdp = bytesReceivedCounter.WithTags("udp");

            tcp.CheckBodyLength = CheckTcpBodyLength;
            tcp.OnSendCompleted = TcpSendCompleted;
        }
        
        /// <summary>
        ///     Begins listening for data.
        /// </summary>
        public override void StartListening()
        {
            //tcp.Socket.Blocking = false;
            //tcp.CheckAvailable = false;

            tcp.ResetBuffers();

            //Register for UDP Messages
            networkListener.RegisterUdpConnection(this);

            //Mark as listening
            lock (myLock)
                lockedIsListening = true;

            PollingThread.AddWork(DoPolling);
        }

        /// <inheritdoc/>
        public override bool SendMessageReliable(MessageBuffer message)
        {
            if (!CanSend)
                return false;

            return tcp.SendMessageReliable(message);
        }

        /// <inheritdoc/>
        public override bool SendMessageUnreliable(MessageBuffer message)
        {
            if (!CanSend)
                return false;
            
            return networkListener.SendUdpBuffer(RemoteUdpEndPoint, message, UdpSendCompleted);
        }

        /// <summary>
        ///     Disconnects this client from the remote host.
        /// </summary>
        /// <returns>Whether the disconnect was successful.</returns>
        public override bool Disconnect()
        {
            lock (myLock)
            {
                if (!lockedCanSend && !lockedIsListening)
                    return false;
            }
            
            PollingThread.RemoveWork(DoPolling);

            try
            {
                tcp.Shutdown();
            }
            catch (SocketException)
            {
                //Ignore exception as socket is already shutdown
            }

            networkListener.UnregisterUdpConnection(this);

            lock (myLock)
            {
                lockedCanSend = false;
                lockedIsListening = false;
            }

            return true;
        }

        private void HandleTcpMessage(MessageBuffer buffer, SendMode sendMode)
        {
            this.HandleMessageReceived(buffer, sendMode);

            bytesReceivedCounterTcp.Increment(buffer.Count + 4);
        }

        /// <summary>
        ///     Handles a UDP message sent to the listener.
        /// </summary>
        internal void HandleUdpMessage(MessageBuffer buffer)
        {
            HandleMessageReceived(buffer, SendMode.Unreliable);

            bytesReceivedCounterUdp.Increment(buffer.Count);
        }

        /// <summary>
        ///     Called when a UDP send has completed.
        /// </summary>
        /// <param name="bytesSent">The number of bytes sent.</param>
        /// <param name="e">The socket error that was returned.</param>
        private void UdpSendCompleted(int bytesSent, SocketError e)
        {
            if (e != SocketError.Success)
                UnregisterAndDisconnect(e);

            bytesSentCounterUdp.Increment(bytesSent);
        }

        private void TcpSendCompleted(MessageBuffer messageBuffer)
        {
            bytesSentCounterTcp.Increment(messageBuffer.Count + 4);
        }

        private bool CheckTcpBodyLength(int bodyLength)
        {
            if (bodyLength >= networkListener.MaxTcpBodyLength)
            {
                Strike("TCP body length was above allowed limits.", 10);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Called when a socket error has occured.
        /// </summary>
        /// <param name="error"></param>
        private void UnregisterAndDisconnect(SocketError error)
        {
            bool canUnregister = false;
            lock (myLock)
            {
                canUnregister = lockedCanSend || lockedIsListening;
            }

            if (canUnregister)
            {
                networkListener.UnregisterUdpConnection(this);

                lock (myLock)
                {
                    lockedCanSend = false;
                    lockedIsListening = false;
                }

                HandleDisconnection(error);
            }
        }
        
        /// <inheritdoc/>
        public override IPEndPoint GetRemoteEndPoint(string name)
        {
            if (name.ToLower() == "tcp")
                return RemoteTcpEndPoint;
            else if (name.ToLower() == "udp")
                return RemoteUdpEndPoint;
            else
                throw new ArgumentException("Endpoint name must either be TCP or UDP");
        }

        /// <summary>
        /// Explicitly performs a step of message polling.
        /// </summary>
        public void DoPolling()
        {
            tcp.PollReceiveHeaderAndBodyNonBlocking();
        }

#region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    bool shouldDisconnect = false;
                    lock (myLock)
                    {
                        shouldDisconnect = lockedIsListening || lockedCanSend;
                    }

                    if (shouldDisconnect)
                        Disconnect();

                    PollingThread.RemoveWork(DoPolling);

                    tcp.Dispose();
                }

                disposedValue = true;
            }
        }
        
#endregion
    }
}
