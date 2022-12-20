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
using System.Runtime.CompilerServices;
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
        public bool NoDelay
        {
            get => tcpSocket.NoDelay;
            set => tcpSocket.NoDelay = value;
        }

        /// <summary>
        ///     If true (default), reliable messages are delivered in order. If false, reliable messages can be delivered out of order to improve performance.
        /// </summary>
        public bool PreserveTcpOrdering { get; private set; } = true;

        /// <inheritdoc/>
        public override IEnumerable<IPEndPoint> RemoteEndPoints => new IPEndPoint[] { RemoteTcpEndPoint, RemoteUdpEndPoint };

        /// <inheritdoc/>
        public override ConnectionState ConnectionState => connectionState;

        /// <summary>
        ///     Backing for <see cref="ConnectionState"/>.
        /// </summary>
        private ConnectionState connectionState;

        /// <summary>
        ///     The socket used in TCP communication.
        /// </summary>
        private readonly Socket tcpSocket;

        /// <summary>
        ///     The socket used in UDP communication.
        /// </summary>
        private readonly Socket udpSocket;

        /// <summary>
        ///     Creates a new bichannel client.
        /// </summary>
        /// <param name="ipAddress">The IP address of the server.</param>
        /// <param name="port">The port (UDP and TCP) the server is listening on.</param>
        /// <param name="noDelay">Whether to disable Nagle's algorithm or not.</param>
        public BichannelClientConnection(IPAddress ipAddress, int port, bool noDelay)
            : this(ipAddress, port, port, noDelay)
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
            : base()
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
                tcpSocket.ReceiveTimeout = 5000; // 5s connection timeout
                int receivedTcp = tcpSocket.Receive(buffer);
                tcpSocket.ReceiveTimeout = 10000; // reset to 10s

                if (receivedTcp != 9 || buffer[0] != 0)
                {
                    Disconnect();
                    throw new DarkRiftConnectionException("Timeout waiting for auth token from server.", SocketError.ConnectionAborted);
                }

                //Transmit token back over UDP to server listening port
                udpSocket.Send(buffer);

                //Receive response from server to initiate the connection
                buffer = new byte[1];
                udpSocket.ReceiveTimeout = 5000; // 5s connection timeout
                int receivedUdp = udpSocket.Receive(buffer);
                udpSocket.ReceiveTimeout = 10000; // reset to 10s

                if (receivedUdp != 1 || buffer[0] != 0)
                {
                    Disconnect();
                    throw new DarkRiftConnectionException("Timeout waiting for UDP acknowledgement from server.", SocketError.ConnectionAborted);
                }
            }
            catch (DarkRiftConnectionException)
            {
                // If any exceptions get thrown reset the connection state
                Disconnect();
                throw;
            }
            catch (SocketException)
            {
                // If any exceptions get thrown reset the connection state
                Disconnect();
                throw;
            }

            //Setup the TCP socket to receive a header
            SocketAsyncEventArgs tcpArgs = ObjectCache.GetSocketAsyncEventArgs();
            tcpArgs.BufferList = null;

            SetupReceiveHeader(tcpArgs);
            bool headerCompletingAsync = tcpSocket.ReceiveAsync(tcpArgs);
            if (!headerCompletingAsync)
                AsyncReceiveHeaderCompleted(this, tcpArgs);

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
        }

        /// <inheritdoc/>
        public override bool SendMessageReliable(MessageBuffer message)
        {
            if (connectionState == ConnectionState.Disconnected)
                return false;

            SocketAsyncEventArgs args = ObjectCache.GetSocketAsyncEventArgs();

            args.SetBuffer(message.Buffer, message.Offset, message.Count);
            args.UserToken = message;

            args.Completed += TcpSendCompleted;

            bool completingAsync;
            try
            {
                completingAsync = tcpSocket.SendAsync(args);
            }
            catch (ObjectDisposedException)
            {
                TcpSendCompleted(this, args);
                return false;
            }
            catch (Exception)
            {
                TcpSendCompleted(this, args);
                return false;
            }

            if (!completingAsync)
                TcpSendCompleted(this, args);

            return true;
        }

        /// <inheritdoc/>
        public override bool SendMessageUnreliable(MessageBuffer message)
        {
            if (connectionState == ConnectionState.Disconnected)
                return false;

            SocketAsyncEventArgs args = ObjectCache.GetSocketAsyncEventArgs();
            args.BufferList = null;
            args.SetBuffer(message.Buffer, message.Offset, message.Count);
            args.UserToken = message;

            args.Completed += UdpSendCompleted;

            bool completingAsync;
            try
            {
                completingAsync = udpSocket.SendAsync(args);
            }
            catch (Exception)
            {
                UdpSendCompleted(this, args);
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
            try
            {
                tcpSocket.Shutdown(SocketShutdown.Both);
                udpSocket.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException)
            {
                //Ignore exception as socket is already shutdown
            }
            finally
            {
                tcpSocket.Close();
                udpSocket.Close();
            }
            
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

        /// <summary>
        ///     Receives TCP header followed by a TCP body, looping the operation becomes asynchronous.
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

                    try
                    {
                        // header not fully received yet, wait for it
                        bool headerContinueCompletingAsync = tcpSocket.ReceiveAsync(args);
                        if (headerContinueCompletingAsync)
                            return;
                    }
                    catch (ObjectDisposedException)
                    {
                        HandleDisconnectionDuringHeaderReceive(args);
                        return;
                    }
                    catch (Exception)
                    {
                        HandleDisconnectionDuringHeaderReceive(args);
                        return;
                    }

                    // keep getting header in a loop until received
                    continue;
                }

                int bodyLength = ProcessHeader(args);

                SetupReceiveBody(args, bodyLength);
                while (true)
                {
                    try
                    {
                        bool bodyCompletingAsync = tcpSocket.ReceiveAsync(args);
                        if (bodyCompletingAsync)
                            return;
                    }
                    catch (ObjectDisposedException)
                    {
                        HandleDisconnectionDuringBodyReceive(args);
                        return;
                    }
                    catch (Exception)
                    {
                        HandleDisconnectionDuringBodyReceive(args);
                        return;
                    }

                    if (!WasBodyReceiveSucessful(args))
                    {
                        HandleDisconnectionDuringBodyReceive(args);
                        return;
                    }

                    if (IsBodyReceiveComplete(args))
                        break;

                    UpdateBufferPointers(args);
                }

                // body received, process it
                MessageBuffer bodyBuffer = ProcessBody(args);

                if (PreserveTcpOrdering)
                    ProcessMessage(bodyBuffer);

                // Start next receive before invoking events
                SetupReceiveHeader(args);
                bool headerCompletingAsync;
                try
                {
                    headerCompletingAsync = tcpSocket.ReceiveAsync(args);
                }
				catch (ObjectDisposedException)
                {
                    HandleDisconnectionDuringHeaderReceive(args);
                    return;
                }
                catch (Exception)
                {
                    HandleDisconnectionDuringHeaderReceive(args);
                    return;
                }

                if (!PreserveTcpOrdering)
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

                try
                {
                    bool bodyContinueCompletingAsync = tcpSocket.ReceiveAsync(args);
                    if (bodyContinueCompletingAsync)
                        return;
                }
                catch (ObjectDisposedException)
                {
                    HandleDisconnectionDuringHeaderReceive(args);
                    return;
                }
                catch (Exception)
                {
                    HandleDisconnectionDuringHeaderReceive(args);
                    return;
                }
            }

            MessageBuffer bodyBuffer = ProcessBody(args);

            if (PreserveTcpOrdering)
                ProcessMessage(bodyBuffer);

            // Start next receive before invoking events
            SetupReceiveHeader(args);
            bool headerCompletingAsync;
            try
            {
                headerCompletingAsync = tcpSocket.ReceiveAsync(args);
            }
			catch (ObjectDisposedException)
            {
                HandleDisconnectionDuringHeaderReceive(args);
                return;
            }
            catch (Exception)
            {
                HandleDisconnectionDuringHeaderReceive(args);
                return;
            }

            if (!PreserveTcpOrdering)
                ProcessMessage(bodyBuffer);

            if (headerCompletingAsync)
                return;

            // header not recieved async, move back into main loop until no more data is present
            ReceiveHeaderAndBody(args);
        }

        /// <summary>
        ///     Checks if a TCP header was received in its entirety.
        /// </summary>
        /// <param name="args">The socket args used during the operation.</param>
        /// <returns>If the whole header has been received.</returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MethodImpl(256)]
        private bool IsHeaderReceiveComplete(SocketAsyncEventArgs args)
        {
            // header size is 4 bytes, an int
            return args.Offset + args.BytesTransferred >= Message.HEADER_RESERVED_BYTES_COUNT;
        }

        /// <summary>
        ///     Checks if a TCP body was received in its entirety.
        /// </summary>
        /// <param name="args">The socket args used during the operation.</param>
        /// <returns>If the whole body has been received.</returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MethodImpl(256)]
        private bool IsBodyReceiveComplete(SocketAsyncEventArgs args)
        {
            MessageBuffer buffer = (MessageBuffer)args.UserToken;
            return args.Offset + args.BytesTransferred >= buffer.Count;

            // DEBUG when using byte[] of exact size an not pooled MessageBuffer
            //return args.Offset + args.BytesTransferred >= args.Buffer.Length;
        }

        /// <summary>
        ///     Processes a TCP header received.
        /// </summary>
        /// <param name="args">The socket args used during the operation.</param>
        /// <returns>The number of bytes in the body.</returns>
        private int ProcessHeader(SocketAsyncEventArgs args)
        {
            args.Completed -= AsyncReceiveHeaderCompleted;

            int bodyLength = BigEndianHelper.ReadInt32(args.Buffer, args.Offset);
            return bodyLength;
        }

        /// <summary>
        ///     Processes a TCP body received.
        /// </summary>
        /// <param name="args">The socket args used during the operation.</param>
        /// <returns>The buffer received.</returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MethodImpl(256)]
        private MessageBuffer ProcessBody(SocketAsyncEventArgs args)
        {
            args.Completed -= AsyncReceiveBodyCompleted;
            return (MessageBuffer)args.UserToken;

            // DEBUG MODE: create a buffer rather then getting it from the pool
            //var buffer = MessageBuffer.Create(args.Buffer.Length);
            //buffer.Count = args.Buffer.Length;
            //Buffer.BlockCopy(args.Buffer, 0, buffer.Buffer, 0, args.Buffer.Length);
            //return buffer;
        }

        /// <summary>
        ///     Invokes message recevied events and cleans up.
        /// </summary>
        /// <param name="buffer">The TCP body received.</param>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MethodImpl(256)]
        private void ProcessMessage(MessageBuffer buffer)
        {
            HandleMessageReceived(buffer, SendMode.Reliable);
            buffer.Dispose();
        }

        /// <summary>
        ///     Checks if a TCP header was received correctly.
        /// </summary>
        /// <param name="args">The socket args used during the operation.</param>
        /// <returns>If the receive completed correctly.</returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MethodImpl(256)]
        private bool WasHeaderReceiveSucessful(SocketAsyncEventArgs args)
        {
            return args.BytesTransferred != 0 && args.SocketError == SocketError.Success;
        }

        /// <summary>
        ///     Checks if a TCP body was received correctly.
        /// </summary>
        /// <param name="args">The socket args used during the operation.</param>
        /// <returns>If the receive completed correctly.</returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            args.Completed -= AsyncReceiveHeaderCompleted;

            Disconnect(args.SocketError);

            ObjectCache.ReturnSocketAsyncEventArgs(args);
        }

        /// <summary>
        ///     Handles a disconnection while receiving a TCP body.
        /// </summary>
        /// <param name="args">The socket args used during the operation.</param>
        private void HandleDisconnectionDuringBodyReceive(SocketAsyncEventArgs args)
        {
            args.Completed -= AsyncReceiveBodyCompleted;

            MessageBuffer buffer = (MessageBuffer)args.UserToken;
            Disconnect(args.SocketError);

            ObjectCache.ReturnSocketAsyncEventArgs(args);
            buffer.Dispose();
        }

        /// <summary>
        ///     Setup a lsiten operation for a new TCP header.
        /// </summary>
        /// <param name="args">The socket args to use during the operation.</param>
        private void SetupReceiveHeader(SocketAsyncEventArgs args)
        {
            // TODO tiny alloc
            args.SetBuffer(new byte[4], 0, 4);
            args.Completed += AsyncReceiveHeaderCompleted;
        }

        /// <summary>
        ///     Setup a listen operation for a new TCP body.
        /// </summary>
        /// <param name="args">The socket args to use during the operation.</param>
        /// <param name="length">The number of bytes in the body.</param>
        private void SetupReceiveBody(SocketAsyncEventArgs args, int length)
        {
            // here we're setting the buffer count as a marker on how much data received we need exactly for the body
            MessageBuffer bodyBuffer = MessageBuffer.Create(length);
            bodyBuffer.Count = length;

            args.SetBuffer(bodyBuffer.Buffer, 0, length);
            args.UserToken = bodyBuffer;
            //Debug.Log($"Receive buffer created. Size: [{length}], Capacity: [{bodyBuffer.Buffer.Length}], Offset: [{bodyBuffer.Offset}]");

            // DEBUG: use byte[] as buffer rather then pooled MessageBuffer
            //byte[] bodyReceiveBuffer = new byte[length];
            //args.SetBuffer(bodyReceiveBuffer, 0, length);
            //Debug.Log($"Receive buffer created. Size: [{length}], Capacity: [{length}], Offset: [{args.Offset}]");

            args.Completed += AsyncReceiveBodyCompleted;
        }

        /// <summary>
        ///     Updates the pointers on the buffer to continue a receive operation.
        /// </summary>
        /// <param name="args">The socket args to update.</param>
        private void UpdateBufferPointers(SocketAsyncEventArgs args)
        {
            MessageBuffer buffer = args.UserToken as MessageBuffer;
            int newOffset = args.Offset + args.BytesTransferred;
            args.SetBuffer(newOffset, buffer.Count - newOffset);
            //Debug.Log($"Received bytes: [{args.BytesTransferred}], in buffer: [{newOffset}], remaining [{buffer.Count - newOffset}]");

            // DEBUG: use byte[] as buffer rather then pooled MessageBuffer
            // move the offset to bytes read
            //int newOffset = args.Offset + args.BytesTransferred;
            //args.SetBuffer(newOffset, args.Buffer.Length - newOffset);
            //Debug.Log($"Received bytes: [{args.BytesTransferred}], in buffer: [{newOffset}], remaining [{args.Buffer.Length - newOffset}]");
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
                    using MessageBuffer buffer = MessageBuffer.Create(e.BytesTransferred);
                    Buffer.BlockCopy(e.Buffer, 0, buffer.Buffer, buffer.Offset, e.BytesTransferred);
                    buffer.Count = e.BytesTransferred;

                    completingAsync = udpSocket.ReceiveAsync(e);

                    //Length of 0 must be a hole punching packet
                    if (buffer.Count != 0)
                        HandleMessageReceived(buffer, SendMode.Unreliable);
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
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void TcpSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            MessageBuffer messageBuffer = (MessageBuffer)args.UserToken;
            SocketError socketError = args.SocketError;

            // Linux+Mono combination can complete without sending every byte on large packets
            // this is not documented anythwere, call SendAsync again when that happens
            // except in error case that is handled later on
            int bytesTransferredTotal = args.Offset + args.BytesTransferred;
            if (socketError == SocketError.Success && bytesTransferredTotal > 0 && bytesTransferredTotal < messageBuffer.Count)
            {
                args.SetBuffer(offset: bytesTransferredTotal, count: messageBuffer.Count - bytesTransferredTotal);
                bool isCompletedAsync = tcpSocket.SendAsync(args);
                if (isCompletedAsync)
                    return;

                // otherwise it completed sync, just continue with execution as we're already in the complete callback
            }

            args.Completed -= TcpSendCompleted;

            // must save some info before we release the args into the pool and it gets reset
            int bytesTotal = messageBuffer.Count + Message.HEADER_RESERVED_BYTES_COUNT;
            int bufferSize = messageBuffer.Buffer.Length;
            int bytesTransferred = args.BytesTransferred;
            int sendSize = args.SendPacketsSendSize;
            SocketFlags flags = args.SocketFlags;

            // socket disposed first to release ref to buffer as buffer might get reused immediatly
            // if disposed with socket still holding the ref to it
            ObjectCache.ReturnSocketAsyncEventArgs(args);

            // Always dispose buffer when completed!
            messageBuffer.Dispose();

            // during receive BytesTransferred == 0 means disconnect, maybe it could mean here as well, it's not documented
            if (socketError != SocketError.Success || bytesTransferredTotal == 0)
            {
                Disconnect(socketError);
                return;
            }
        }

        /// <summary>
        ///     Called when a UDP send has completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UdpSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            e.Completed -= UdpSendCompleted;

            MessageBuffer messageBuffer = (MessageBuffer)e.UserToken;

            // socket disposed first to release ref to buffer as buffer might get reused immediatly
            // if disposed with socket still holding the ref to it
            ObjectCache.ReturnSocketAsyncEventArgs(e);

            // Always dispose buffer when completed!
            messageBuffer.Dispose();

            if (e.SocketError != SocketError.Success)
                Disconnect(e.SocketError);
        }

        /// <summary>
        ///     Called when a socket error has occured.
        /// </summary>
        /// <param name="error">The error causing the disconnect.</param>
        private void Disconnect(SocketError error)
        {
            if (connectionState == ConnectionState.Connected)
            {
                Disconnect();
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
                }

                disposedValue = true;
            }
        }
        #endregion
    }
}
