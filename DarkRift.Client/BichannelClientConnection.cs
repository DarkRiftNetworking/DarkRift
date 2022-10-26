/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DarkRift.Client
{
    /// <summary>
    ///     A connection to a remote server and handles TCP and UDP channels.
    /// </summary>
    public sealed class BichannelClientConnection : NetworkClientConnection, IDisposable
    {
        /// <summary>
        ///     The IP address of the remote client.
        /// </summary>
        public IPEndPoint RemoteTcpEndPoint { get; }

        /// <summary>
        ///     The IP address of the remote client.
        /// </summary>
        public IPEndPoint RemoteUdpEndPoint { get; }
        
        /// <summary>
        ///     Whether Nagel's algorithm should be disabled or not.
        /// </summary>
        public bool NoDelay {
            get => tcpSocket.NoDelay;
            set => tcpSocket.NoDelay = value;
        }

        /// <inheritdoc/>
        public override IEnumerable<IPEndPoint> RemoteEndPoints => new IPEndPoint[] { RemoteTcpEndPoint, RemoteUdpEndPoint };

        /// <inheritdoc/>
        public override ConnectionState ConnectionState => connectionState;

        /// <summary>
        ///     Backing for <see cref="ConnectionState"/>.
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        private ConnectionState connectionState
#pragma warning restore IDE1006 // Naming Styles
        {
            set
            {
                lock (myLock)
                {
                    lockedConnectionState = value;
                }
            }
            get
            {
                lock (myLock)
                {
                    return lockedConnectionState;
                }
            }
        }

        private ConnectionState lockedConnectionState;
        private readonly object myLock = new object();

        /// <summary>
        ///     The socket used in TCP communication.
        /// </summary>
        private readonly Socket tcpSocket;

        /// <summary>
        ///     The socket used in UDP communication.
        /// </summary>
        private readonly Socket udpSocket;

        private SocketAsyncEventArgs tcpArgs;
        private TcpReceiveState tcpReceiveState;
        private int tcpBytesTransferred;

        private enum TcpReceiveState
        {
            ReceiveHeader,
            ReceiveBody,
        }

        /// <summary>
        ///     Creates a new bichannel client.
        /// </summary>
        /// <param name="ipAddress">The IP address of the server.</param>
        /// <param name="port">The port (UDP and TCP) the server is listening on.</param>
        /// <param name="noDelay">Whether to disable Nagle's algorithm or not.</param>
        public BichannelClientConnection(IPAddress ipAddress, int port, bool noDelay)
            : this (ipAddress, port, port, noDelay)
        {
        }

        /// <summary>
        ///     Creates a new bichannel client.
        /// </summary>
        /// <param name="ipAddress">The IP address of the server.</param>
        /// <param name="udpPort">The port the server is listening on for UDP.</param>
        /// <param name="tcpPort">The port the server is listening on for TCP.</param>
        /// <param name="noDelay">Whether to disable Nagle's algorithm or not.</param>
        public BichannelClientConnection(IPAddress ipAddress, int tcpPort, int udpPort, bool noDelay)
            : base ()
        {
            RemoteTcpEndPoint = new IPEndPoint(ipAddress, tcpPort);
            RemoteUdpEndPoint = new IPEndPoint(ipAddress, udpPort);

            tcpSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            udpSocket = new Socket(tcpSocket.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            NoDelay = noDelay;
        }

        /// <summary>
        ///     Creates a new bichannel client.
        /// </summary>
        /// <param name="ipVersion">The IP version to connect via.</param>
        /// <param name="ipAddress">The IP address of the server.</param>
        /// <param name="port">The port the server is listening on.</param>
        /// <param name="noDelay">Whether to disable Nagle's algorithm or not.</param>
        [Obsolete("Use other constructors that automatically detect the IP version.")]
        public BichannelClientConnection(IPVersion ipVersion, IPAddress ipAddress, int port, bool noDelay)
            : base()
        {
            AddressFamily addressFamily = ipVersion == IPVersion.IPv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;

            RemoteTcpEndPoint = new IPEndPoint(ipAddress, port);
            RemoteUdpEndPoint = new IPEndPoint(ipAddress, port);

            tcpSocket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
            udpSocket = new Socket(tcpSocket.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            
            NoDelay = noDelay;
        }

        /// <inheritdoc/>
        public override void Connect()
        {
            connectionState = ConnectionState.Connecting;

            try
            {
                //Connect TCP
                try
                {
                    tcpSocket.Connect(RemoteTcpEndPoint);
                }
                catch (SocketException e)
                {
                    throw new DarkRiftConnectionException("Unable to establish TCP connection to remote server.", e);
                }

                try
                {
                    //Bind UDP to a free port
                    udpSocket.Bind(new IPEndPoint(((IPEndPoint)tcpSocket.LocalEndPoint).Address, 0));
                    udpSocket.Connect(RemoteUdpEndPoint);
                }
                catch (SocketException e)
                {
                    throw new DarkRiftConnectionException("Unable to bind UDP ports.", e);
                }

                //Receive auth token from TCP
                byte[] buffer = new byte[9];
                tcpSocket.ReceiveTimeout = 5000;
                int receivedTcp = tcpSocket.Receive(buffer);
                tcpSocket.ReceiveTimeout = 0;   //Reset to infinite

                if (receivedTcp != 9 || buffer[0] != 0)
                {
                    tcpSocket.Shutdown(SocketShutdown.Both);
                    throw new DarkRiftConnectionException("Timeout waiting for auth token from server.", SocketError.ConnectionAborted);
                }

                //Transmit token back over UDP to server listening port
                udpSocket.Send(buffer);

                //Receive response from server to initiate the connection
                buffer = new byte[1];
                udpSocket.ReceiveTimeout = 5000;
                int receivedUdp = udpSocket.Receive(buffer);
                udpSocket.ReceiveTimeout = 0;   //Reset to infinite

                if (receivedUdp != 1 || buffer[0] != 0)
                {
                    tcpSocket.Shutdown(SocketShutdown.Both);
                    throw new DarkRiftConnectionException("Timeout waiting for UDP acknowledgement from server.", SocketError.ConnectionAborted);
                }
            }
            catch (DarkRiftConnectionException)
            {
                // If any exceptions get thrown reset the connection state
                connectionState = ConnectionState.Disconnected;
                throw;
            }
            catch (SocketException)
            {
                // If any exceptions get thrown reset the connection state
                connectionState = ConnectionState.Disconnected;
                throw;
            }

            //Setup the TCP socket to receive a header
            tcpArgs = ObjectCache.GetSocketAsyncEventArgs();
            tcpArgs.BufferList = null;
			SetupReceiveHeader(tcpArgs);

            //Start receiving UDP packets
            SocketAsyncEventArgs udpArgs = ObjectCache.GetSocketAsyncEventArgs();
            udpArgs.BufferList = null;
            udpArgs.SetBuffer(new byte[ushort.MaxValue], 0, ushort.MaxValue);

            udpArgs.Completed += UdpReceiveCompleted;

            bool udpCompletingAsync = udpSocket.ReceiveAsync(udpArgs);
            if (!udpCompletingAsync)
                UdpReceiveCompleted(this, udpArgs);

            //Mark connected to allow sending
            connectionState = ConnectionState.Connected;

            //Calling synchronously in game loop would probably be better as it keeps messages in socket buffer instead.

            PollingThread.AddWork(DoPolling);
        }

        /// <inheritdoc/>
        public override bool SendMessageReliable(MessageBuffer message)
        {
            if (connectionState == ConnectionState.Disconnected)
                return false;

            byte[] header = new byte[4];
            BigEndianHelper.WriteBytes(header, 0, message.Count);

            SocketAsyncEventArgs args = ObjectCache.GetSocketAsyncEventArgs();
            args.SocketError = SocketError.Success;

            args.SetBuffer(null, 0, 0);
            args.BufferList = new List<ArraySegment<byte>>()
            {
                new ArraySegment<byte>(header),
                new ArraySegment<byte>(message.Buffer, message.Offset, message.Count)
            };
            args.UserToken = message;

            try
            {
                tcpSocket.Send(args.BufferList);
            }
            catch (SocketException ex)
            {
                args.SocketError = ex.SocketErrorCode;
            }
            catch (Exception)
            {
                return false;
            }

            TcpSendCompleted(args);

            return true;
        }

        /// <inheritdoc/>
        public override bool SendMessageUnreliable(MessageBuffer message)
        {
            if (connectionState == ConnectionState.Disconnected)
                return false;

            SocketAsyncEventArgs args = ObjectCache.GetSocketAsyncEventArgs();
            args.SocketError = SocketError.Success;
            args.BufferList = null;
            args.SetBuffer(message.Buffer, message.Offset, message.Count);
            args.UserToken = message;

            args.Completed += UdpSendCompleted;

            bool completingAsync;
            try
            {
                udpSocket.Send(message.Buffer, message.Offset, message.Count, SocketFlags.None);
                completingAsync = false;
            }
            catch (Exception)
            {
                return false;
            }

            if (!completingAsync)
                UdpSendCompleted(this, args);

            return true;
        }

        /// <inheritdoc/>
        public override bool Disconnect()
        {
            if (connectionState == ConnectionState.Disconnected)
                return false;

            connectionState = ConnectionState.Disconnected;

            PollingThread.RemoveWork(DoPolling);
            
            tcpSocket.Shutdown(SocketShutdown.Both);

            return true;
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

        private void DoPolling()
        {
            PollReceiveTcpHeaderAndBody();
        }

        /// <summary>
        ///     Receives TCP header followed by a TCP body. The operation
        ///     may exit early in an incomplete state.
        /// </summary>
        private void PollReceiveTcpHeaderAndBody()
        {
            var args = tcpArgs;

            while (true)
            {
                if (tcpReceiveState == TcpReceiveState.ReceiveHeader)
                {
                    if (!PollReceiveTcpNonBlocking(args))
                        return;

                    int bodyLength = ProcessHeader(args);
                    SetupReceiveBody(args, bodyLength);
                }

                if (tcpReceiveState == TcpReceiveState.ReceiveBody)
                {
                    if (!PollReceiveTcpNonBlocking(args))
                        return;

                    try
                    {
                        MessageBuffer bodyBuffer = ProcessBody(args);
                        ProcessMessage(bodyBuffer);
                    }
                    finally
                    {
                        SetupReceiveHeader(tcpArgs);
                    }
                }
            }
        }

        private bool IsReceiveComplete(SocketAsyncEventArgs args)
        {
            if (tcpBytesTransferred == 0)
                return false;

            MessageBuffer buffer = (MessageBuffer)args.UserToken;

            return args.Offset + tcpBytesTransferred - buffer.Offset >= buffer.Count;
        }

        private bool PollReceiveTcpNonBlocking(SocketAsyncEventArgs args)
        {
            while (!IsReceiveComplete(args))
            {
                UpdateBufferPointers(args);

                int bytesAvailable = tcpSocket.Available;

                if (bytesAvailable == 0)
                    return false;

                try
                {
                    tcpBytesTransferred = tcpSocket.Receive(args.Buffer, args.Offset, Math.Min(bytesAvailable, args.Count), SocketFlags.None);
                }
                catch (ObjectDisposedException)
                {
                    HandleDisconnectionDuringTcpReceive(args);
                    return false;
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock)
                        return false;

                    args.SocketError = ex.SocketErrorCode;
                    HandleDisconnectionDuringTcpReceive(args);
                    return false;
                }

                if (tcpBytesTransferred == 0)
                    return false;
            }

            return true;
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

            return bodyLength;
        }

        /// <summary>
        ///     Processes a TCP body received.
        /// </summary>
        /// <param name="args">The socket args used during the operation.</param>
        /// <returns>The buffer received.</returns>
        private MessageBuffer ProcessBody(SocketAsyncEventArgs args)
        {
            return (MessageBuffer)args.UserToken;
        }

        /// <summary>
        ///     Invokes message recevied events and cleans up.
        /// </summary>
        /// <param name="buffer">The TCP body received.</param>
        private void ProcessMessage(MessageBuffer buffer)
        {
            HandleMessageReceived(buffer, SendMode.Reliable);

            buffer.Dispose();
        }

        /// <summary>
        ///     Handles a disconnection while receiving a TCP header.
        /// </summary>
        /// <param name="args">The socket args used during the operation.</param>
        private void HandleDisconnectionDuringTcpReceive(SocketAsyncEventArgs args)
        {
            Disconnect(args.SocketError);
        }

        /// <summary>
        ///     Setup a listen operation for a new TCP header.
        /// </summary>
        /// <param name="args">The socket args to use during the operation.</param>
        private void SetupReceiveHeader(SocketAsyncEventArgs args)
        {
            tcpBytesTransferred = 0;
            tcpReceiveState = TcpReceiveState.ReceiveHeader;

            MessageBuffer headerBuffer = MessageBuffer.Create(4);

            args.SetBuffer(headerBuffer.Buffer, headerBuffer.Offset, 4);
            args.UserToken = headerBuffer;
        }

        /// <summary>
        ///     Setup a listen operation for a new TCP body.
        /// </summary>
        /// <param name="args">The socket args to use during the operation.</param>
        /// <param name="length">The number of bytes in the body.</param>
        private void SetupReceiveBody(SocketAsyncEventArgs args, int length)
        {
            tcpBytesTransferred = 0;
            tcpReceiveState = TcpReceiveState.ReceiveBody;

            MessageBuffer bodyBuffer = MessageBuffer.Create(length);
            bodyBuffer.Count = length;

            args.SetBuffer(bodyBuffer.Buffer, bodyBuffer.Offset, length);
            args.UserToken = bodyBuffer;
        }

        /// <summary>
        ///     Updates the pointers on the buffer to continue a receive operation.
        /// </summary>
        /// <param name="args">The socket args to update.</param>
        private void UpdateBufferPointers(SocketAsyncEventArgs args) {
            args.SetBuffer(args.Offset + tcpBytesTransferred, args.Count - tcpBytesTransferred);
        }

        /// <summary>
        ///     Called when a UDP message arrives.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UdpReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            bool completingAsync;
            do
            {
                //If we received a Success then process it
                if (e.SocketError == SocketError.Success)
                {
                    using (MessageBuffer buffer = MessageBuffer.Create(e.BytesTransferred))
                    {
                        Buffer.BlockCopy(e.Buffer, 0, buffer.Buffer, buffer.Offset, e.BytesTransferred);
                        buffer.Count = e.BytesTransferred;

                        completingAsync = udpSocket.ReceiveAsync(e);

                        //Length of 0 must be a hole punching packet
                        if (buffer.Count != 0)
                            HandleMessageReceived(buffer, SendMode.Unreliable);
                    }
                }

                //Ignore ConnectionReset (ICMP Port Unreachable) since NATs will return that when they get 
                //the punchthrough packets and they've not already been opened
                else if (e.SocketError == SocketError.ConnectionReset)
                {
                    completingAsync = udpSocket.ReceiveAsync(e);
                }

                //Anything else is probably bad news
                else
                {
                    Disconnect(e.SocketError);

                    e.Completed -= UdpReceiveCompleted;
                    ObjectCache.ReturnSocketAsyncEventArgs(e);

                    // Leave the loop
                    return;
                }

            } while (!completingAsync);
        }

        /// <summary>
        ///     Called when a TCP send has completed.
        /// </summary>
        /// <param name="e"></param>
        private void TcpSendCompleted(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
                Disconnect(e.SocketError);

            //Always dispose buffer when completed!
            ((MessageBuffer)e.UserToken).Dispose();

            ObjectCache.ReturnSocketAsyncEventArgs(e);
        }

        /// <summary>
        ///     Called when a UDP send has completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UdpSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
                Disconnect(e.SocketError);

            e.Completed -= UdpSendCompleted;

            //Always dispose buffer when completed!
            ((MessageBuffer)e.UserToken).Dispose();

            ObjectCache.ReturnSocketAsyncEventArgs(e);
        }

        /// <summary>
        ///     Called when a socket error has occured.
        /// </summary>
        /// <param name="error">The error causing the disconnect.</param>
        private void Disconnect(SocketError error)
        {
            if (connectionState == ConnectionState.Connected)
            {
                connectionState = ConnectionState.Disconnected;

                PollingThread.RemoveWork(DoPolling);

                HandleDisconnection(error);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        ///     Disposes of the connection.
        /// </summary>
        /// <param name="disposing">Whether the object is bing disposed or not.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(true);

            if (!disposedValue)
            {
                if (disposing)
                {
                    Disconnect();

                    tcpSocket.Close();
                    udpSocket.Close();

                    if (tcpArgs != null)
                    {
                        MessageBuffer buffer = (MessageBuffer)tcpArgs.UserToken;
                        buffer.Dispose();

                        ObjectCache.ReturnSocketAsyncEventArgs(tcpArgs);
                        tcpArgs = null;
                    }
                }

                disposedValue = true;
            }
        }
        #endregion
    }
}
