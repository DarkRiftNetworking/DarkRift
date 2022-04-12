/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DarkRift.Client
{
    /// <summary>
    ///     The client for DarkRift connections.
    /// </summary>
    public class DarkRiftClient : IDisposable
    {
        /// <summary>
        ///     Event fired when a message is received.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        ///     Event fired when the client becomes disconnected.
        /// </summary>
        public event EventHandler<DisconnectedEventArgs> Disconnected;

        /// <summary>
        ///     The ID the client has been assigned.
        /// </summary>
        public ushort ID { get; private set; }

        /// <summary>
        ///     The state of the connection.
        /// </summary>
        public ConnectionState ConnectionState => Connection?.ConnectionState ?? ConnectionState.Disconnected;

        /// <summary>
        ///     Returns whether or not this client is connected to the server.
        /// </summary>
        [Obsolete("Use DarkRiftClient.ConnectionState instead.")]
        public bool Connected => Connection == null ? false : Connection.ConnectionState == ConnectionState.Connected;

        /// <summary>
        ///     The endpoints of the connection.
        /// </summary>
        public IEnumerable<IPEndPoint> RemoteEndPoints => Connection?.RemoteEndPoints ?? new IPEndPoint[0];

        /// <summary>
        ///     The remote end point of the connection.
        /// </summary>
        [Obsolete("Use DarkRiftClient.GetRemoteEndPoint(\"TCP\") instead.")]
        public IPEndPoint RemoteEndPoint => Connection?.GetRemoteEndPoint("TCP");

        /// <summary>
        ///     Delegate type for handling the completion of an asynchronous connect.
        /// </summary>
        /// <param name="e">The exception that occured, if any.</param>
        public delegate void ConnectCompleteHandler(Exception e);

        /// <summary>
        ///     The connection to the remote server.
        /// </summary>
        public NetworkClientConnection Connection { get; private set; }

        /// <summary>
        ///     The round trip time helper for this client.
        /// </summary>
        public RoundTripTimeHelper RoundTripTime { get; }

        /// <summary>
        ///     Mutex that is triggered once the connection is completely setup.
        /// </summary>
        private readonly ManualResetEvent setupMutex = new ManualResetEvent(false);

        /// <summary>
        ///     The recommended cache settings for clients.
        /// </summary>
        [Obsolete("Use DefaultClientCacheSettings instead.")]
        public static ObjectCacheSettings DefaultCacheSettings => DefaultClientCacheSettings;

        /// <summary>
        ///     The recommended cache settings for clients.
        /// </summary>
        //TODO DR3 rename back to DefaultCacheSettings
        public static ClientObjectCacheSettings DefaultClientCacheSettings => new ClientObjectCacheSettings {
            MaxWriters = 2,
            MaxReaders = 2,
            MaxMessages = 4,
            MaxMessageBuffers = 4,
            MaxSocketAsyncEventArgs = 32,
            MaxActionDispatcherTasks = 16,
            MaxAutoRecyclingArrays = 4,

            ExtraSmallMemoryBlockSize = 16,
            MaxExtraSmallMemoryBlocks = 2,
            SmallMemoryBlockSize = 64,
            MaxSmallMemoryBlocks = 2,
            MediumMemoryBlockSize = 256,
            MaxMediumMemoryBlocks = 2,
            LargeMemoryBlockSize = 1024,
            MaxLargeMemoryBlocks = 2,
            ExtraLargeMemoryBlockSize = 4096,
            MaxExtraLargeMemoryBlocks = 2,

            MaxMessageReceivedEventArgs = 4
    };

        /// <summary>
        ///     Creates a new DarkRiftClient object with default cache settings.
        /// </summary>
        public DarkRiftClient()
            : this (DefaultClientCacheSettings)
        {

        }

        /// <summary>
        ///     Creates a new DarkRiftClient object with specified cache settings.
        /// </summary>
        /// <param name="objectCacheSettings">The settings for the object cache.</param>
        public DarkRiftClient(ClientObjectCacheSettings objectCacheSettings)
        {
            ObjectCache.Initialize(objectCacheSettings);
            ClientObjectCache.Initialize(objectCacheSettings);

            this.RoundTripTime = new RoundTripTimeHelper(10, 10);
        }

        /// <summary>
        ///     Creates a new DarkRiftClient object with specified cache settings.
        /// </summary>
        /// <param name="maxCachedWriters">The maximum number of DarkRiftWriters to cache per thread.</param>
        /// <param name="maxCachedReaders">The maximum number of DarkRiftReaders to cache per thread.</param>
        /// <param name="maxCachedMessages">The maximum number of Messages to cache per thread.</param>
        /// <param name="maxCachedSocketAsyncEventArgs">The maximum number of SocketAsyncEventArgs to cache per thread.</param>
        /// <param name="maxActionDispatcherTasks">The maximum number of ActionDispatcherTasks to cache per thread.</param>
        [Obsolete("Use DarkRiftClient(ClientObjectCacheSettings) instead.")]
        public DarkRiftClient(int maxCachedWriters = 2, int maxCachedReaders = 2, int maxCachedMessages = 4, int maxCachedSocketAsyncEventArgs = 32, int maxActionDispatcherTasks = 16)
        {
            ObjectCacheSettings objectCacheSettings = DefaultCacheSettings;
            objectCacheSettings.MaxWriters = maxCachedWriters;
            objectCacheSettings.MaxReaders = maxCachedReaders;
            objectCacheSettings.MaxMessages = maxCachedMessages;
            objectCacheSettings.MaxSocketAsyncEventArgs = maxCachedSocketAsyncEventArgs;
            objectCacheSettings.MaxActionDispatcherTasks = maxActionDispatcherTasks;

            ObjectCache.Initialize(objectCacheSettings);

            this.RoundTripTime = new RoundTripTimeHelper(10, 10);
        }

        /// <summary>
        ///     Creates a new DarkRiftClient object with specified cache settings.
        /// </summary>
        /// <param name="objectCacheSettings">The settings for the object cache.</param>
        [Obsolete("Use DarkRiftClient(ClientObjectCacheSettings) instead.")]
        public DarkRiftClient(ObjectCacheSettings objectCacheSettings)
        {
            ObjectCache.Initialize(objectCacheSettings);

            if (objectCacheSettings is ClientObjectCacheSettings settings)
            {
                ClientObjectCache.Initialize(settings);
            }
            else
            {
                ClientObjectCacheSettings clientObjectCacheSettings = new ClientObjectCacheSettings {
                    MaxMessageReceivedEventArgs = 4
                };
                ClientObjectCache.Initialize(clientObjectCacheSettings);
            }
            
            this.RoundTripTime = new RoundTripTimeHelper(10, 10);
        }

        /// <summary>
        ///     Connects to a remote server using a <see cref="BichannelClientConnection"/>.
        /// </summary>
        /// <param name="ip">The IP address of the server.</param>
        /// <param name="port">The port of the server.</param>
        /// <param name="ipVersion">The IP version to connect using.</param>
        [Obsolete("Use other Connect overloads that automatically detect the IP version.")]
        public void Connect(IPAddress ip, int port, IPVersion ipVersion)
        {
            Connect(ip, port, ipVersion, false);
        }

        /// <summary>
        ///     Connects to a remote server using a <see cref="BichannelClientConnection"/>.
        /// </summary>
        /// <param name="ip">The IP address of the server.</param>
        /// <param name="port">The port of the server.</param>
        /// <param name="ipVersion">The IP version to connect using.</param>
        /// <param name="noDelay">Whether to disable Nagel's algorithm or not.</param>
        [Obsolete("Use other Connect overloads that automatically detect the IP version.")]
        public void Connect(IPAddress ip, int port, IPVersion ipVersion, bool noDelay)
        {
            Connect(new BichannelClientConnection(ipVersion, ip, port, noDelay));
        }

        /// <summary>
        ///     Connects to a remote server using a <see cref="BichannelClientConnection"/>.
        /// </summary>
        /// <param name="ip">The IP address of the server.</param>
        /// <param name="port">The port of the server.</param>
        /// <param name="noDelay">Whether to disable Nagel's algorithm or not.</param>
        public void Connect(IPAddress ip, int port, bool noDelay)
        {
            Connect(new BichannelClientConnection(ip, port, noDelay));
        }

        /// <summary>
        ///     Connects to a remote server using a <see cref="BichannelClientConnection"/>.
        /// </summary>
        /// <param name="ip">The IP address of the server.</param>
        /// <param name="tcpPort">The port the server is listening on for TCP.</param>
        /// <param name="udpPort">The port the server is listening on for UDP.</param>
        /// <param name="noDelay">Whether to disable Nagel's algorithm or not.</param>
        public void Connect(IPAddress ip, int tcpPort, int udpPort, bool noDelay)
        {
            Connect(new BichannelClientConnection(ip, tcpPort, udpPort, noDelay));
        }

        /// <summary>
        ///     Connects the client using the given connection.
        /// </summary>
        /// <param name="connection">The connection to use to connect to the server.</param>
        public void Connect(NetworkClientConnection connection)
        {
            setupMutex.Reset();

            if (this.Connection != null)
                this.Connection.Dispose();

            this.Connection = connection;
            connection.MessageReceived = MessageReceivedHandler;
            connection.Disconnected = DisconnectedHandler;

            connection.Connect();

            //On timeout disconnect
            if (!setupMutex.WaitOne(10000))
                Connection.Disconnect();
        }

        /// <summary>
        ///     Connects to a remote server in the background using a <see cref="BichannelClientConnection"/>.
        /// </summary>
        /// <param name="ip">The IP address of the server.</param>
        /// <param name="port">The port of the server.</param>
        /// <param name="ipVersion">The IP version to connect using.</param>
        /// <param name="callback">The callback to invoke one the connection attempt has finished.</param>
        [Obsolete("Use other ConnectInBackground overloads that automatically detect the IP version.")]
        public void ConnectInBackground(IPAddress ip, int port, IPVersion ipVersion, ConnectCompleteHandler callback = null)
        {
            ConnectInBackground(ip, port, ipVersion, false, callback);
        }

        /// <summary>
        ///     Connects to a remote server in the background using a <see cref="BichannelClientConnection"/>.
        /// </summary>
        /// <param name="ip">The IP address of the server.</param>
        /// <param name="port">The port of the server.</param>
        /// <param name="ipVersion">The IP version to connect using.</param>
        /// <param name="callback">The callback to invoke one the connection attempt has finished.</param>
        /// <param name="noDelay">Whether to disable Nagel's algorithm or not.</param>
        [Obsolete("Use other ConnectInBackground overloads that automatically detect the IP version.")]
        public void ConnectInBackground(IPAddress ip, int port, IPVersion ipVersion, bool noDelay, ConnectCompleteHandler callback = null)
        {
            ConnectInBackground(new BichannelClientConnection(ipVersion, ip, port, noDelay), callback);
        }

        /// <summary>
        ///     Connects to a remote server in the background using a <see cref="BichannelClientConnection"/>.
        /// </summary>
        /// <param name="ip">The IP address of the server.</param>
        /// <param name="port">The port of the server.</param>
        /// <param name="callback">The callback to invoke one the connection attempt has finished.</param>
        /// <param name="noDelay">Whether to disable Nagel's algorithm or not.</param>
        public void ConnectInBackground(IPAddress ip, int port, bool noDelay, ConnectCompleteHandler callback = null)
        {
            ConnectInBackground(new BichannelClientConnection(ip, port, noDelay), callback);
        }

        /// <summary>
        ///     Connects to a remote server in the background using a <see cref="BichannelClientConnection"/>.
        /// </summary>
        /// <param name="ip">The IP address of the server.</param>
        /// <param name="tcpPort">The port the server is listening on for TCP.</param>
        /// <param name="udpPort">The port the server is listening on for UDP.</param>
        /// <param name="callback">The callback to invoke one the connection attempt has finished.</param>
        /// <param name="noDelay">Whether to disable Nagel's algorithm or not.</param>
        public void ConnectInBackground(IPAddress ip, int tcpPort, int udpPort, bool noDelay, ConnectCompleteHandler callback = null)
        {
            ConnectInBackground(new BichannelClientConnection(ip, tcpPort, udpPort, noDelay), callback);
        }

        /// <summary>
        ///     Connects to a remote server in the background.
        /// </summary>
        /// <param name="connection">The connection to use to connect to the server.</param>
        /// <param name="callback">The callback to invoke one the connection attempt has finished.</param>
        public void ConnectInBackground(NetworkClientConnection connection, ConnectCompleteHandler callback = null)
        {
            new Thread(
                delegate ()
                {
                    try
                    {
                        Connect(connection);
                    }
                    catch (Exception e)
                    {
                        callback?.Invoke(e);
                        return;
                    }

                    callback?.Invoke(null);
                }
            ).Start();
        }

        /// <summary>
        ///     Sends a message to the server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="sendMode">How the message should be sent.</param>
        /// <returns>Whether the send was successful.</returns>
        public bool SendMessage(Message message, SendMode sendMode)
        {
            if (message.IsPingMessage)
                RoundTripTime.RecordOutboundPing(message.PingCode);

            return Connection.SendMessage(message.ToBuffer(), sendMode);
        }

        /// <summary>
        ///     Gets the endpoint with the given name.
        /// </summary>
        /// <param name="name">The name of the endpoint.</param>
        /// <returns>The end point.</returns>
        public IPEndPoint GetRemoteEndPoint(string name)
        {
            return Connection.GetRemoteEndPoint(name);
        }

        /// <summary>
        ///     Disconnects this client from the server.
        /// </summary>
        /// <returns>Whether the disconnect was successful.</returns>
        public bool Disconnect()
        {
            if (Connection == null)
                return false;

            if (!Connection.Disconnect())
                return false;

            Disconnected?.Invoke(this, new DisconnectedEventArgs(true, SocketError.Disconnecting, null));

            return true;
        }

        /// <summary>
        ///     Callback for when data is received.
        /// </summary>
        /// <param name="buffer">The data recevied.</param>
        /// <param name="sendMode">The SendMode used to send the data.</param>
        private void MessageReceivedHandler(MessageBuffer buffer, SendMode sendMode)
        {
            using (Message message = Message.Create(buffer, true))
            {
                //Record any ping acknowledgements
                if (message.IsPingAcknowledgementMessage)
                {
                    try
                    {
                        RoundTripTime.RecordInboundPing(message.PingCode);
                    }
                    catch (KeyNotFoundException)
                    {
                        //Nothing we can really do about this
                    }
                }

                if (message.IsCommandMessage)
                    HandleCommand(message);
                else
                    HandleMessage(message, sendMode);
            }
        }

        /// <summary>
        ///     Handles a command received.
        /// </summary>
        /// <param name="message">The message that was received.</param>
        private void HandleCommand(Message message)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                switch ((CommandCode)message.Tag)
                {
                    case CommandCode.Configure:
                        ID = reader.ReadUInt16();

                        setupMutex.Set();
                        break;
                }
            }
        }

        /// <summary>
        ///     Handles a message received.
        /// </summary>
        /// <param name="message">The message that was received.</param>
        /// <param name="sendMode">The send mode the message was received with.</param>
        private void HandleMessage(Message message, SendMode sendMode)
        {
            //Invoke for message received event
            using (MessageReceivedEventArgs args = MessageReceivedEventArgs.Create(message, sendMode))
                MessageReceived?.Invoke(this, args);
        }

        /// <summary>
        ///     Called when this connection becomes disconnected.
        /// </summary>
        /// <param name="error">The error that caused the disconnection.</param>
        /// <param name="exception">The exception that caused the disconnection.</param>
        private void DisconnectedHandler(SocketError error, Exception exception)
        {
            Disconnected?.Invoke(this, new DisconnectedEventArgs(false, error, exception));
        }

        private volatile bool disposed = false;
        
        /// <summary>
        ///     Disposes of the DarkRift client object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Handles disposing of the DarkRift client object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                disposed = true;

                if (Connection != null)
                    Connection.Dispose();

                setupMutex.Close();
            }
        }
    }
}
