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
using System.Text;

namespace DarkRift.Server
{
    /// <summary>
    ///     Base load data class for plugins inheriting <see cref="ExtendedPluginBase"/>.
    /// </summary>
    public abstract class ExtendedPluginBaseLoadData : PluginBaseLoadData
    {
#if PRO
        /// <summary>
        ///     The server's metrics manager.
        /// </summary>
        /// <remarks>
        ///     Pro only.
        /// </remarks>
        public IMetricsManager MetricsManager { get; set; }

        /// <summary>
        ///     The metrics collector this plugin will use.
        /// </summary>
        /// <remarks>
        ///     Pro only.
        /// </remarks>
        public MetricsCollector MetricsCollector { get; set; }
#endif

        /// <summary>
        ///     The handler for writing events via <see cref="ExtendedPluginBase.WriteEvent(string, LogType, Exception)"/>.
        /// </summary>
        [Obsolete("Use Logger instead. This is kept for plugins using the legacy WriteEvent methods only.")]
        public WriteEventHandler WriteEventHandler { get; set; }
        
        internal ExtendedPluginBaseLoadData(string name, DarkRiftServer server, NameValueCollection settings, Logger logger
#if PRO
            , MetricsCollector metricsCollector
#endif
            )
            : base(name, server, settings, logger)
        {
#pragma warning disable CS0618 // Implementing obsolete functionality
            if (logger != null)
                WriteEventHandler = logger.Log;
#pragma warning restore CS0618

#if PRO
            this.MetricsManager = server.MetricsManager;
            MetricsCollector = metricsCollector;
#endif
        }

        /// <summary>
        ///     Creates new load data with the given properties.
        /// </summary>
        /// <param name="name">The name of the plugin.</param>
        /// <param name="settings">The settings to pass the plugin.</param>
        /// <param name="serverInfo">The runtime details about the server.</param>
        /// <param name="threadHelper">The server's thread helper.</param>
        /// <param name="logger">The logger this plugin will use.</param>
        /// <remarks>
        ///     This constructor ensures that the legacy <see cref="WriteEventHandler"/> field is initialised to <see cref="Logger.Log(string, LogType, Exception)"/> for backwards compatibility.
        /// </remarks>
        public ExtendedPluginBaseLoadData(string name, NameValueCollection settings, DarkRiftInfo serverInfo, DarkRiftThreadHelper threadHelper, Logger logger)
            : base(name, settings, serverInfo, threadHelper)
        {
#pragma warning disable CS0618 // Implementing obsolete functionality
            if (logger != null)
                WriteEventHandler = logger.Log;
#pragma warning restore CS0618

            Logger = logger;
        }

        /// <summary>
        ///     Creates new load data with the given properties.
        /// </summary>
        /// <param name="name">The name of the plugin.</param>
        /// <param name="settings">The settings to pass the plugin.</param>
        /// <param name="serverInfo">The runtime details about the server.</param>
        /// <param name="threadHelper">The server's thread helper.</param>
        /// <param name="writeEventHandler"><see cref="WriteEventHandler"/> for logging.</param>
        [Obsolete("Use the constructor accepting Logger instead. This is kept for plugins using the legacy WriteEvent methods only.")]
        public ExtendedPluginBaseLoadData(string name, NameValueCollection settings, DarkRiftInfo serverInfo, DarkRiftThreadHelper threadHelper, WriteEventHandler writeEventHandler)
            : base(name, settings, serverInfo, threadHelper)
        {
            WriteEventHandler = writeEventHandler;
        }
    }
}
