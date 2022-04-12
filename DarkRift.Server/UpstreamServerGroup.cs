/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using DarkRift.Client;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using DarkRift.Server.Metrics;

namespace DarkRift.Server
{
#if PRO
    internal sealed class UpstreamServerGroup : ServerGroup<UpstreamRemoteServer>
    {
        /// <inheritdoc />
        public override ServerConnectionDirection Direction => ServerConnectionDirection.Upstream;

        /// <summary>
        ///     The server's thread helper.
        /// </summary>
        private readonly DarkRiftThreadHelper threadHelper;

        /// <summary>
        ///     The remote server manager for the server.
        /// </summary>
        private readonly RemoteServerManager remoteServerManager;

        /// <summary>
        ///     The server's registry connector manager.
        /// </summary>
        private readonly ServerRegistryConnectorManager serverRegistryConnectorManager;

        /// <summary>
        ///     The number of times to try to retry before considering a server unconnectable.
        /// </summary>
        private readonly int reconnectAttempts;

        /// <summary>
        ///     The logger to use.
        /// </summary>
        private readonly Logger logger;

        /// <summary>
        /// The logger to pass to created remote servers.
        /// </summary>
        private readonly Logger remoteServerLogger;

        /// <summary>
        /// The metrics collector to pass to created remote servers.
        /// </summary>
        private readonly MetricsCollector remoteServerMetricsCollector;

        /// <summary>
        ///     Creates a new upstream connected server group
        /// </summary>
        /// <param name="name">The name of the group.</param>
        /// <param name="visibility">The groups visibility.</param>
        /// <param name="threadHelper">The server's thread helper.</param>
        /// <param name="serverRegistryConnectorManager">The server's registry connector manager</param>
        /// <param name="remoteServerManager">The remote server manager for the server.</param>
        /// <param name="reconnectAttempts">The number of times to attempt to reconnect to a server before considering it unconnectable.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="remoteServerLogger">The logger to pass to created remote servers.</param>
        /// <param name="metricsCollector">The metrics collector to use.</param>
        /// <param name="remoteServerMetricsCollector">The metrics collector to pass to created remote servers.</param>
        internal UpstreamServerGroup(string name, ServerVisibility visibility, DarkRiftThreadHelper threadHelper, ServerRegistryConnectorManager serverRegistryConnectorManager, RemoteServerManager remoteServerManager, int reconnectAttempts, Logger logger, Logger remoteServerLogger, MetricsCollector metricsCollector, MetricsCollector remoteServerMetricsCollector)
            : base(name, visibility, threadHelper, logger, metricsCollector)
        {
            this.threadHelper = threadHelper;
            this.serverRegistryConnectorManager = serverRegistryConnectorManager;
            this.remoteServerManager = remoteServerManager;
            this.reconnectAttempts = reconnectAttempts;
            this.logger = logger;
            this.remoteServerLogger = remoteServerLogger;
            this.remoteServerMetricsCollector = remoteServerMetricsCollector;
        }

        /// <inheritdoc />
        public override void HandleServerJoin(ushort id, string host, ushort port, IDictionary<string, string> properties)
        {
            UpstreamRemoteServer remoteServer = new UpstreamRemoteServer(remoteServerManager, id, host, port, this, threadHelper, remoteServerLogger, remoteServerMetricsCollector);

            AddServer(remoteServer);

            HandleServerJoinEvent(id, remoteServer);

            threadHelper.ExponentialBackoff(
                (context) =>
                {
                    logger.Trace($"Connecting to server {id} on {host}:{port}. Attempt {context.Tries}.");

                    remoteServer.Connect();

                    logger.Info($"Connected to server {id} on {host}:{port}.");
                },
                reconnectAttempts,
                (lastException) =>
                {
                    logger.Warning($"Could not connect to server {id} on {host}:{port}.", lastException);

                    // Inform the registry plugin. We don't remove this server as that's up to the registry connector to do
                    serverRegistryConnectorManager.ServerRegistryConnector.HandleConnectionFailure(id);
                }
            );
        }

        /// <inheritdoc />
        public override void HandleServerLeave(ushort id)
        {
            UpstreamRemoteServer remoteServer = RemoveServer(id);

            HandleServerLeaveEvent(id, remoteServer);
        }

        /// <summary>
        ///     Retrieves a connection to the specified endpoint.
        /// </summary>
        /// <param name="address">The address to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        /// <returns>The created connection.</returns>
        internal NetworkClientConnection GetConnection(IPAddress address, ushort port)
        {
            //TODO support custom listeners & settings
            return new BichannelClientConnection(address, port, true);
        }

        /// <summary>
        ///     Handles a server disconnecting.
        /// </summary>
        /// <param name="lostConnection">The connection that was lost.</param>
        /// <param name="remoteServer">The server that the connection was for.</param>
        /// <param name="exception">The exception that caused the disconnection.</param>
        internal void DisconnectedHandler(NetworkClientConnection lostConnection, UpstreamRemoteServer remoteServer, Exception exception)
        {
            logger.Trace($"Lost connection to server {remoteServer.ID} on {remoteServer.Host}:{remoteServer.Port}.", exception);

            lostConnection.Disconnected = null;
            threadHelper.ExponentialBackoff(
                (context) =>
                {
                    logger.Trace($"Reconnecting to server {remoteServer.ID} on {remoteServer.Host}:{remoteServer.Port}. Attempt {context.Tries}.");

                    remoteServer.Connect();

                    logger.Info($"Reconnected to server {remoteServer.ID} on {remoteServer.Host}:{remoteServer.Port}.");
                },
                reconnectAttempts,
                (lastException) =>
                {
                    logger.Warning($"Could not reconnect to server {remoteServer.ID} on {remoteServer.Host}:{remoteServer.Port}.", lastException);

                    // Inform the registry plugin. We don't remove this server as that's up to the registry connector to do
                    serverRegistryConnectorManager.ServerRegistryConnector.HandleConnectionFailure(remoteServer.ID);
                }
            );
        }
    }
#endif
}
