using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace DarkRift
{
    internal class SynchronousUdpSocket : IDisposable
    {
        public Socket Socket { get; private set; }

        private readonly Action<SocketError> disconnect;
        private readonly Action<MessageBuffer, SendMode> handleMessage;

        private SocketAsyncEventArgs udpArgs;
        private bool disposedValue;

        public SynchronousUdpSocket(Socket socket, Action<SocketError> disconnect, Action<MessageBuffer, SendMode> handleMessage)
        {
            Socket = socket;
            this.disconnect = disconnect;
            this.handleMessage = handleMessage;

            //TODO: be able to call ResetBuffers()
        }

        public void ResetBuffers()
        {
            udpArgs = ObjectCache.GetSocketAsyncEventArgs();
            udpArgs.BufferList = null;
            udpArgs.SetBuffer(new byte[ushort.MaxValue], 0, ushort.MaxValue);
        }

        public bool SendMessageUnreliable(MessageBuffer message)
        {
            SocketAsyncEventArgs args = ObjectCache.GetSocketAsyncEventArgs();
            args.SocketError = SocketError.Success;
            args.BufferList = null;
            args.SetBuffer(message.Buffer, message.Offset, message.Count);
            args.UserToken = message;

            try
            {
                Socket.Send(message.Buffer, message.Offset, message.Count, SocketFlags.None);
            }
            catch (Exception)
            {
                return false;
            }

            SendCompleted(args);

            return true;
        }

        /// <summary>
        ///     Called when a UDP message arrives.
        /// </summary>
        public void PollReceiveBodyNonBlocking()
        {
            var args = udpArgs;

            while (true)
            {
                int bytesAvailable = Socket.Available;
                if (bytesAvailable == 0)
                    return;

                int bytesTransferred;
                try
                {
                    args.SocketError = SocketError.Success;
                    bytesTransferred = Socket.Receive(args.Buffer, ushort.MaxValue, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        //Ignore ConnectionReset (ICMP Port Unreachable) since NATs will return that when they get 
                        //the punchthrough packets and they've not already been opened
                        return;
                    }

                    args.SocketError = ex.SocketErrorCode;
                    disconnect(args.SocketError);
                    return;
                }

                using (MessageBuffer buffer = MessageBuffer.Create(bytesTransferred))
                {
                    Buffer.BlockCopy(args.Buffer, 0, buffer.Buffer, buffer.Offset, bytesTransferred);
                    buffer.Count = bytesTransferred;

                    //Length of 0 must be a hole punching packet
                    if (buffer.Count != 0)
                        handleMessage(buffer, SendMode.Unreliable);
                }
            }
        }

        /// <summary>
        ///     Called when a TCP or UDP send has completed.
        /// </summary>
        /// <param name="e"></param>
        private void SendCompleted(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
                disconnect(e.SocketError);

            //Always dispose buffer when completed!
            ((MessageBuffer)e.UserToken).Dispose();

            ObjectCache.ReturnSocketAsyncEventArgs(e);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (udpArgs != null)
                    {
                        udpArgs.SetBuffer(null, 0, 0);
                        ObjectCache.ReturnSocketAsyncEventArgs(udpArgs);
                        udpArgs = null;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SynchronousUdpSocket()
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
