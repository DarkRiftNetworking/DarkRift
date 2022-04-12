/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using DarkRift.Server.Metrics;

namespace DarkRift.Server
{
#if PRO
    internal sealed class DownstreamServerGroup : ServerGroup<DownstreamRemoteServer>
    {
        /// <inheritdoc />
        public override ServerConnectionDirection Direction => ServerConnectionDirection.Downstream;

        /// <summary>
        ///     The server's thread helper.
        /// </summary>
        private readonly DarkRiftThreadHelper threadHelper;

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
        ///     Creates a new downstream connected server group
        /// </summary>
        /// <param name="name">The name of the group.</param>
        /// <param name="visibility">The groups visibility.</param>
        /// <param name="threadHelper">The server's thread helper.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="remoteServerLogger">The logger to pass to created remote servers.</param>
        /// <param name="metricsCollector">The metrics collector to use.</param>
        /// <param name="remoteServerMetricsCollector">The metrics collector to pass to created remote servers.</param>
        internal DownstreamServerGroup(string name, ServerVisibility visibility, DarkRiftThreadHelper threadHelper, Logger logger, Logger remoteServerLogger, MetricsCollector metricsCollector, MetricsCollector remoteServerMetricsCollector)
            : base(name, visibility, threadHelper, logger, metricsCollector)
        {
            this.threadHelper = threadHelper;
            this.logger = logger;
            this.remoteServerLogger = remoteServerLogger;
            this.remoteServerMetricsCollector = remoteServerMetricsCollector;
        }

        /// <inheritdoc />
        public override void HandleServerJoin(ushort id, string host, ushort port, IDictionary<string, string> properties)
        {
            DownstreamRemoteServer remoteServer = new DownstreamRemoteServer(id, host, port, this, threadHelper, remoteServerLogger, remoteServerMetricsCollector);

            AddServer(remoteServer);

            HandleServerJoinEvent(id, remoteServer);
        }

        /// <inheritdoc />
        public override void HandleServerLeave(ushort id)
        {
            DownstreamRemoteServer remoteServer = RemoveServer(id);

            HandleServerLeaveEvent(id, remoteServer);
        }

        /// <summary>
        ///     Handles a server disconnecting.
        /// </summary>
        /// <param name="remoteServer">The server that the connection was for.</param>
        /// <param name="exception">The exception that caused the disconnection.</param>
        internal void DisconnectedHandler(DownstreamRemoteServer remoteServer, Exception exception)
        {
            logger.Trace($"Lost connection to server {remoteServer.ID} on {remoteServer.Host}:{remoteServer.Port}.", exception);
        }
    }
#endif
}
