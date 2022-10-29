using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace DarkRift
{
    internal class SynchronousTcpSocket : IDisposable
    {
        public Socket Socket { get; private set; }

        private readonly Action<SocketError> disconnect;
        private readonly Action<MessageBuffer, SendMode> handleMessage;
        public Func<int, bool> CheckBodyLength { get; set; }
        public Action<MessageBuffer> OnSendCompleted { get; set; }

        private SocketAsyncEventArgs tcpArgs;
        private TcpReceiveState tcpReceiveState;
        private int tcpBytesTransferred;
        private bool disposedValue;

        private enum TcpReceiveState
        {
            ReceiveHeader,
            ReceiveBody,
        }

        public SynchronousTcpSocket(Socket socket, Action<SocketError> disconnect, Action<MessageBuffer, SendMode> handleMessage)
        {
            Socket = socket;
            this.disconnect = disconnect;
            this.handleMessage = handleMessage;

            //TODO: be able to call ResetBuffers()
        }

        public void ResetBuffers()
        {
            //Setup the TCP socket to receive a header
            tcpArgs = ObjectCache.GetSocketAsyncEventArgs();
            tcpArgs.BufferList = null;

            SetupReceiveHeader(tcpArgs);
        }

        public void Shutdown()
        {
            Socket.Shutdown(SocketShutdown.Both);
        }

        public bool SendMessageReliable(MessageBuffer message)
        {
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
                Socket.Send(args.BufferList);
            }
            catch (SocketException ex)
            {
                args.SocketError = ex.SocketErrorCode;
            }
            catch (Exception)
            {
                return false;
            }

            SendCompleted(args);

            return true;
        }

        /// <summary>
        ///     Receives TCP header followed by a TCP body. The operation
        ///     may exit early in an incomplete state.
        /// </summary>
        public void PollReceiveHeaderAndBodyNonBlocking()
        {
            var args = tcpArgs;

            while (true)
            {
                if (tcpReceiveState == TcpReceiveState.ReceiveHeader)
                {
                    if (!PollReceiveTcpNonBlocking(args))
                        return;

                    int bodyLength = ProcessHeader(args);
                    if (CheckBodyLength != null && !CheckBodyLength(bodyLength))
                    {
                        return;
                    }
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

        private bool IsTcpReceiveComplete(SocketAsyncEventArgs args)
        {
            if (tcpBytesTransferred == 0)
                return false;

            MessageBuffer buffer = (MessageBuffer)args.UserToken;

            return args.Offset + tcpBytesTransferred - buffer.Offset >= buffer.Count;
        }

        public bool CheckAvailable { get; set; } = true;

        private bool PollReceiveTcpNonBlocking(SocketAsyncEventArgs args)
        {
            while (!IsTcpReceiveComplete(args))
            {
                UpdateBufferPointers(args);

                int bytesAvailable = Socket.Available;

                if (CheckAvailable && bytesAvailable == 0)
                    return false;

                try
                {
                    args.SocketError = SocketError.Success;
                    tcpBytesTransferred = Socket.Receive(args.Buffer, args.Offset, Math.Min(bytesAvailable, args.Count), SocketFlags.None);
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
                {
                    HandleDisconnectionDuringTcpReceive(args);
                    return false;
                }
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
            handleMessage(buffer, SendMode.Reliable);

            buffer.Dispose();
        }

        /// <summary>
        ///     Handles a disconnection while receiving a TCP header.
        /// </summary>
        /// <param name="args">The socket args used during the operation.</param>
        private void HandleDisconnectionDuringTcpReceive(SocketAsyncEventArgs args)
        {
            disconnect(args.SocketError);
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
        private void UpdateBufferPointers(SocketAsyncEventArgs args)
        {
            args.SetBuffer(args.Offset + tcpBytesTransferred, args.Count - tcpBytesTransferred);
        }

        /// <summary>
        ///     Called when a TCP or UDP send has completed.
        /// </summary>
        /// <param name="e"></param>
        private void SendCompleted(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
                disconnect(e.SocketError);

            MessageBuffer messageBuffer = (MessageBuffer)e.UserToken;
            OnSendCompleted?.Invoke(messageBuffer);

            //Always dispose buffer when completed!
            messageBuffer.Dispose();

            ObjectCache.ReturnSocketAsyncEventArgs(e);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Socket.Close();

                    if (tcpArgs != null)
                    {
                        MessageBuffer buffer = (MessageBuffer)tcpArgs.UserToken;
                        buffer.Dispose();

                        ObjectCache.ReturnSocketAsyncEventArgs(tcpArgs);
                        tcpArgs = null;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SynchronousTcpSocket()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
