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
    [Obsolete("Use the BichannelListener instead")]
    internal sealed class CompatibilityBichannelListener : BichannelListenerBase
    {
        public override Version Version => new Version(1, 0, 0);
        
        public CompatibilityBichannelListener(NetworkListenerLoadData listenerLoadData) : base(listenerLoadData)
        {
        }

        public override void StartListening()
        {
            BindSockets();

            Logger.Trace("Starting compatibility listener.");

            TcpListener.BeginAccept(TcpAcceptCompleted, null);

            EndPoint remoteEndPoint = new IPEndPoint(Address.AddressFamily == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Any : IPAddress.Any, 0);
            byte[] buffer = new byte[ushort.MaxValue];
            UdpListener.BeginReceiveFrom(buffer, 0, ushort.MaxValue, SocketFlags.None, ref remoteEndPoint, UdpMessageReceived, buffer);

            Logger.Info($"Server mounted, listening on port {Port}{(UdpPort != Port ? "|" + UdpPort : "")}.");
            Logger.Warning("The CompatibilityBichannelListener is now obsolete an not recommended for use. Instead, you should update your configuration to use the BichannelListener instead." +
                "\n\nYou can continue using the CompatibilityBichannelListener until it is removed in a future version of DarkRift. If you are using a version of Unity that the " +
                "BichannelListener does not support you should consider upgrading your Unity version.");
        }

        /// <summary>
        ///     Called when a new client has been accepted through the fallback accept.
        /// </summary>
        /// <param name="result">The result of the accept.</param>
        private void TcpAcceptCompleted(IAsyncResult result)
        {
            Socket socket;
            try
            {
                socket = TcpListener.EndAccept(result);
            }
            catch
            {
                return;
            }

            try
            {
                HandleTcpConnection(socket);
            }
            finally
            {
                TcpListener.BeginAccept(TcpAcceptCompleted, null);
            }
        }

        /// <summary>
        ///     Called when a UDP message is received on the fallback system.
        /// </summary>
        /// <param name="result">The result of the operation.</param>
        private void UdpMessageReceived(IAsyncResult result)
        {
            EndPoint remoteEndPoint = new IPEndPoint(Address.AddressFamily == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Any : IPAddress.Any, 0);
            int bytesReceived;
            try
            {
                bytesReceived = UdpListener.EndReceiveFrom(result, ref remoteEndPoint);
            }
            catch (SocketException)
            {
                UdpListener.BeginReceiveFrom((byte[])result.AsyncState, 0, ushort.MaxValue, SocketFlags.None, ref remoteEndPoint, UdpMessageReceived, (byte[])result.AsyncState);
                return;
            }

            //Copy over buffer and remote endpoint
            using (MessageBuffer buffer = MessageBuffer.Create(bytesReceived))
            {
                Buffer.BlockCopy((byte[])result.AsyncState, 0, buffer.Buffer, buffer.Offset, bytesReceived);
                buffer.Count = bytesReceived;

                //Start listening again
                UdpListener.BeginReceiveFrom((byte[])result.AsyncState, 0, ushort.MaxValue, SocketFlags.None, ref remoteEndPoint, UdpMessageReceived, (byte[])result.AsyncState);

                //Handle message or new connection
                BichannelServerConnection connection;
                bool exists;
                lock (UdpConnections)
                    exists = UdpConnections.TryGetValue(remoteEndPoint, out connection);

                if (exists)
                    connection.HandleUdpMessage(buffer);
                else
                    HandleUdpConnection(buffer, remoteEndPoint);
            }
        }

        private struct UdpSendOperation
        {
            public Action<int, SocketError> callback;
            public MessageBuffer message;
        }

        internal override bool SendUdpBuffer(EndPoint remoteEndPoint, MessageBuffer message, Action<int, SocketError> completed)
        {
            UdpListener.BeginSendTo(message.Buffer, message.Offset, message.Count, SocketFlags.None, remoteEndPoint, UdpSendCompleted, new UdpSendOperation { callback = completed, message = message });

            return true;
        }

        private void UdpSendCompleted(IAsyncResult result)
        {
            UdpSendOperation operation = (UdpSendOperation)result.AsyncState;

            int bytesSent;
            try
            {
                bytesSent = UdpListener.EndSendTo(result);
            }
            catch (SocketException e)
            {
                operation.callback(0, e.SocketErrorCode);
                return;
            }
            finally
            {
                operation.message.Dispose();
            }

            operation.callback(bytesSent, SocketError.Success);
        }
    }
}
