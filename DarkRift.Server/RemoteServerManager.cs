/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Server.Metrics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DarkRift.Server
{
#if PRO
    internal sealed class RemoteServerManager : IRemoteServerManager, IDisposable
    {
        /// <summary>
        ///     The ID of the server in the cluster.
        /// </summary>
        public ushort ServerID { get; private set; }

        /// <summary>
        ///     The group the server is in.
        /// </summary>
        public string Group { get; }
        
        /// <summary>
        ///     The visibility of the server.
        /// </summary>
        public ServerVisibility Visibility { get; }

        /// <summary>
        ///     The host we're advertised on.
        /// </summary>
        public string AdvertisedHost { get; set; }

        /// <summary>
        ///     The port we're advertised on.
        /// </summary>
        public ushort AdvertisedPort { get; set; }

        /// <summary>
        ///     The server groups in this server.
        /// </summary>
        private readonly Dictionary<string, IModifiableServerGroup> groups = new Dictionary<string, IModifiableServerGroup>();

        /// <summary>
        ///     The server's network listener manager.
        /// </summary>
        private readonly NetworkListenerManager networkListenerManager;

        /// <summary>
        ///     The server's registry connector manager.
        /// </summary>
        private readonly ServerRegistryConnectorManager serverRegistryConnectorManager;

        /// <summary>
        ///     The downstream servers awaiting identification.
        /// </summary>
        private readonly List<PendingDownstreamRemoteServer> pendingDownstreamServers = new List<PendingDownstreamRemoteServer>();

        /// <summary>
        ///     The logger to use.
        /// </summary>
        private readonly Logger logger;

        /// <summary>
        ///     Creates a new server manager.
        /// </summary>
        /// <param name="serverSettings">The server's main configuration.</param>
        /// <param name="serverRegistrySettings">The server's registry configuration.</param>
        /// <param name="clusterSpawnData">The cluster configuration</param>
        /// <param name="networkListenerManager">The server's network listener manager.</param>
        /// <param name="threadHelper">The server's thread helper.</param>
        /// <param name="serverRegistryConnectorManager">The server's registry connector manager.</param>
        /// <param name="logManager">The server's log manager.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="metricsManager">The server's metrics manager.</param>
        internal RemoteServerManager(ServerSpawnData.ServerSettings serverSettings, ServerSpawnData.ServerRegistrySettings serverRegistrySettings, ClusterSpawnData clusterSpawnData, NetworkListenerManager networkListenerManager, DarkRiftThreadHelper threadHelper, ServerRegistryConnectorManager serverRegistryConnectorManager, LogManager logManager, Logger logger, MetricsManager metricsManager)
        {
            this.AdvertisedHost = serverRegistrySettings.AdvertisedHost;
            this.AdvertisedPort = serverRegistrySettings.AdvertisedPort;


            this.networkListenerManager = networkListenerManager;
            this.serverRegistryConnectorManager = serverRegistryConnectorManager;
            this.logger = logger;
            this.Group = serverSettings.ServerGroup;

            if (!string.IsNullOrEmpty(serverSettings.ServerGroup))
            {
                if (serverRegistrySettings.AdvertisedHost == null || serverRegistrySettings.AdvertisedPort == 0)
                    throw new ArgumentException("Cannot start server clustering without an advertised host and port. Consider setting the 'advertisedHost' and 'advertisedPort' properties in configuration.");

                // Find our group
                ClusterSpawnData.GroupsSettings.GroupSettings ourGroup = null;
                foreach (ClusterSpawnData.GroupsSettings.GroupSettings groupSettings in clusterSpawnData.Groups.Groups)
                {
                    if (groupSettings.Name == serverSettings.ServerGroup)
                    {
                        ourGroup = groupSettings;
                        break;
                    }
                }

                if (ourGroup == null)
                    throw new ArgumentException($"The group specified to own this server '{serverSettings.ServerGroup}' is not present in the cluster configuration. Consider adding this group to the cluster configuration or moving the server to an existing group.");

                this.Visibility = ourGroup.Visibility;

                // Build relationships to other groups
                foreach (ClusterSpawnData.GroupsSettings.GroupSettings groupSettings in clusterSpawnData.Groups.Groups)
                {
                    ClusterSpawnData.GroupsSettings.GroupSettings.ConnectsToSettings ourGroupConnectsTo = ourGroup.ConnectsTo.FirstOrDefault(c => c.Name == groupSettings.Name);
                    ClusterSpawnData.GroupsSettings.GroupSettings.ConnectsToSettings connectsToOurGroup = groupSettings.ConnectsTo.FirstOrDefault(c => c.Name == ourGroup.Name);

                    IModifiableServerGroup serverGroup;
                    if (ourGroupConnectsTo != null)
                        serverGroup = new UpstreamServerGroup(groupSettings.Name, groupSettings.Visibility, threadHelper, serverRegistryConnectorManager, this, serverSettings.ReconnectAttempts, logManager.GetLoggerFor(nameof(UpstreamServerGroup)), logManager.GetLoggerFor(nameof(UpstreamRemoteServer)), metricsManager.GetMetricsCollectorFor(nameof(UpstreamServerGroup)), metricsManager.GetMetricsCollectorFor(nameof(UpstreamRemoteServer)));
                    else if (connectsToOurGroup != null)
                        serverGroup = new DownstreamServerGroup(groupSettings.Name, groupSettings.Visibility, threadHelper, logManager.GetLoggerFor(nameof(DownstreamServerGroup)), logManager.GetLoggerFor(nameof(DownstreamRemoteServer)), metricsManager.GetMetricsCollectorFor(nameof(DownstreamServerGroup)), metricsManager.GetMetricsCollectorFor(nameof(DownstreamRemoteServer)));
                    else
                        continue;

                    groups.Add(groupSettings.Name, serverGroup);
                }
            }
        }

        /// <summary>
        ///     Subscribes the server manager to all network listeners in the NetworkListenerManager.
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
        ///     Registers the server in the registry.
        /// </summary>
        internal void RegisterServer()
        {
            if (serverRegistryConnectorManager.ServerRegistryConnector != null)
            {
                ServerID = serverRegistryConnectorManager.ServerRegistryConnector.RegisterServer(Group, AdvertisedHost, AdvertisedPort, new Dictionary<string, string>());
                logger.Info("Registered server with registry as ID " + ServerID + ".");
            }
            else
            {
                logger.Trace("No server registry connector configured, skipping registration.");
            }
        }

        /// <summary>
        ///     Deregisters the server from the registry.
        /// </summary>
        internal void DeregisterServer()
        {
            if (serverRegistryConnectorManager.ServerRegistryConnector != null)
            {
                serverRegistryConnectorManager.ServerRegistryConnector.DeregisterServer();
                logger.Info("Deregistered server from registry.");
            }
            else
            {
                logger.Trace("No server registry connector configured, skipping deregistration.");
            }
        }

        /// <inheritdoc/>
        public IServerGroup[] GetAllGroups()
        {
            lock (groups)
                return groups.Values.ToArray();
        }

        /// <inheritdoc/>
        public IServerGroup this[string name] => GetGroup(name);

        /// <inheritdoc/>
        public IServerGroup GetGroup(string name)
        {
            lock (groups)
                return groups[name];
        }

        /// <inheritdoc/>
        public IRemoteServer FindServer(ushort id)
        {
            lock (groups)
            {
                foreach (IModifiableServerGroup group in groups.Values)
                {
                    try
                    {
                        return group.GetRemoteServer(id);
                    }
                    catch (KeyNotFoundException)
                    {
                        // TODO this sucks
                        continue;
                    }
                }

                throw new KeyNotFoundException("Unable to find remote server in any group.");
            }
        }

        /// <summary>
        ///     Handles a server becoming available.
        /// </summary>
        /// <param name="id">The ID of the server joining.</param>
        /// <param name="group">The group the server belongs to.</param>
        /// <param name="host">The host of the server.</param>
        /// <param name="port">The port of the server.</param>
        /// <param name="properties">The additional properties to connect with.</param>
        internal void HandleServerJoin(ushort id, string group, string host, ushort port, IDictionary<string, string> properties)
        {
            // Skip ourselves, we know we exist
            if (id == ServerID)
                return;

            IModifiableServerGroup serverGroup;
            bool exists;
            lock (groups)
                exists = groups.TryGetValue(group, out serverGroup);

            logger.Trace($"Server registry informed us of a server ({id}) in group '{group}'.");

            if (exists)
                serverGroup.HandleServerJoin(id, host, port, properties);
        }

        /// <summary>
        ///     Handles a server leaving the cluster.
        /// </summary>
        /// <param name="id">The ID of the sever leaving.</param>
        internal void HandleServerLeave(ushort id)
        {
            // TODO is this part of the remote server object's responsibility?
            IRemoteServer server;
            try
            {
                server = FindServer(id);
            }
            catch (KeyNotFoundException)
            {
                return;
            }

            logger.Trace($"Server registry informed us that server {id} has left the cluster.");

            ((IModifiableServerGroup)server.ServerGroup).HandleServerLeave(id);
        }

        /// <summary>
        ///     Called when a new server connects to this server.
        /// </summary>
        /// <param name="connection">The new connection.</param>
        internal void HandleNewConnection(NetworkServerConnection connection)
        {
            //TODO make configurable
            PendingDownstreamRemoteServer server = new PendingDownstreamRemoteServer(connection, 10000, HandleServerReady, HandleServerDroppedBeforeReady, logger);

            lock (pendingDownstreamServers)
                pendingDownstreamServers.Add(server);

            connection.StartListening();

            logger.Trace($"New server connected, awaiting identification [{connection.RemoteEndPoints.Format()}].");
        }

        /// <summary>
        ///     Called when a pending server has identified itself.
        /// </summary>
        /// <param name="pendingServer">The pending server that has been identified.</param>
        /// <param name="id">The ID the server has identified with.</param>
        internal void HandleServerReady(PendingDownstreamRemoteServer pendingServer, ushort id)
        {
            logger.Trace($"Server at [{pendingServer.Connection.RemoteEndPoints.Format()}] has identified as server {id}.");

            try
            {
                ((DownstreamRemoteServer)FindServer(id)).SetConnection(pendingServer);
            }
            catch (KeyNotFoundException)
            {
                pendingServer.Connection.Disconnect();

                logger.Trace($"Server at [{pendingServer.Connection.RemoteEndPoints.Format()} connected and identified itself as server {id} however the registry has not yet propgated information about that server. The connection has been dropped.");
            }
            finally
            {
                lock (pendingDownstreamServers)
                    pendingDownstreamServers.Remove(pendingServer);
            }
        }

        /// <summary>
        ///     Called when a pending server gets dropped.
        /// </summary>
        /// <param name="pendingServer">The pending server that has been dropped.</param>
        internal void HandleServerDroppedBeforeReady(PendingDownstreamRemoteServer pendingServer)
        {
            logger.Trace($"Server at [{pendingServer.Connection.RemoteEndPoints.Format()}]  did not identifiy itself in time.");

            lock (pendingDownstreamServers)
                pendingDownstreamServers.Remove(pendingServer);
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
                lock (groups)
                {
                    foreach (IModifiableServerGroup group in groups.Values)
                        group.Dispose();
                }

                lock (pendingDownstreamServers)
                {
                    foreach (PendingDownstreamRemoteServer pendingDownstreamServer in pendingDownstreamServers)
                        pendingDownstreamServer.Dispose();
                }
            }
        }
#pragma warning restore CS0628
    }
#endif
}
