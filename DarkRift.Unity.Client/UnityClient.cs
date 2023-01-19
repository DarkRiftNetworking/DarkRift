/*
Copyright (c) 2022 Unordinal AB

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using DarkRift.Dispatching;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Serialization;

namespace DarkRift.Client.Unity
{
    [AddComponentMenu("DarkRift/Client")]
	public sealed class UnityClient : MonoBehaviour
	{
        /// <summary>
        ///     The IP address this client connects to.
        /// </summary>
        /// <remarks>
        ///     If <see cref="Host"/> is not an IP address this property will perform a DNS resolution which may be slow!
        /// </remarks>
        public IPAddress Address
        {
            get { return Dns.GetHostAddresses(host)[0]; }
            set { host = value.ToString(); }
        }

        /// <summary>
        ///     The host this client connects to.
        /// </summary>
        public string Host
        {
            get { return host; }
            set { host = value; }
        }

        [SerializeField]
        [FormerlySerializedAs("address")]
        [Tooltip("The host to connect to.")]
        private string host = "localhost";

        /// <summary>
        ///     The port this client connects to.
        /// </summary>
        public ushort Port
        {
            get { return port; }
            set { port = value; }
        }

		[SerializeField]
		[Tooltip("The port on the server the client will connect to.")]
		private ushort port = 4296;

        [SerializeField]
        [Tooltip("Whether to disable Nagel's algorithm or not.")]
#pragma warning disable IDE0044 // Add readonly modifier, Unity can't serialize readonly fields
        private bool noDelay = false;

        [SerializeField]
        [FormerlySerializedAs("autoConnect")]
        [Tooltip("Indicates whether the client will connect to the server in the Start method.")]
        private bool connectOnStart = true;

        [SerializeField]
        [FormerlySerializedAs("invokeFromDispatcher")]
        [Tooltip("Specifies that DarkRift should take care of multithreading and invoke all events from Unity's main thread.")]
        private volatile bool eventsFromDispatcher = true;

        [SerializeField]
        [Tooltip("Specifies whether DarkRift should log all data to the console.")]
        private volatile bool sniffData = false;
#pragma warning restore IDE0044 // Add readonly modifier
        #region Cache settings
        #region Legacy
        /// <summary>
        ///     The maximum number of <see cref="DarkRiftWriter"/> instances stored per thread.
        /// </summary>
        [Obsolete("Use the ObjectCacheSettings property instead.")]
        public int MaxCachedWriters
        {
            get
            {
                return ObjectCacheSettings.MaxWriters;
            }
        }

        /// <summary>
        ///     The maximum number of <see cref="DarkRiftReader"/> instances stored per thread.
        /// </summary>
        [Obsolete("Use the ObjectCacheSettings property instead.")]
        public int MaxCachedReaders
        {
            get
            {
                return ObjectCacheSettings.MaxReaders;
            }
        }

        /// <summary>
        ///     The maximum number of <see cref="Message"/> instances stored per thread.
        /// </summary>
        [Obsolete("Use the ObjectCacheSettings property instead.")]
        public int MaxCachedMessages
        {
            get
            {
                return ObjectCacheSettings.MaxMessages;
            }
        }

        /// <summary>
        ///     The maximum number of <see cref="System.Net.Sockets.SocketAsyncEventArgs"/> instances stored per thread.
        /// </summary>
        [Obsolete("Use the ObjectCacheSettings property instead.")]
        public int MaxCachedSocketAsyncEventArgs
        {
            get
            {
                return ObjectCacheSettings.MaxSocketAsyncEventArgs;
            }
        }

        /// <summary>
        ///     The maximum number of <see cref="ActionDispatcherTask"/> instances stored per thread.
        /// </summary>
        [Obsolete("Use the ObjectCacheSettings property instead.")]
        public int MaxCachedActionDispatcherTasks
        {
            get
            {
                return ObjectCacheSettings.MaxActionDispatcherTasks;
            }
        }
        #endregion Legacy

        /// <summary>
        ///     The client object cache settings in use.
        /// </summary>
        public ClientObjectCacheSettings ClientObjectCacheSettings
        {
            get
            {
#pragma warning disable CS0618 // Type or member is obsolete
                return ObjectCacheSettings as ClientObjectCacheSettings;
            }
            set
            {
                ObjectCacheSettings = value;
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        /// <summary>
        ///     The object cache settings in use.
        /// </summary>
        [Obsolete("Use ClientObjectCacheSettings instead.")]
        public ObjectCacheSettings ObjectCacheSettings { get; set; }

        /// <summary>
        ///     Serialisable version of the object cache settings for Unity.
        /// </summary>
        [SerializeField]
#pragma warning disable IDE0044 // Add readonly modifier, Unity can't serialize readonly fields
        private SerializableObjectCacheSettings objectCacheSettings = new SerializableObjectCacheSettings();
#pragma warning restore IDE0044 // Add readonly modifier, Unity can't serialize readonly fields
        #endregion

        /// <summary>
        ///     Event fired when a message is received.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        ///     Event fired when we disconnect form the server.
        /// </summary>
        public event EventHandler<DisconnectedEventArgs> Disconnected;

        /// <summary>
        ///     The ID the client has been assigned.
        /// </summary>
        public ushort ID
        {
            get
            {
                return Client.ID;
            }
        }

        /// <summary>
        ///     Returns whether or not this client is connected to the server.
        /// </summary>
        [Obsolete("User ConnectionState instead.")]
        public bool Connected
        {
            get
            {
                return Client.Connected;
            }
        }


        /// <summary>
        ///     Returns the state of the connection with the server.
        /// </summary>
        public ConnectionState ConnectionState
        {
            get
            {
                return Client.ConnectionState;
            }
        }

        /// <summary>
        /// 	The actual client connecting to the server.
        /// </summary>
        /// <value>The client.</value>
        public DarkRiftClient Client { get; private set; }

        /// <summary>
        ///     The dispatcher for moving work to the main thread.
        /// </summary>
        public Dispatcher Dispatcher { get; private set; }
        
        private void Awake()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            ObjectCacheSettings = objectCacheSettings.ToClientObjectCacheSettings();

            Client = new DarkRiftClient(ObjectCacheSettings);
#pragma warning restore CS0618 // Type or member is obsolete

            //Setup dispatcher
            Dispatcher = new Dispatcher(true);

            //Setup routing for events
            Client.MessageReceived += Client_MessageReceived;
            Client.Disconnected += Client_Disconnected;
        }

        private void Start()
		{
            //If connect on start is true then connect to the server
            if (connectOnStart)
			    Connect(host, port, noDelay);
		}

        private void Update()
        {
            //Execute all the queued dispatcher tasks
            Dispatcher.ExecuteDispatcherTasks();
        }

        private void OnDestroy()
        {
            //Remove resources
            Close();
        }

        private void OnApplicationQuit()
        {
            //Remove resources
            Close();
        }

        /// <summary>
        ///     Connects to a remote server.
        /// </summary>
        /// <param name="ip">The IP address of the server.</param>
        /// <param name="port">The port of the server.</param>
        [Obsolete("Use other Connect overloads that automatically detect the IP version.")]
        public void Connect(IPAddress ip, int port, IPVersion ipVersion)
        {
            Client.Connect(ip, port, ipVersion);

            if (ConnectionState == ConnectionState.Connected)
                Debug.Log("Connected to " + ip + " on port " + port + " using " + ipVersion + ".");
            else
                Debug.Log("Connection failed to " + ip + " on port " + port + " using " + ipVersion + ".");
        }

        /// <summary>
        ///     Connects to a remote server.
        /// </summary>
        /// <param name="ip">The IP address of the server.</param>
        /// <param name="port">The port of the server.</param>
        /// <param name="noDelay">Whether to disable Nagel's algorithm or not.</param>
        public void Connect(IPAddress ip, int port, bool noDelay)
        {
            Client.Connect(ip, port, noDelay);

            if (ConnectionState == ConnectionState.Connected)
                Debug.Log("Connected to " + ip + " on port " + port + ".");
            else
                Debug.Log("Connection failed to " + ip + " on port " + port + ".");
        }

        /// <summary>
        ///     Connects to a remote server.
        /// </summary>
        /// <param name="host">The host to connect to.</param>
        /// <param name="port">The port of the server.</param>
        /// <param name="noDelay">Whether to disable Nagel's algorithm or not.</param>
        public void Connect(string host, int port, bool noDelay)
        {
            Connect(Dns.GetHostAddresses(host)[0], port, noDelay);
        }

        /// <summary>
        ///     Connects to a remote server.
        /// </summary>
        /// <param name="ip">The IP address of the server.</param>
        /// <param name="tcpPort">The port the server is listening on for TCP.</param>
        /// <param name="udpPort">The port the server is listening on for UDP.</param>
        /// <param name="noDelay">Whether to disable Nagel's algorithm or not.</param>
        public void Connect(IPAddress ip, int tcpPort, int udpPort, bool noDelay)
        {
            Client.Connect(ip, tcpPort, udpPort, noDelay);

            if (ConnectionState == ConnectionState.Connected)
                Debug.Log("Connected to " + ip + " on port " + tcpPort + "|" + udpPort + ".");
            else
                Debug.Log("Connection failed to " + ip + " on port " + tcpPort + "|" + udpPort + ".");
        }

        /// <summary>
        ///     Connects to a remote server.
        /// </summary>
        /// <param name="host">The host to connect to.</param>
        /// <param name="tcpPort">The port the server is listening on for TCP.</param>
        /// <param name="udpPort">The port the server is listening on for UDP.</param>
        /// <param name="noDelay">Whether to disable Nagel's algorithm or not.</param>
        public void Connect(string host, int tcpPort, int udpPort, bool noDelay)
        {
            Connect(Dns.GetHostAddresses(host)[0], tcpPort, udpPort, noDelay);
        }

        /// <summary>
        ///     Connects to a remote asynchronously.
        /// </summary>
        /// <param name="ip">The IP address of the server.</param>
        /// <param name="port">The port of the server.</param>
        /// <param name="callback">The callback to make when the connection attempt completes.</param>
        [Obsolete("Use other ConnectInBackground overloads that automatically detect the IP version.")]
#pragma warning disable 0618 // Implementing obsolete functionality
        public void ConnectInBackground(IPAddress ip, int port, IPVersion ipVersion, DarkRiftClient.ConnectCompleteHandler callback = null)
        {
            Client.ConnectInBackground(
                ip,
                port, 
                ipVersion, 
                delegate (Exception e)
                {
                    if (callback != null)
                    {
                        if (eventsFromDispatcher)
                            Dispatcher.InvokeAsync(() => callback(e));
                        else
                            callback.Invoke(e);
                    }
                    
                    if (ConnectionState == ConnectionState.Connected)
                        Debug.Log("Connected to " + ip + " on port " + port + " using " + ipVersion + ".");
                    else
                        Debug.Log("Connection failed to " + ip + " on port " + port + " using " + ipVersion + ".");
                }
            );
#pragma warning restore 0618 // Implementing obsolete functionality
        }

        /// <summary>
        ///     Connects to a remote asynchronously.
        /// </summary>
        /// <param name="ip">The IP address of the server.</param>
        /// <param name="port">The port of the server.</param>
        /// <param name="noDelay">Whether to disable Nagel's algorithm or not.</param>
        /// <param name="callback">The callback to make when the connection attempt completes.</param>
        public void ConnectInBackground(IPAddress ip, int port, bool noDelay, DarkRiftClient.ConnectCompleteHandler callback = null)
        {
            Client.ConnectInBackground(
                ip,
                port,
                noDelay,
                delegate (Exception e)
                {
                    if (callback != null)
                    {
                        if (eventsFromDispatcher)
                            Dispatcher.InvokeAsync(() => callback(e));
                        else
                            callback.Invoke(e);
                    }
                    
                    if (ConnectionState == ConnectionState.Connected)
                        Debug.Log("Connected to " + ip + " on port " + port + ".");
                    else
                        Debug.Log("Connection failed to " + ip + " on port " + port + ".");
                }
            );
        }

        /// <summary>
        ///     Connects to a remote asynchronously.
        /// </summary>
        /// <param name="host">The host to connect to.</param>
        /// <param name="port">The port of the server.</param>
        /// <param name="noDelay">Whether to disable Nagel's algorithm or not.</param>
        /// <param name="callback">The callback to make when the connection attempt completes.</param>
        public void ConnectInBackground(string host, int port, bool noDelay, DarkRiftClient.ConnectCompleteHandler callback = null)
        {
            ConnectInBackground(
                Dns.GetHostAddresses(host)[0],
                port,
                noDelay,
                callback
            );
        }

        /// <summary>
        ///     Connects to a remote asynchronously.
        /// </summary>
        /// <param name="ip">The IP address of the server.</param>
        /// <param name="tcpPort">The port the server is listening on for TCP.</param>
        /// <param name="udpPort">The port the server is listening on for UDP.</param>
        /// <param name="noDelay">Whether to disable Nagel's algorithm or not.</param>
        /// <param name="callback">The callback to make when the connection attempt completes.</param>
        public void ConnectInBackground(IPAddress ip, int tcpPort, int udpPort, bool noDelay, DarkRiftClient.ConnectCompleteHandler callback = null)
        {
            Client.ConnectInBackground(
                ip,
                tcpPort,
                udpPort,
                noDelay,
                delegate (Exception e)
                {
                    if (callback != null)
                    {
                        if (eventsFromDispatcher)
                            Dispatcher.InvokeAsync(() => callback(e));
                        else
                            callback.Invoke(e);
                    }
                    
                    if (ConnectionState == ConnectionState.Connected)
                        Debug.Log("Connected to " + ip + " on port " + tcpPort + "|" + udpPort + ".");
                    else
                        Debug.Log("Connection failed to " + ip + " on port " + tcpPort + "|" + udpPort + ".");
                }
            );
        }
        
        /// <summary>
        ///     Connects to a remote asynchronously.
        /// </summary>
        /// <param name="host">The host to connect to.</param>
        /// <param name="tcpPort">The port the server is listening on for TCP.</param>
        /// <param name="udpPort">The port the server is listening on for UDP.</param>
        /// <param name="noDelay">Whether to disable Nagel's algorithm or not.</param>
        /// <param name="callback">The callback to make when the connection attempt completes.</param>
        public void ConnectInBackground(string host, int tcpPort, int udpPort, bool noDelay, DarkRiftClient.ConnectCompleteHandler callback = null)
        {
            ConnectInBackground(
                Dns.GetHostAddresses(host)[0],
                tcpPort,
                udpPort,
                noDelay,
                callback
            );
        }

        /// <summary>
        ///     Sends a message to the server.
        /// </summary>
        /// <param name="message">The message template to send.</param>
        /// <returns>Whether the send was successful.</returns>
        public bool SendMessage(Message message, SendMode sendMode)
        {
            return Client.SendMessage(message, sendMode);
        }

        /// <summary>
        ///     Invoked when DarkRift receives a message from the server.
        /// </summary>
        /// <param name="sender">The client that received the message.</param>
        /// <param name="e">The arguments for the event.</param>
        private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            //If we're handling multithreading then pass the event to the dispatcher
            if (eventsFromDispatcher)
            {
                if (sniffData)
                    Debug.Log("Message Received: Tag = " + e.Tag + ", SendMode = " + e.SendMode);

                // DarkRift will recycle the message inside the event args when this method exits so make a copy now that we control the lifecycle of!
                Message message = e.GetMessage();
                MessageReceivedEventArgs args = MessageReceivedEventArgs.Create(message, e.SendMode);

                Dispatcher.InvokeAsync(
                    () => 
                        {
                            EventHandler<MessageReceivedEventArgs> handler = MessageReceived;
                            if (handler != null)
                            {
                                handler.Invoke(sender, args);
                            }

                            message.Dispose();
                            args.Dispose();
                        }
                );
            }
            else
            {
                if (sniffData)
                    Debug.Log("Message Received: Tag = " + e.Tag + ", SendMode = " + e.SendMode);

                EventHandler<MessageReceivedEventArgs> handler = MessageReceived;
                if (handler != null)
                {
                    handler.Invoke(sender, e);
                }
            }
        }

        private void Client_Disconnected(object sender, DisconnectedEventArgs e)
        {
            //If we're handling multithreading then pass the event to the dispatcher
            if (eventsFromDispatcher)
            {
                if (!e.LocalDisconnect)
                    Debug.Log("Disconnected from server, error: " + e.Error);

                Dispatcher.InvokeAsync(
                    () =>
                    {
                        EventHandler<DisconnectedEventArgs> handler = Disconnected;
                        if (handler != null)
                        {
                            handler.Invoke(sender, e);
                        }
                    }
                );
            }
            else
            {
                if (!e.LocalDisconnect)
                    Debug.Log("Disconnected from server, error: " + e.Error);
                
                EventHandler<DisconnectedEventArgs> handler = Disconnected;
                if (handler != null)
                {
                    handler.Invoke(sender, e);
                }
            }
        }

        /// <summary>
        ///     Disconnects this client from the server.
        /// </summary>
        /// <returns>Whether the disconnect was successful.</returns>
        public bool Disconnect()
        {
            return Client.Disconnect();
        }

        /// <summary>
        ///     Closes this client.
        /// </summary>
        public void Close()
        {
            Client.MessageReceived -= Client_MessageReceived;
            Client.Disconnected -= Client_Disconnected;

            Client.Dispose();
            Dispatcher.Dispose();
        }
	}
}
