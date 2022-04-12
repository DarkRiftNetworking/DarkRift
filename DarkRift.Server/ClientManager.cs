/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using DarkRift.Server.Plugins.Listeners.Bichannel;
using DarkRift.Dispatching;
using System.Diagnostics;
using DarkRift.Server.Metrics;
using System.Xml.Schema;

namespace DarkRift.Server
{
    /// <summary>
    ///     Handles all clients on the server.
    /// </summary>
    internal sealed class ClientManager : IDisposable, IClientManager
    {
        /// <summary>
        ///     The address the server is listening on.
        /// </summary>
        [Obsolete("Use listener system instead. This will currently read the address of a BichannelListener called 'DefaultNetworkListener'.")]
        public IPAddress Address => GetDefaultBichannelListenerOrError().Address;

        /// <summary>
        ///     The port the server is listening on.
        /// </summary>
        [Obsolete("Use listener system instead. This will currently read the port of a BichannelListener called 'DefaultNetworkListener'.")]
        public ushort Port => GetDefaultBichannelListenerOrError().Port;
            
        /// <summary>
        ///     The IP version that the server is listening on.
        /// </summary>
        [Obsolete("Use listener system instead. This will currently read the IP version of a BichannelListener called 'DefaultNetworkListener'.")]
        public IPVersion IPVersion => GetDefaultBichannelListenerOrError().Address.AddressFamily == AddressFamily.InterNetworkV6 ? IPVersion.IPv6 : IPVersion.IPv4;

        /// <summary>
        ///     Whether Nagle's algorithm is disabled.
        /// </summary>
        [Obsolete("Use listener system instead. This will currently read the no delay property of a BichannelListener called 'DefaultNetworkListener'.")]
        public bool NoDelay => GetDefaultBichannelListenerOrError().NoDelay;

        /// <summary>
        ///     Returns whether the server has been started and not yet stopped.
        /// </summary>
        public bool Listening { get; private set; }

        /// <summary>
        ///     Invoked when a client connects to the server.
        /// </summary>
        public event EventHandler<ClientConnectedEventArgs> ClientConnected;

        /// <summary>
        ///     Invoked when a client disconnects from the server.
        /// </summary>
        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;
        
        /// <summary>
        ///     Returns the number of clients currently connected.
        /// </summary>
        public int Count
        {
            get
            {
                lock (clients)
                    return clients.Count;
            }
        }

        /// <summary>
        ///     The number of strikes a client can get before they are kicked.
        /// </summary>
        public byte MaxStrikes { get; }

        /// <summary>
        ///     Whether the fallback networking is being used for compatability.
        /// </summary>
        /// <remarks>
        ///     Unity has issues with DarkRift's default (better) socket interfaces so this indicates
        ///     the fallback networking is in use for compatability at a performance cost.
        /// </remarks>
        [Obsolete("Use listener system instead. This will currently read if a BichannelListener or CompatibilityBichannelListener is called 'DefaultNetworkListener'.")]
        public bool UseFallbackNetworking => GetDefaultBichannelListenerOrError() is CompatibilityBichannelListener;

        /// <summary>
        ///     The clients connected to this server.
        /// </summary>
        private readonly Dictionary<ushort, Client> clients = new Dictionary<ushort, Client>();

        /// <summary>
        ///     The IDs of clients connecting but without objects created on the sever yet.
        /// </summary>
        private readonly HashSet<ushort> allocatedIds = new HashSet<ushort>();

        /// <summary>
        ///     The last ID allocated on this server
        /// </summary>
        private ushort lastIDAllocated = ushort.MaxValue;       //Start at 0

        /// <summary>
        ///     The lock on ID allocation
        /// </summary>
        private readonly object idLockObj = new object();

        /// <summary>
        ///     The server's network listener manager.
        /// </summary>
        private readonly NetworkListenerManager networkListenerManager;

        /// <summary>
        ///     The thread helper the client manager will use.
        /// </summary>
        private readonly DarkRiftThreadHelper threadHelper;

        /// <summary>
        ///     The logger this client manager will use.
        /// </summary>
        private readonly Logger logger;

        /// <summary>
        ///     The logger clients will use.
        /// </summary>
        private readonly Logger clientLogger;

#if PRO
        /// <summary>
        ///     Gauge metric of the number of clients currently connected.
        /// </summary>
        private readonly IGaugeMetric clientsConnectedGauge;

        /// <summary>
        ///     Histogram metric of time taken to execute the <see cref="ClientConnected"/> event.
        /// </summary>
        private readonly IHistogramMetric clientConnectedEventTimeHistogram;

        /// <summary>
        ///     Histogram metric of time taken to execute the <see cref="ClientDisconnected"/> event.
        /// </summary>
        private readonly IHistogramMetric clientDisconnectedEventTimeHistogram;

        /// <summary>
        ///     Counter metric of failures executing the <see cref="ClientConnected"/> event.
        /// </summary>
        private readonly ICounterMetric clientConnectedEventFailuresCounter;

        /// <summary>
        ///     Counter metric of failures executing the <see cref="ClientDisconnected"/> event.
        /// </summary>
        private readonly ICounterMetric clientDisconnectedEventFailuresCounter;

        /// <summary>
        ///     Metrics collector used by the clients.
        /// </summary>
        private readonly MetricsCollector clientMetricsCollector;
#endif

#if PRO
        /// <summary>
        ///     Creates a new client manager.
        /// </summary>
        /// <param name="settings">The settings for this client manager.</param>
        /// <param name="networkListenerManager">The server's network listener manager. Used to implement obsolete functionality.</param>
        /// <param name="threadHelper">The thread helper the client manager will use.</param>
        /// <param name="logger">The logger this client manager will use.</param>
        /// <param name="clientLogger">The logger clients will use.</param>
        /// <param name="metricsCollector">The metrics collector to use.</param>
        /// <param name="clientMetricsCollector">The metrics collector clients will use.</param>
        internal ClientManager(ServerSpawnData.ServerSettings settings, NetworkListenerManager networkListenerManager, DarkRiftThreadHelper threadHelper, Logger logger, Logger clientLogger, MetricsCollector metricsCollector, MetricsCollector clientMetricsCollector)
#else
        /// <summary>
        ///     Creates a new client manager.
        /// </summary>
        /// <param name="settings">The settings for this client manager.</param>
        /// <param name="networkListenerManager">The server's network listener manager.</param>
        /// <param name="threadHelper">The thread helper the client manager will use.</param>
        /// <param name="logger">The logger this client manager will use.</param>
        /// <param name="clientLogger">The logger clients will use.</param>
        internal ClientManager(ServerSpawnData.ServerSettings settings, NetworkListenerManager networkListenerManager, DarkRiftThreadHelper threadHelper, Logger logger, Logger clientLogger)
#endif
        {
            this.MaxStrikes = settings.MaxStrikes;
            this.networkListenerManager = networkListenerManager;
            this.threadHelper = threadHelper;
            this.logger = logger;
            this.clientLogger = clientLogger;
#if PRO
            this.clientMetricsCollector = clientMetricsCollector;

            clientsConnectedGauge = metricsCollector.Gauge("clients_connected", "The number of clients connected to the server.");
            clientConnectedEventTimeHistogram = metricsCollector.Histogram("client_connected_event_time", "The time taken to execute the ClientConnected event.");
            clientDisconnectedEventTimeHistogram = metricsCollector.Histogram("client_disconnected_event_time", "The time taken to execute the ClientDisconnected event.");
            clientConnectedEventFailuresCounter = metricsCollector.Counter("client_connected_event_failures", "The number of failures executing the ClientConnected event.");
            clientDisconnectedEventFailuresCounter = metricsCollector.Counter("client_disconnected_event_failures", "The number of failures executing the ClientDisconnected event.");
#endif
        }

        /// <summary>
        ///     Subscribes the client manager to all network listeners in the NetworkListenerManager.
        /// </summary>
        internal void SubscribeToListeners()
        {
            foreach (NetworkListener listener in networkListenerManager.GetAllNetworkListeners())
            {
                // Unsubscribe first to make sure we don't start getting duplicate calls if this method is called twice
                listener.RegisteredConnection -= HandleNewConnection;
                listener.RegisteredConnection += HandleNewConnection;
            }
        }

        /// <summary>
        ///     Returns the Default BichannelListener if present or throws an exception.
        /// </summary>
        /// <returns>The default Bichannel listener.</returns>
        private BichannelListenerBase GetDefaultBichannelListenerOrError()
        {
            NetworkListener listener = networkListenerManager.GetNetworkListenerByName("DefaultNetworkListener");
            if (listener == null)
                throw new InvalidOperationException("There is no listener named \"DefaultNetworkListener\" configured, use the NetworkListenerManager to retreive a NetworkListener and access this property directly instead.");

            if (!(listener is BichannelListenerBase bListener))
                throw new InvalidOperationException("The listener named \"DefaultNetworkListener\" is not a BichannelListener, use the NetworkListenerManager to retreive a NetworkListener and access this property directly instead.");

            return bListener;
        }

        /// <summary>
        ///     Called when a new client connects.
        /// </summary>
        /// <param name="connection">The new client.</param>
        internal void HandleNewConnection(NetworkServerConnection connection)
        {
            //Allocate ID and add to list
            ushort id;
            try
            {
                id = ReserveID();
            }
            catch (InvalidOperationException)
            {
                logger.Info($"New client could not be connected as there were no IDs available to allocate to them [{connection.RemoteEndPoints.Format()}].");

                connection.Disconnect();

                return;
            }


            Client client;
            try
            {
                client = Client.Create(
                    connection,
                    id,
                    this,
                    threadHelper,
                    clientLogger
#if PRO
                    , clientMetricsCollector
#endif
                );
            }
            catch (Exception e)
            {
                logger.Error("An exception ocurred while connecting a client. The client has been dropped.", e);

                connection.Disconnect();

                DeallocateID(id, out int _);

                return;
            }

            AllocateIDToClient(id, client, out int noClients);

            // TODO if a client sends immediately after connecting then the message will be missed as the Connected event has not yet fired

            connection.Client = client;

            logger.Info($"New client [{client.ID}] connected [{client.RemoteEndPoints.Format()}].");
#if PRO
            clientsConnectedGauge.Report(noClients);
#endif

            //Inform plugins of the new connection
            EventHandler<ClientConnectedEventArgs> handler = ClientConnected;
            if (handler != null)
            {
                threadHelper.DispatchIfNeeded(
                    delegate ()
                    {
#if PRO
                        long startTimestamp = Stopwatch.GetTimestamp();
#endif
                        try
                        {
                            handler.Invoke(this, new ClientConnectedEventArgs(client));
                        }
                        catch (Exception e)
                        {
                            logger.Error("A plugin encountered an error whilst handling the ClientConnected event. The client will be disconnected. (See logs for exception)", e);

                            client.DropConnection();

#if PRO
                            clientConnectedEventFailuresCounter.Increment();
#endif
                            return;
                        }

#if PRO
                        double time = (double)(Stopwatch.GetTimestamp() - startTimestamp) / Stopwatch.Frequency;
                        clientConnectedEventTimeHistogram.Report(time);
#endif
                    },
                    (_) => client.StartListening()
                );
            }
        }

        /// <summary>
        ///     Allocates a specified ID to a client.
        /// </summary>
        /// <param name="id">The ID to allocate.</param>
        /// <param name="client">The client to allocate the ID to.</param>
        /// <param name="totalClients">The total number of clients connected.</param>
        private void AllocateIDToClient(ushort id, Client client, out int totalClients)
        {
            lock (clients)
            {
                clients[id] = client;
                totalClients = clients.Count;
            }

            lock (idLockObj)
                allocatedIds.Remove(id);
        }

        /// <summary>
        ///     Deallocates a specified ID.
        /// </summary>
        /// <param name="id">The ID to deallocate.</param>
        /// <param name="totalClients">The total number of clients connected.</param>
        /// <returns>true, if the ID was allocated; else, false.</returns>
        private bool DeallocateID(ushort id, out int totalClients)
        {
            bool removed;
            lock (clients)
            {
                removed = clients.Remove(id);
                totalClients = clients.Count;
            }

            lock (idLockObj)
                removed = removed || allocatedIds.Remove(id);

            return removed;
        }

        /// <summary>
        ///     Allocates a new ID.
        /// </summary>
        /// <returns>The ID allocated for the new client.</returns>
        /// <exception cref="KeyNotFoundException">If the ID is already allocated.</exception>
        /// <exception cref="InvalidOperationException">If there are no IDs available to allocate.</exception>
        internal ushort ReserveID()
        {
            lock (idLockObj)
            {
                ushort toTest = lastIDAllocated;
                bool taken;
                do
                {
                    unchecked
                    {
                        toTest++;
                    }

                    lock (clients)
                        taken = clients.ContainsKey(toTest) || allocatedIds.Contains(toTest);

                    //Check there are still IDs to allocate!
                    if (toTest == lastIDAllocated)
                    {
                        throw new InvalidOperationException("No ID free to allocate.");
                    }
                } while (taken);

                lastIDAllocated = toTest;

                // Reserve the ID
                allocatedIds.Add(toTest);

                return toTest;
            }
        }

        /// <summary>
        ///     Handles a client disconnecting.
        /// </summary>
        /// <param name="client">The client disconnecting.</param>
        /// <param name="localDisconnect">If the disconnection was caused by a call to <see cref="Client.Disconnect"/></param>
        /// <param name="error">The error that caused the disconnect.</param>
        /// <param name="exception">The exception that caused the disconnect.</param>
        internal void HandleDisconnection(Client client, bool localDisconnect, SocketError error, Exception exception)
        {
            // If we're not in the current list of clients we've already disconnected
            if (!DeallocateID(client.ID, out int noClients))
                return;

            //Inform plugins of the disconnection
            EventHandler<ClientDisconnectedEventArgs> handler = ClientDisconnected;
            if (handler != null)
            {
                threadHelper.DispatchIfNeeded(
                    delegate ()
                    {
#if PRO
                        long startTimestamp = Stopwatch.GetTimestamp();
#endif
                        try
                        {
                            handler.Invoke(this, new ClientDisconnectedEventArgs(client, localDisconnect, error, exception));
                        }
                        catch (Exception e)
                        {
                            logger.Error("A plugin encountered an error whilst handling the ClientDisconnected event. (See logs for exception)", e);
#if PRO
                            clientDisconnectedEventFailuresCounter.Increment();
#endif
                            return;
                        }

#if PRO
                        double time = (double)(Stopwatch.GetTimestamp() - startTimestamp) / Stopwatch.Frequency;
                        clientDisconnectedEventTimeHistogram.Report(time);
#endif
                    },
                    delegate (ActionDispatcherTask t)
                    {
                        FinaliseClientDisconnect(exception, error, client, noClients);
                    }
                );
            }
            else
            {
                FinaliseClientDisconnect(exception, error, client, noClients);
            }
        }

        /// <summary>
        ///     Finalises the client disconnecting.
        /// </summary>
        /// <param name="exception">The exception causing the disconnect.</param>
        /// <param name="error">The SocketError causing the disconnect.</param>
        /// <param name="client">The client disconnecting.</param>
        /// <param name="noClients">The number of clients to report as connected</param>
        private void FinaliseClientDisconnect(Exception exception, SocketError error, Client client, int noClients)
        {
            if (error == SocketError.Success || error == SocketError.Disconnecting || error == SocketError.OperationAborted)
            {
                logger.Info($"Client [{client.ID}] disconnected.", exception);
            }
            else
            {
                string reason = error == SocketError.Success ? exception.Message : error.ToString();
                logger.Info($"Client [{client.ID}] disconnected: {reason}.", exception);
            }

            client.Dispose();

#if PRO
            clientsConnectedGauge.Report(noClients);
#endif
        }

        /// <summary>
        ///     Handles a client being dropped.
        /// </summary>
        /// <param name="client">The client disconnecting.</param>
        internal void DropClient(Client client)
        {
            DeallocateID(client.ID, out int noClients);

#if PRO
            clientsConnectedGauge.Report(noClients);
#endif
        }

        // TODO calling the methods below rapidly can block new connections being accepted as the lock on clients cant be aquired

        /// <inheritdoc/>
        public IClient[] GetAllClients()
        {
            lock (clients)
                return clients.Values.ToArray();
        }

        /// <inheritdoc/>
        public IClient this[ushort id] => GetClient(id);

        /// <inheritdoc/>
        public IClient GetClient(ushort id)
        {
            lock (clients)
                return clients[id];
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

#pragma warning disable CS0628
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (clients)
                {
                    foreach (Client connection in clients.Values)
                        connection.Dispose();
                }
            }
        }
#pragma warning restore CS0628
    }
}
