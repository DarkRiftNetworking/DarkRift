/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using System.Data.Common;
using System.Collections.Specialized;
using System.Threading;
using DarkRift.Server.Metrics;

namespace DarkRift.Server
{
    /// <summary>
    ///     The manager of all plugins on the server.
    /// </summary>
    internal sealed class PluginManager : ExtendedPluginManagerBase<Plugin>, IPluginManager
    {
        /// <summary>
        ///     The server that owns this plugin manager.
        /// </summary>
        private readonly DarkRiftServer server;

        /// <summary>
        ///     The server's datamanager.
        /// </summary>
        private readonly DataManager dataManager;

        /// <summary>
        ///     The server's log manager.
        /// </summary>
        private readonly LogManager logManager;

#if PRO
        /// <summary>
        ///     The server's log manager.
        /// </summary>
        private readonly MetricsManager metricsManager;
#endif

        /// <summary>
        ///     The server's plugin factory.
        /// </summary>
        private readonly PluginFactory pluginFactory;

#if PRO
        /// <summary>
        ///     Creates a new PluginManager.
        /// </summary>
        /// <param name="server">The server that owns this plugin manager.</param>
        /// <param name="dataManager">The server's datamanager.</param>
        /// <param name="logManager">The server's log manager.</param>
        /// <param name="pluginFactory">The server's plugin factory.</param>
        /// <param name="logger">The logger for this manager.</param>
        /// <param name="metricsManager">The server's metrics manager.</param>
        internal PluginManager(DarkRiftServer server, DataManager dataManager, LogManager logManager, MetricsManager metricsManager, PluginFactory pluginFactory, Logger logger)
#else
        /// <summary>
        ///     Creates a new PluginManager.
        /// </summary>
        /// <param name="server">The server that owns this plugin manager.</param>
        /// <param name="dataManager">The server's datamanager.</param>
        /// <param name="logManager">The server's log manager.</param>
        /// <param name="pluginFactory">The server's plugin factory.</param>
        /// <param name="logger">The logger for this manager.</param>
        internal PluginManager(DarkRiftServer server, DataManager dataManager, LogManager logManager, PluginFactory pluginFactory, Logger logger)
#endif
            : base (server, dataManager, pluginFactory, logger)
        {
            this.server = server;
            this.dataManager = dataManager;
            this.logManager = logManager;
#if PRO
            this.metricsManager = metricsManager;
#endif
            this.pluginFactory = pluginFactory;
        }

        /// <summary>
        ///     Loads the plugins found by the plugin factory.
        /// </summary>
        /// <param name="settings">The settings to load plugins with.</param>
        internal void LoadPlugins(ServerSpawnData.PluginsSettings settings)
        {
            Type[] types = pluginFactory.GetAllSubtypes(typeof(Plugin));
            
            foreach (Type type in types)
            {
                var s = settings.Plugins.FirstOrDefault(p => p.Type == type.Name);

                PluginLoadData loadData = new PluginLoadData(
                    type.Name, 
                    server, 
                    s?.Settings ?? new NameValueCollection(),
                    logManager.GetLoggerFor(type.Name),
#if PRO
                    metricsManager.GetMetricsCollectorFor(type.Name),
#endif
                    dataManager.GetResourceDirectory(type.Name)
                );

                if (s?.Load ?? settings.LoadByDefault)
                    LoadPlugin(type.Name, type, loadData, null, true);
            }
        }

        /// <inheritdoc/>
        public Plugin this[string name] => GetPlugin(name);

        /// <inheritdoc/>
        public Plugin GetPluginByName(string name)
        {
            return this[name];
        }

        /// <inheritdoc/>
        public T GetPluginByType<T>() where T : Plugin
        {
            return (T)GetPlugins().First((x) => x is T);
        }

        /// <inheritdoc/>
        public Plugin[] GetAllPlugins()
        {
            return GetPlugins().Where((p) => !p.Hidden).ToArray();
        }

        /// <inheritdoc/>
        public Plugin[] ActuallyGetAllPlugins()
        {
            return GetPlugins().ToArray();
        }
    }
}
