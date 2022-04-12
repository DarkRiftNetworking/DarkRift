/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Server.Metrics;
using System.Collections.Specialized;

namespace DarkRift.Server
{
#if PRO
    /// <summary>
    ///     Plugin load data for server registry connectors
    /// </summary>
    public class ServerRegistryConnectorLoadData : ExtendedPluginBaseLoadData
    {
        /// <summary>
        ///     The server registry connector manager to pass to the plugin.
        /// </summary>
        public IServerRegistryConnectorManager ServerRegistryConnectorManager { get; set; }

        /// <summary>
        ///     The server manager to pass to the plugin.
        /// </summary>
        public IRemoteServerManager RemoteServerManager { get; set; }

        internal ServerRegistryConnectorLoadData(string name, DarkRiftServer server, NameValueCollection settings, Logger logger, MetricsCollector metricsCollector)
            : base(name, server, settings, logger, metricsCollector)
        {
            this.ServerRegistryConnectorManager = server.ServerRegistryConnectorManager;
            this.RemoteServerManager = server.RemoteServerManager;
        }

        /// <summary>
        ///     Creates new load data with the given properties.
        /// </summary>
        /// <param name="name">The name of the connector.</param>
        /// <param name="settings">The settings to pass the connector.</param>
        /// <param name="serverInfo">The runtime details about the server.</param>
        /// <param name="threadHelper">The server's thread helper.</param>
        /// <param name="logger">The logger to use.</param>
        public ServerRegistryConnectorLoadData(string name, NameValueCollection settings, DarkRiftInfo serverInfo, DarkRiftThreadHelper threadHelper, Logger logger)
            : base(name, settings, serverInfo, threadHelper, logger)
        {
        }
    }
#endif
}
