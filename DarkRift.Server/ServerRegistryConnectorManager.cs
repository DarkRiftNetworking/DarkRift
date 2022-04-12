/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Server.Metrics;
using System;
using System.Collections.Specialized;

namespace DarkRift.Server
{
#if PRO
    /// <summary>
    ///     The manager of the server registry connector on the server.
    /// </summary>
    internal sealed class ServerRegistryConnectorManager : IServerRegistryConnectorManager, IDisposable
    {
        /// <summary>
        ///     The server we belong to.
        /// </summary>
        private readonly DarkRiftServer server;

        /// <summary>
        ///     The server's plugin manager.
        /// </summary>
        private readonly PluginFactory pluginFactory;

        /// <summary>
        ///     The server's log manager.
        /// </summary>
        private readonly LogManager logManager;

        /// <summary>
        /// The server's metrics manager.
        /// </summary>
        private readonly MetricsManager metricsManager;

        /// <inheritdoc/>
        public ServerRegistryConnector ServerRegistryConnector { get; set; }

        /// <summary>
        ///     Creates a new ServerRegistryConnectorManager.
        /// </summary>
        /// <param name="server">The server that owns this manager.</param>
        /// <param name="pluginFactory">The server's plugin factory.</param>
        /// <param name="logManager">The server's log manager.</param>
        /// <param name="metricsManager">The server's metrics manager.</param>
        internal ServerRegistryConnectorManager(DarkRiftServer server, PluginFactory pluginFactory, LogManager logManager, MetricsManager metricsManager)
        {
            this.server = server;
            this.pluginFactory = pluginFactory;
            this.logManager = logManager;
            this.metricsManager = metricsManager;
        }

        /// <summary>
        ///     Loads the plugins found by the plugin factory.
        /// </summary>
        /// <param name="settings">The settings to load plugins with.</param>
        internal void LoadPlugins(ServerSpawnData.ServerRegistrySettings settings)
        {
            if (settings.ServerRegistryConnector?.Type != null)
            {
                ServerRegistryConnectorLoadData loadData = new ServerRegistryConnectorLoadData(
                    settings.ServerRegistryConnector.Type,
                    server,
                    settings.ServerRegistryConnector.Settings ?? new NameValueCollection(),
                    logManager.GetLoggerFor(settings.ServerRegistryConnector.Type),
                    metricsManager.GetMetricsCollectorFor(settings.ServerRegistryConnector.Type)
                );

                ServerRegistryConnector = pluginFactory.Create<ServerRegistryConnector>(settings.ServerRegistryConnector.Type, loadData, null);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ServerRegistryConnector?.Dispose();
                }

                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
#endif
}
