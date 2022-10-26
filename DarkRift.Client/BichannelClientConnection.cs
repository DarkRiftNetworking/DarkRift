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
            get => tcp.Socket.NoDelay;
            set => tcp.Socket.NoDelay = value;
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

        private readonly SynchronousTcpSocket tcp;
        private readonly SynchronousUdpSocket udp;

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

            var tcpSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            tcp = new SynchronousTcpSocket(tcpSocket, Disconnect, HandleMessageReceived);
            var udpSocket = new Socket(tcpSocket.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            udp = new SynchronousUdpSocket(udpSocket, Disconnect, HandleMessageReceived);

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

            var tcpSocket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
            tcp = new SynchronousTcpSocket(tcpSocket, Disconnect, HandleMessageReceived);
            var udpSocket = new Socket(tcpSocket.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            udp = new SynchronousUdpSocket(udpSocket, Disconnect, HandleMessageReceived);

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
                    tcp.Socket.Connect(RemoteTcpEndPoint);
                }
                catch (SocketException e)
                {
                    throw new DarkRiftConnectionException("Unable to establish TCP connection to remote server.", e);
                }

                try
                {
                    //Bind UDP to a free port
                    udp.Socket.Bind(new IPEndPoint(((IPEndPoint)tcp.Socket.LocalEndPoint).Address, 0));
                    udp.Socket.Connect(RemoteUdpEndPoint);
                }
                catch (SocketException e)
                {
                    throw new DarkRiftConnectionException("Unable to bind UDP ports.", e);
                }

                //Receive auth token from TCP
                byte[] buffer = new byte[9];
                tcp.Socket.ReceiveTimeout = 5000;
                int receivedTcp = tcp.Socket.Receive(buffer);
                tcp.Socket.ReceiveTimeout = 0;   //Reset to infinite

                if (receivedTcp != 9 || buffer[0] != 0)
                {
                    tcp.Shutdown();
                    throw new DarkRiftConnectionException("Timeout waiting for auth token from server.", SocketError.ConnectionAborted);
                }

                //Transmit token back over UDP to server listening port
                udp.Socket.Send(buffer);

                //Receive response from server to initiate the connection
                buffer = new byte[1];
                udp.Socket.ReceiveTimeout = 5000;
                int receivedUdp = udp.Socket.Receive(buffer);
                udp.Socket.ReceiveTimeout = 0;   //Reset to infinite

                if (receivedUdp != 1 || buffer[0] != 0)
                {
                    tcp.Shutdown();
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

            tcp.ResetBuffers();
            udp.ResetBuffers();

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

            return tcp.SendMessageReliable(message);
        }

        /// <inheritdoc/>
        public override bool SendMessageUnreliable(MessageBuffer message)
        {
            if (connectionState == ConnectionState.Disconnected)
                return false;

            return udp.SendMessageUnreliable(message);
        }

        /// <inheritdoc/>
        public override bool Disconnect()
        {
            if (connectionState == ConnectionState.Disconnected)
                return false;

            connectionState = ConnectionState.Disconnected;

            PollingThread.RemoveWork(DoPolling);
            
            tcp.Shutdown();

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
            tcp.PollReceiveHeaderAndBodyNonBlocking();
            udp.PollReceiveBodyNonBlocking();
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

                    tcp.Socket.Close();
                    udp.Socket.Close();

                    tcp.Dispose();
                    udp.Dispose();
                }

                disposedValue = true;
            }
        }
        #endregion
    }
}
