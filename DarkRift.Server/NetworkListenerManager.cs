/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Server.Metrics;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;

namespace DarkRift.Server
{
    /// <summary>
    ///     Manager for all <see cref="NetworkListener"/> plugins.
    /// </summary>
    internal sealed class NetworkListenerManager : PluginManagerBase<NetworkListener>, INetworkListenerManager
    {
        /// <summary>
        ///     The DarkRift server.
        /// </summary>
        private readonly DarkRiftServer server;

        /// <summary>
        ///     The server's log manager.
        /// </summary>
        private readonly LogManager logManager;

#if PRO
        /// <summary>
        ///     The server's metrics manager.
        /// </summary>
        private readonly MetricsManager metricsManager;
#endif

#if PRO
        /// <summary>
        ///     Creates a new NetworkListenerManager.
        /// </summary>
        /// <param name="server">The server that owns this plugin manager.</param>
        /// <param name="logManager">The server's log manager.</param>
        /// <param name="dataManager">The server's datamanager.</param>
        /// <param name="pluginFactory">The server's plugin factory.</param>
        /// <param name="metricsManager">The server's metrics manager.</param>
        internal NetworkListenerManager(DarkRiftServer server, LogManager logManager, MetricsManager metricsManager, DataManager dataManager, PluginFactory pluginFactory)
#else
        /// <summary>
        ///     Creates a new NetworkListenerManager.
        /// </summary>
        /// <param name="server">The server that owns this plugin manager.</param>
        /// <param name="logManager">The server's log manager.</param>
        /// <param name="dataManager">The server's datamanager.</param>
        /// <param name="pluginFactory">The server's plugin factory.</param>
        internal NetworkListenerManager(DarkRiftServer server, LogManager logManager, DataManager dataManager, PluginFactory pluginFactory)
#endif
            : base(server, dataManager, pluginFactory)
        {
            this.logManager = logManager;
#if PRO
            this.metricsManager = metricsManager;
#endif
            this.server = server;
        }

        /// <summary>
        ///     Loads the plugins found by the plugin factory.
        /// </summary>
        /// <param name="settings">The settings to load plugins with.</param>
        internal void LoadNetworkListeners(ServerSpawnData.ListenersSettings settings)
        {
            foreach (ServerSpawnData.ListenersSettings.NetworkListenerSettings s in settings.NetworkListeners)
            {
                NetworkListenerLoadData loadData = new NetworkListenerLoadData(
                    s.Name,
                    s.Address,
                    s.Port,
                    server,
                    s.Settings,
                    logManager.GetLoggerFor(s.Name)
#if PRO
                    , metricsManager.GetMetricsCollectorFor(s.Name)
#endif
                );

                LoadPlugin(s.Name, s.Type, loadData, null, false);
            }
        }

        /// <summary>
        ///     Load the listener given.
        /// </summary>
        /// <param name="type">The plugin type to load.</param>
        /// <param name="name">The name of the plugins instance.</param>
        /// <param name="address">The address to listen on.</param>
        /// <param name="port">The port to listen on.</param>
        /// <param name="settings">The settings for this plugin.</param>
        internal NetworkListener LoadNetworkListener(Type type, string name, IPAddress address, ushort port, NameValueCollection settings)
        {
            NetworkListenerLoadData loadData = new NetworkListenerLoadData(
                type.Name,
                address, 
                port,
                server,
                settings,
                logManager.GetLoggerFor(name)
#if PRO
                , metricsManager.GetMetricsCollectorFor(name)
#endif
            );

            return LoadPlugin(name, type, loadData, null, false);
        }

        /// <summary>
        ///     Starts all <see cref="NetworkListener">NetworkListeners</see> listening.
        /// </summary>
        internal void StartListening()
        {
            foreach (NetworkListener listener in GetPlugins())
                listener.StartListening();
        }

        /// <inheritdoc/>
        public NetworkListener this[string name] => GetPlugin(name);

        /// <inheritdoc/>
        public NetworkListener GetNetworkListenerByName(string name)
        {
            return this[name];
        }

        /// <inheritdoc/>
        public T[] GetNetworkListenersByType<T>() where T : NetworkListener
        {
            return GetPlugins().Where((x) => x is T).Cast<T>().ToArray();
        }

        /// <inheritdoc/>
        public NetworkListener[] GetNetworkListeners()
        {
            return GetPlugins().Where((p) => !p.Hidden).ToArray();
        }

        /// <inheritdoc/>
        public NetworkListener[] GetAllNetworkListeners()
        {
            return GetPlugins().ToArray();
        }
    }
}
