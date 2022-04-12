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
using System.Text;
using System.Threading;

namespace DarkRift.Server.Plugins.Listeners.Bichannel
{
    internal abstract class BichannelListenerBase : AbstractBichannelListener
    {
        /// <summary>
        ///     The TCP listening socket.
        /// </summary>
        protected Socket TcpListener { get; }

        /// <summary>
        ///     The UDP listening socket.
        /// </summary>
        protected Socket UdpListener { get; }

        /// <summary>
        ///     The UDP port being listened on.
        /// </summary>
        public override ushort UdpPort { get; protected set; }

        /// <summary>
        ///     Whether Nagle's algorithm should be disabled.
        /// </summary>
        public override bool NoDelay {
            get => TcpListener.NoDelay;
            set => TcpListener.NoDelay = value;
        }

        /// <summary>
        ///     Dictionary of TCP connections awaiting their UDP counterpart.
        /// </summary>
        protected Dictionary<long, PendingConnection> PendingTcpSockets { get; } = new Dictionary<long, PendingConnection>();

        /// <summary>
        ///     Represents a connection to the server awaiting the UDP channel to connect.
        /// </summary>
        protected struct PendingConnection
        {
            /// <summary>
            ///     The TCP socket connected.
            /// </summary>
            public Socket TcpSocket { get; set; }

            /// <summary>
            ///     The timer for timing out the connection request.
            /// </summary>
            public System.Threading.Timer Timer { get; set; }
        }

        /// <summary>
        ///     The UDP connections to the server.
        /// </summary>
        protected Dictionary<EndPoint, BichannelServerConnection> UdpConnections { get; } = new Dictionary<EndPoint, BichannelServerConnection>();
       
        /// <summary>
        ///     The maximum size the client can ask a TCP body to be without being striked.
        /// </summary>
        /// <remarks>This defaults to 65KB.</remarks>
        public override int MaxTcpBodyLength { get; }

#if PRO
        /// <summary>
        /// Counter for the number of connections attempts that have timed out.
        /// </summary>
        private readonly ICounterMetric connectionAttemptTimeoutsCounter;
#endif

        public BichannelListenerBase(NetworkListenerLoadData listenerLoadData)
            : base(listenerLoadData)
        {
            // Only use the values from the settings element if not specified in the standardised way, for backwards compatibility
            // N.B. originally you could specify an ipVersion param but this has since been removed.
            if (this.Address == null)
            {
                this.Address = IPAddress.Parse(listenerLoadData.Settings["address"]);
                this.Port = ushort.Parse(listenerLoadData.Settings["port"]);
            }

            if (listenerLoadData.Settings["udpPort"] != null)
                this.UdpPort = ushort.Parse(listenerLoadData.Settings["udpPort"]);
            else
                this.UdpPort = this.Port;

            TcpListener = new Socket(Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            UdpListener = new Socket(Address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            this.NoDelay = listenerLoadData.Settings["noDelay"]?.ToLower() == "true";

            if (listenerLoadData.Settings["maxTcpBodyLength"] != null)
                this.MaxTcpBodyLength = int.Parse(listenerLoadData.Settings["maxTcpBodyLength"]);
            else
                this.MaxTcpBodyLength = 65535;

#if PRO
            connectionAttemptTimeoutsCounter = MetricsCollector.Counter("connection_attempt_timeouts", "The number of connection attempts made to this listener that timed out.");
#endif
        }

        /// <summary>
        ///     Binds the sockets to their ports.
        /// </summary>
        protected void BindSockets()
        {
            TcpListener.Bind(new IPEndPoint(Address, Port));
            TcpListener.Listen(100);

            // If Port was set as 0, we'll now have been assigned a port. Use that from now on
            Port = (ushort)((IPEndPoint)TcpListener.LocalEndPoint).Port;

            UdpListener.Bind(new IPEndPoint(Address, UdpPort));

            // If UdpPort was set as 0, we'll now have been assigned a port. Use that from now on
            UdpPort = (ushort)((IPEndPoint)UdpListener.LocalEndPoint).Port;
        }

        /// <summary>
        ///     Handles new TCP connections from main or fallback methods.
        /// </summary>
        /// <param name="acceptSocket">The socket accepted.</param>
        protected void HandleTcpConnection(Socket acceptSocket)
        {
            Logger.Trace("Accepted TCP connection from " + acceptSocket.RemoteEndPoint + ".");

            long token;
            lock (PendingTcpSockets)
            {
                //Generate random authentication token
                Random r = new Random();
                do
                    token = ((long)r.Next() << 32) | (long)r.Next();
                while (PendingTcpSockets.ContainsKey(token));

                //Create pending connection object
                PendingConnection pendingConnection = new PendingConnection
                {
                    TcpSocket = acceptSocket,
                    Timer = new System.Threading.Timer(ConnectionTimeoutHandler, token, 5000, Timeout.Infinite)
                };

                //Store token
                PendingTcpSockets[token] = pendingConnection;
            }

            acceptSocket.NoDelay = NoDelay;

            try
            {
                //Send token via TCP
                byte[] buffer = new byte[9];                    //Version, Token * 8
                BigEndianHelper.WriteBytes(buffer, 1, token);
                acceptSocket.Send(buffer);
            }
            catch (SocketException e)
            {
                //Failed to send auth token, cleanup
                EndPoint remoteEndPoint = CancelPendingTcpConnection(token);

                if (remoteEndPoint != null)
                    Logger.Trace("A SocketException occurred whilst sending the auth token to " + remoteEndPoint + ". It is likely the client disconnected before the server was able to perform the operation.", e);
            }
        }

        /// <summary>
        ///     Called when a connection times out due to lack the of a UDP connection.
        /// </summary>
        /// <param name="state">The token given to the connection.</param>
        private void ConnectionTimeoutHandler(object state)
        {
            EndPoint remoteEndPoint = CancelPendingTcpConnection((long)state);

            //Check found (should always be but will crash server otherwise)
            if (remoteEndPoint != null)
                Logger.Trace("Connection attempt from " + remoteEndPoint + " timed out.");

#if PRO
            connectionAttemptTimeoutsCounter.Increment();
#endif
        }

        /// <summary>
        ///     Cancels a pending TCP socket and timers.
        /// </summary>
        /// <param name="token">The identification token for the connection.</param>
        /// <returns>The endpoint associated with the connection.</returns>
        private EndPoint CancelPendingTcpConnection(long token)
        {
            lock (PendingTcpSockets)
            {
                PendingConnection connection = PendingTcpSockets[token];
                connection.Timer.Dispose();

                EndPoint endPoint = null;
                try
                {
                    endPoint = connection.TcpSocket.RemoteEndPoint;

                    connection.TcpSocket.Close();
                }
                catch (SocketException e)
                {
                    if (endPoint != null)
                        Logger.Trace("A SocketException occurred whilst cancelling the connection to " + endPoint + ". It is likely the client disconnected before the server was able to perform the operation.", e);
                    else
                        Logger.Trace("A SocketException occurred whilst cancelling a connection. It is likely the client disconnected before the server was able to perform the operation.", e);
                }

                PendingTcpSockets.Remove(token);

                return endPoint;
            }
        }


        /// <summary>
        ///     Handles a new connection to the UDP listener.
        /// </summary>
        /// <param name="buffer">The buffer sent as an entry.</param>
        /// <param name="remoteEndPoint">The originating endpoint.</param>
        protected void HandleUdpConnection(MessageBuffer buffer, EndPoint remoteEndPoint)
        {
            //Check length
            if (buffer.Count != 9)
                return;

            //Decode token
            long token = BigEndianHelper.ReadInt64(buffer.Buffer, buffer.Offset + 1);

            //Lookup TCP socket
            Socket tcpSocket;
            lock (PendingTcpSockets)
            {
                //Get connection
                if (!PendingTcpSockets.TryGetValue(token, out PendingConnection pendingConnection))
                {
                    tcpSocket = null;
                }
                else
                {
                    //Dispose timer and remove from pending list
                    pendingConnection.Timer.Dispose();
                    PendingTcpSockets.Remove(token);

                    tcpSocket = pendingConnection.TcpSocket;
                }
            }

            if (tcpSocket != null)
            {
                Logger.Trace("Accepted UDP connection from " + remoteEndPoint + ".");

                //Create connection object
                BichannelServerConnection connection = new BichannelServerConnection(
                    tcpSocket,
                    this,
                    (IPEndPoint)remoteEndPoint,
                    token
#if PRO
                    , MetricsManager.GetPerMessageMetricsCollectorFor(Name)
#endif
                );

                // Send message back to client to say hi
                // This MemoryBuffer is not supposed to be disposed here! It's disposed when the message is sent!
                MessageBuffer helloBuffer = MessageBuffer.Create(1);
                helloBuffer.Count = 1;
                helloBuffer.Buffer[0] = 0;                          // Layout: 0 - Version
                connection.SendMessageUnreliable(helloBuffer);

                //Inform everyone
                RegisterConnection(connection);
            }
            else
            {
                Logger.Trace("UDP connection from " + remoteEndPoint + " had no associated TCP connection.");
                return;
            }
        }

        /// <summary>
        ///     Subscribes a connection to receive messages.
        /// </summary>
        /// <param name="connection">The connection to subscribe.</param>
        internal void RegisterUdpConnection(BichannelServerConnection connection)
        {
            //Register for UDP messages
            lock (UdpConnections)
                UdpConnections.Add(connection.RemoteUdpEndPoint, connection);
        }

        /// <summary>
        ///     Unsubscribes a connection from receiveing messages.
        /// </summary>
        /// <param name="connection">The connection to subscribe.</param>
        internal void UnregisterUdpConnection(BichannelServerConnection connection)
        {
            //Register for UDP messages
            lock (UdpConnections)
                UdpConnections.Remove(connection.RemoteUdpEndPoint);
        }
        
        /// <summary>
        ///     Sends a buffer to the given endpoint using the UDP socket.
        /// </summary>
        /// <param name="remoteEndPoint">The end point to send to.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="completed">The function to invoke once the send is completed.</param>
        internal abstract bool SendUdpBuffer(EndPoint remoteEndPoint, MessageBuffer message, Action<int, SocketError> completed);

#region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    TcpListener.Close();
                    UdpListener.Close();
                }

                disposedValue = true;
            }
        }

#endregion
    }
}
