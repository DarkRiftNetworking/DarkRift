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
using System.Text;
using System.Threading;

namespace DarkRift.Server.Plugins.Listeners.Bichannel
{
    /// <summary>
    ///     Listener for TCP/UDP bichannel network connections.
    /// </summary>
    internal sealed class BichannelListener : BichannelListenerBase
    {
        public override Version Version => new Version(1, 0, 0);

        /// <summary>
        ///     Creates a new network listener.
        /// </summary>
        public BichannelListener(NetworkListenerLoadData listenerLoadData)
            : base(listenerLoadData)
        {
        }

        /// <summary>
        ///     Begins accepting new connections.
        /// </summary>
        public override void StartListening()
        {
            BindSockets();

            Logger.Trace("Starting bichannel listener.");

            //Sort TCP
            SocketAsyncEventArgs tcpArgs = ObjectCache.GetSocketAsyncEventArgs();
            tcpArgs.BufferList = null;
            tcpArgs.SetBuffer(null, 0, 0);
            tcpArgs.Completed += TcpAcceptCompleted;

            bool completingAsync = TcpListener.AcceptAsync(tcpArgs);

            if (!completingAsync)
                TcpAcceptCompleted(this, tcpArgs);

            //Sort UDP
            SocketAsyncEventArgs udpArgs = ObjectCache.GetSocketAsyncEventArgs();
            udpArgs.Completed += UdpMessageReceived;

            udpArgs.RemoteEndPoint = new IPEndPoint(Address, 0);
            udpArgs.BufferList = null;
            udpArgs.SetBuffer(new byte[ushort.MaxValue], 0, ushort.MaxValue);

            completingAsync = UdpListener.ReceiveFromAsync(udpArgs);
            if (!completingAsync)
                UdpMessageReceived(this, udpArgs);

            Logger.Info($"Server mounted, listening on port {Port}{(UdpPort != Port ? "|" + UdpPort : "")}.");
        }

        /// <summary>
        ///     Called when a new client has been accepted.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TcpAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                e.Completed -= TcpAcceptCompleted;

                ObjectCache.ReturnSocketAsyncEventArgs(e);

                return;
            }

            try
            {
                HandleTcpConnection(e.AcceptSocket);
            }
            catch (Exception ex)
            {
                Logger.Warning("Failed to complete connection accept as an exception occurred. Listener will continue accepting new connections.", ex);
            }

            // Start new accept operation
            e.AcceptSocket = null;

            bool completingAsync;
            try
            {
                completingAsync = TcpListener.AcceptAsync(e);
            }
            catch (Exception ex)
            {
                Logger.Fatal("Failed to accept connections on TCP socket. The listener cannot continue.", ex);
                return;
            }

            if (!completingAsync)
                TcpAcceptCompleted(this, e);
        }

        /// <summary>
        ///     Called when a new UDP packet is received on the listening port.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UdpMessageReceived(object sender, SocketAsyncEventArgs e)
        {
            //Copy over buffer and remote endpoint
            bool completingAsync;
            do
            {
                using (MessageBuffer buffer = MessageBuffer.Create(e.BytesTransferred))
                {
                    Buffer.BlockCopy(e.Buffer, 0, buffer.Buffer, buffer.Offset, e.BytesTransferred);
                    buffer.Count = e.BytesTransferred;

                    EndPoint remoteEndPoint = e.RemoteEndPoint;

                    //Start listening again
                    try
                    {
                        completingAsync = UdpListener.ReceiveFromAsync(e);
                    }
                    catch (ObjectDisposedException)
                    {
                        return;
                    }

                    //Handle message or new connection
                    BichannelServerConnection connection;
                    bool exists;
                    lock (UdpConnections)   // TODO remove lock please
                        exists = UdpConnections.TryGetValue(remoteEndPoint, out connection);

                    if (exists)
                        connection.HandleUdpMessage(buffer);
                    else
                        HandleUdpConnection(buffer, remoteEndPoint);
                }
            }
            while (!completingAsync);
        }

        private struct UdpSendOperation
        {
            public Action<int, SocketError> callback;
            public MessageBuffer message;
        }

        internal override bool SendUdpBuffer(EndPoint remoteEndPoint, MessageBuffer message, Action<int, SocketError> completed)
        {
            SocketAsyncEventArgs args = ObjectCache.GetSocketAsyncEventArgs();
            args.BufferList = null;
            args.UserToken = new UdpSendOperation { callback = completed, message = message };
            args.SetBuffer(message.Buffer, message.Offset, message.Count);
            args.RemoteEndPoint = remoteEndPoint;
            args.Completed += UdpSendCompleted;

            bool completingAsync;
            try
            {
                completingAsync = UdpListener.SendToAsync(args);
            }
            catch (Exception e)
            {
                Logger.Warning("UDP send failed as an exception was thrown.", e);
                return false;
            }

            if (!completingAsync)
                UdpSendCompleted(this, args);

            return true;
        }

        private void UdpSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            UdpSendOperation operation = (UdpSendOperation)args.UserToken;

            operation.callback(operation.message.Count, args.SocketError);

            args.Completed -= UdpSendCompleted;

            operation.message.Dispose();

            ObjectCache.ReturnSocketAsyncEventArgs(args);
        }
    }
}
