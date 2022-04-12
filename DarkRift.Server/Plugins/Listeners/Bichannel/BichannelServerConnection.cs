/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Server.Metrics;
using System;
using System.Collections.Generic;
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
        public bool CanSend { get; private set; }

        /// <summary>
        ///     Is this client currently listening for messages or not.
        /// </summary>
        public bool IsListening { get; private set; }

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
            get => tcpSocket.NoDelay;
            set => tcpSocket.NoDelay = value;
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
        ///     The socket used in TCP communication.
        /// </summary>
        private readonly Socket tcpSocket;

        /// <summary>
        ///     The listener used in UDP communication.
        /// </summary>
        private readonly BichannelListenerBase networkListener;

#if PRO
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
#endif

        internal BichannelServerConnection(Socket tcpSocket, BichannelListenerBase networkListener, IPEndPoint udpEndPoint, long authToken
#if PRO
            , MetricsCollector metricsCollector
#endif
            )
        {
            this.tcpSocket = tcpSocket;
            this.networkListener = networkListener;
            this.RemoteTcpEndPoint = (IPEndPoint)tcpSocket.RemoteEndPoint;
            this.RemoteUdpEndPoint = udpEndPoint;
            this.AuthToken = authToken;

            //Mark connected to allow sending
            CanSend = true;

#if PRO
            TaggedMetricBuilder<ICounterMetric> bytesSentCounter = metricsCollector.Counter("bytes_sent", "The number of bytes sent to clients by the listener.", "protocol");
            TaggedMetricBuilder<ICounterMetric> bytesReceivedCounter = metricsCollector.Counter("bytes_received", "The number of bytes received from clients by the listener.", "protocol");
            bytesSentCounterTcp = bytesSentCounter.WithTags("tcp");
            bytesSentCounterUdp = bytesSentCounter.WithTags("udp");
            bytesReceivedCounterTcp = bytesReceivedCounter.WithTags("tcp");
            bytesReceivedCounterUdp = bytesReceivedCounter.WithTags("udp");
#endif
        }
        
        /// <summary>
        ///     Begins listening for data.
        /// </summary>
        public override void StartListening()
        {
            //Setup the TCP socket to receive a header
            SocketAsyncEventArgs tcpArgs = ObjectCache.GetSocketAsyncEventArgs();
            tcpArgs.BufferList = null;

            SetupReceiveHeader(tcpArgs);
            // TODO can throw an object disposed exception here if we connect and disconnect very quickly
            bool headerCompletingAsync = tcpSocket.ReceiveAsync(tcpArgs);
            if (!headerCompletingAsync)
                AsyncReceiveHeaderCompleted(this, tcpArgs);

            //Register for UDP Messages
            networkListener.RegisterUdpConnection(this);

            //Mark as listening
            IsListening = true;
        }

        /// <inheritdoc/>
        public override bool SendMessageReliable(MessageBuffer message)
        {
            if (!CanSend)
                return false;

            byte[] header = new byte[4];        // TODO pool!
            BigEndianHelper.WriteBytes(header, 0, message.Count);

            SocketAsyncEventArgs args = ObjectCache.GetSocketAsyncEventArgs();

            args.SetBuffer(null, 0, 0);
            args.BufferList = new List<ArraySegment<byte>>()    // TODO pooollllllll!
            {
                new ArraySegment<byte>(header),
                new ArraySegment<byte>(message.Buffer, message.Offset, message.Count)
            };
            args.UserToken = message;

            args.Completed += TcpSendCompleted;


            bool completingAsync;
            try
            {
                completingAsync = tcpSocket.SendAsync(args);
            }
            catch (Exception)
            {
                return false;
            }

            if (!completingAsync)
                TcpSendCompleted(this, args);

            return true;
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
            if (!CanSend && !IsListening)
                return false;

            try
            {
                tcpSocket.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException)
            {
                //Ignore exception as socket is already shutdown
            }

            networkListener.UnregisterUdpConnection(this);

            CanSend = false;
            IsListening = false;

            return true;
        }

        /// <summary>
        ///     Receives TCP header followed by a TCP body, looping until the operation becomes asynchronous.
        /// </summary>
        /// <param name="args">The socket args to use during the operations.</param>
        private void ReceiveHeaderAndBody(SocketAsyncEventArgs args)
        {
            while (true)
            {
                if (!WasHeaderReceiveSucessful(args))
                {
                    HandleDisconnectionDuringHeaderReceive(args);
                    return;
                }

                if (!IsHeaderReceiveComplete(args))
                {
                    UpdateBufferPointers(args);

                    bool headerContinueCompletingAsync = tcpSocket.ReceiveAsync(args);
                    if (headerContinueCompletingAsync)
                        return;

                    continue;
                }

                int bodyLength = ProcessHeader(args);
                if (bodyLength >= networkListener.MaxTcpBodyLength)
                {
                    Strike("TCP body length was above allowed limits.", 10);
                    return;
                }

                SetupReceiveBody(args, bodyLength);
                while (true)
                {
                    bool bodyCompletingAsync = tcpSocket.ReceiveAsync(args);
                    if (bodyCompletingAsync)
                        return;

                    if (!WasBodyReceiveSucessful(args))
                    {
                        HandleDisconnectionDuringBodyReceive(args);
                        return;
                    }

                    if (IsBodyReceiveComplete(args))
                        break;

                    UpdateBufferPointers(args);
                }
                
                MessageBuffer bodyBuffer = ProcessBody(args);

                // Start next receive before invoking events
                SetupReceiveHeader(args);
                bool headerCompletingAsync = tcpSocket.ReceiveAsync(args);

                ProcessMessage(bodyBuffer);

                if (headerCompletingAsync)
                    return;
            }
        }

        /// <summary>
        ///     Event handler for when a TCP header has been received.
        /// </summary>
        /// <param name="sender">The invoking object.</param>
        /// <param name="args">The socket args used during the operation.</param>
        private void AsyncReceiveHeaderCompleted(object sender, SocketAsyncEventArgs args)
        {
            //We can move straight back into main loop
            ReceiveHeaderAndBody(args);
        }

        /// <summary>
        ///     Event handler for when a TCP body has been received.
        /// </summary>
        /// <param name="sender">The invoking object.</param>
        /// <param name="args">The socket args used during the operation.</param>
        private void AsyncReceiveBodyCompleted(object sender, SocketAsyncEventArgs args)
        {
            while (true)
            {
                if (!WasBodyReceiveSucessful(args))
                {
                    HandleDisconnectionDuringBodyReceive(args);
                    return;
                }

                if (IsBodyReceiveComplete(args))
                    break;

                UpdateBufferPointers(args);

                bool bodyContinueCompletingAsync = tcpSocket.ReceiveAsync(args);
                if (bodyContinueCompletingAsync)
                    return;
            }

            MessageBuffer bodyBuffer = ProcessBody(args);

            // Start next receive before invoking events
            SetupReceiveHeader(args);
            bool headerCompletingAsync = tcpSocket.ReceiveAsync(args);

            ProcessMessage(bodyBuffer);

            if (headerCompletingAsync)
                return;

            //Now move back into main loop until no more data is present
            ReceiveHeaderAndBody(args);
        }

        /// <summary>
        ///     Checks if a TCP header was received in its entirety.
        /// </summary>
        /// <param name="args">The socket args used during the operation.</param>
        /// <returns>If the whole header has been received.</returns>
        private bool IsHeaderReceiveComplete(SocketAsyncEventArgs args)
        {
            MessageBuffer headerBuffer = (MessageBuffer)args.UserToken;

            return args.Offset + args.BytesTransferred - headerBuffer.Offset >= headerBuffer.Count;
        }

        /// <summary>
        ///     Checks if a TCP body was received in its entirety.
        /// </summary>
        /// <param name="args">The socket args used during the operation.</param>
        /// <returns>If the whole body has been received.</returns>
        private bool IsBodyReceiveComplete(SocketAsyncEventArgs args)
        {
            MessageBuffer bodyBuffer = (MessageBuffer)args.UserToken;

            return args.Offset + args.BytesTransferred - bodyBuffer.Offset >= bodyBuffer.Count;
        }

        /// <summary>
        ///     Processes a TCP header received.
        /// </summary>
        /// <param name="args">The socket args used during the operation.</param>
        /// <returns>The number of bytes in the body.</returns>
        private int ProcessHeader(SocketAsyncEventArgs args)
        {
            MessageBuffer headerBuffer = (MessageBuffer)args.UserToken;

            int bodyLength = BigEndianHelper.ReadInt32(headerBuffer.Buffer, headerBuffer.Offset);

            headerBuffer.Dispose();

            args.Completed -= AsyncReceiveHeaderCompleted;

            return bodyLength;
        }

        /// <summary>
        ///     Processes a TCP body received.
        /// </summary>
        /// <param name="args">The socket args used during the operation.</param>
        /// <returns>The buffer received.</returns>
        private MessageBuffer ProcessBody(SocketAsyncEventArgs args)
        {
            args.Completed -= AsyncReceiveBodyCompleted;

            return (MessageBuffer)args.UserToken;
        }

        /// <summary>
        ///     Invokes message recevied events and cleans up.
        /// </summary>
        /// <param name="buffer">The TCP body received.</param>
        private void ProcessMessage(MessageBuffer buffer)
        {
            HandleMessageReceived(buffer, SendMode.Reliable);

            int bytesReceived = buffer.Count;
            buffer.Dispose();

#if PRO
            bytesReceivedCounterTcp.Increment(bytesReceived + 4);
#endif
        }

        /// <summary>
        ///     Checks if a TCP header was received correctly.
        /// </summary>
        /// <param name="args">The socket args used during the operation.</param>
        /// <returns>If the receive completed correctly.</returns>
        private bool WasHeaderReceiveSucessful(SocketAsyncEventArgs args)
        {
            return args.BytesTransferred != 0 && args.SocketError == SocketError.Success;
        }

        /// <summary>
        ///     Checks if a TCP body was received correctly.
        /// </summary>
        /// <param name="args">The socket args used during the operation.</param>
        /// <returns>If the receive completed correctly.</returns>
        private bool WasBodyReceiveSucessful(SocketAsyncEventArgs args)
        {
            return args.BytesTransferred != 0 && args.SocketError == SocketError.Success;
        }

        /// <summary>
        ///     Handles a disconnection while receiving a TCP header.
        /// </summary>
        /// <param name="args">The socket args used during the operation.</param>
        private void HandleDisconnectionDuringHeaderReceive(SocketAsyncEventArgs args)
        {
            try
            {
                UnregisterAndDisconnect(args.SocketError);
            }
            finally
            {
                MessageBuffer buffer = (MessageBuffer)args.UserToken;
                buffer.Dispose();

                args.Completed -= AsyncReceiveHeaderCompleted;
                ObjectCache.ReturnSocketAsyncEventArgs(args);
            }
        }

        /// <summary>
        ///     Handles a disconnection while receiving a TCP body.
        /// </summary>
        /// <param name="args">The socket args used during the operation.</param>
        private void HandleDisconnectionDuringBodyReceive(SocketAsyncEventArgs args)
        {
            try
            {
                UnregisterAndDisconnect(args.SocketError);
            }
            finally
            {
                MessageBuffer buffer = (MessageBuffer)args.UserToken;
                buffer.Dispose();

                args.Completed -= AsyncReceiveBodyCompleted;
                ObjectCache.ReturnSocketAsyncEventArgs(args);
            }
        }

        /// <summary>
        ///     Setup a listen operation for a new TCP header.
        /// </summary>
        /// <param name="args">The socket args to use during the operation.</param>
        private void SetupReceiveHeader(SocketAsyncEventArgs args)
        {
            MessageBuffer headerBuffer = MessageBuffer.Create(4);
            headerBuffer.Count = 4;

            args.SetBuffer(headerBuffer.Buffer, headerBuffer.Offset, 4);
            args.UserToken = headerBuffer;
            args.Completed += AsyncReceiveHeaderCompleted;
        }

        /// <summary>
        ///     Setup a listen operation for a new TCP body.
        /// </summary>
        /// <param name="args">The socket args to use during the operation.</param>
        /// <param name="length">The number of bytes in the body.</param>
        private void SetupReceiveBody(SocketAsyncEventArgs args, int length)
        {
            MessageBuffer bodyBuffer = MessageBuffer.Create(length);
            bodyBuffer.Count = length;

            args.SetBuffer(bodyBuffer.Buffer, bodyBuffer.Offset, length);
            args.UserToken = bodyBuffer;
            args.Completed += AsyncReceiveBodyCompleted;
        }

        /// <summary>
        ///     Updates the pointers on the buffer to continue a receive operation.
        /// </summary>
        /// <param name="args">The socket args to update.</param>
        private void UpdateBufferPointers(SocketAsyncEventArgs args)
        {
            args.SetBuffer(args.Offset + args.BytesTransferred, args.Count - args.BytesTransferred);
        }

        /// <summary>
        ///     Handles a UDP message sent to the listener.
        /// </summary>
        internal void HandleUdpMessage(MessageBuffer buffer)
        {
            HandleMessageReceived(buffer, SendMode.Unreliable);

#if PRO
            bytesReceivedCounterUdp.Increment(buffer.Count);
#endif
        }

        /// <summary>
        ///     Called when a TCP send has completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TcpSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
                UnregisterAndDisconnect(e.SocketError);
            
            e.Completed -= TcpSendCompleted;

            MessageBuffer messageBuffer = (MessageBuffer)e.UserToken;
            int bytesSent = messageBuffer.Count;

            //Always dispose buffer when completed!
            messageBuffer.Dispose();

            ObjectCache.ReturnSocketAsyncEventArgs(e);

#if PRO
            bytesSentCounterTcp.Increment(bytesSent + 4);
#endif
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

#if PRO
            bytesSentCounterUdp.Increment(bytesSent);
#endif
        }

        /// <summary>
        ///     Called when a socket error has occured.
        /// </summary>
        /// <param name="error"></param>
        private void UnregisterAndDisconnect(SocketError error)
        {
            if (CanSend || IsListening)
            {
                networkListener.UnregisterUdpConnection(this);

                CanSend = false;
                IsListening = false;

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

#region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (IsListening || CanSend)
                        Disconnect();

                    tcpSocket.Close();
                }

                disposedValue = true;
            }
        }
        
#endregion
    }
}
