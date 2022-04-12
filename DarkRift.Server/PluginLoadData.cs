/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Dispatching;
using DarkRift.Server.Metrics;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace DarkRift.Server
{
    /// <summary>
    ///     Data related to the plugin's loading.
    /// </summary>
    public sealed class PluginLoadData : ExtendedPluginBaseLoadData
    {
        /// <summary>
        ///     The client manager to pass to the server.
        /// </summary>
        public IClientManager ClientManager { get; set; }
        
        /// <summary>
        ///     The plugin manager to pass to the plugin.
        /// </summary>
        public IPluginManager PluginManager { get; set; }

        /// <summary>
        ///     The network listener manager to pass to the plugin.
        /// </summary>
        public INetworkListenerManager NetworkListenerManager { get; set; }

#if PRO
        /// <summary>
        ///     The server regsitry connector manager to pass to the plugin.
        /// </summary>
        public IServerRegistryConnectorManager ServerRegistryConnectorManager { get; set; }

        /// <summary>
        ///     The server manager to pass to the plugin.
        /// </summary>
        public IRemoteServerManager RemoteServerManager { get; set; }
#endif

        /// <summary>
        ///     The resource directory to pass to the plugin.
        /// </summary>
        public string ResourceDirectory { get; set; }

        internal PluginLoadData (string name, DarkRiftServer server, NameValueCollection settings, Logger logger,
#if PRO
            MetricsCollector metricsCollector,
#endif
            string resourceDirectory)
            : base(name, server, settings, logger
#if PRO
                  , metricsCollector
#endif
                  )
        {
            this.ClientManager = server.ClientManager;
            this.PluginManager = server.PluginManager;
            this.NetworkListenerManager = server.NetworkListenerManager;
#if PRO
            this.ServerRegistryConnectorManager = server.ServerRegistryConnectorManager;
            this.RemoteServerManager = server.RemoteServerManager;
#endif
            this.ResourceDirectory = resourceDirectory;
        }

        /// <summary>
        ///     Creates new load data with the given properties.
        /// </summary>
        /// <param name="name">The name of the plugin.</param>
        /// <param name="settings">The settings to pass the plugin.</param>
        /// <param name="serverInfo">The runtime details about the server.</param>
        /// <param name="threadHelper">The server's thread helper.</param>
        /// <param name="logger">The logger this plugin will use.</param>
        /// <param name="resourceDirectory">The directory to place this plugin's resources.</param>
        /// <remarks>
        ///     This constructor ensures that the legacy <see cref="WriteEventHandler"/> field is initialised to <see cref="Logger.Log(string, LogType, Exception)"/> for backwards compatibility.
        /// </remarks>
        public PluginLoadData(string name, NameValueCollection settings, DarkRiftInfo serverInfo, DarkRiftThreadHelper threadHelper, Logger logger, string resourceDirectory)
            : base(name, settings, serverInfo, threadHelper, logger)
        {
            this.ResourceDirectory = resourceDirectory;
        }

        /// <summary>
        ///     Creates new load data with the given properties.
        /// </summary>
        /// <param name="name">The name of the plugin.</param>
        /// <param name="settings">The settings to pass the plugin.</param>
        /// <param name="serverInfo">The runtime details about the server.</param>
        /// <param name="threadHelper">The server's thread helper.</param>
        /// <param name="writeEventHandler"><see cref="WriteEventHandler"/> for logging.</param>
        /// <param name="resourceDirectory">The directory to place this plugin's resources.</param>
        [Obsolete("Use the constructor accepting Logger instead. This is kept for plugins using the legacy WriteEvent methods only.")]
        public PluginLoadData(string name, NameValueCollection settings, DarkRiftInfo serverInfo, DarkRiftThreadHelper threadHelper, WriteEventHandler writeEventHandler, string resourceDirectory)
            : base(name, settings, serverInfo, threadHelper, writeEventHandler)
        {
            this.ResourceDirectory = resourceDirectory;
        }
    }
}
