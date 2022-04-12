/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Server.Metrics;
using System;
using System.Collections.Specialized;
using System.Net;

namespace DarkRift.Server
{
    /// <summary>
    ///     Data related to the listener's loading.
    /// </summary>
    public sealed class NetworkListenerLoadData : ExtendedPluginBaseLoadData
    {
        /// <summary>
        ///     The network listener manager to pass to the listener.
        /// </summary>
        public INetworkListenerManager NetworkListenerManager { get; set; }

        /// <summary>
        ///     The address this listener is listening on.
        /// </summary>
        public IPAddress Address { get; set; }

        /// <summary>
        ///     The port this listener is listening on.
        /// </summary>
        public ushort Port { get; set; }

        internal NetworkListenerLoadData(string name, IPAddress address, ushort port, DarkRiftServer server, NameValueCollection settings, Logger logger
#if PRO
            , MetricsCollector metricsCollector
#endif
            )
            : base(name, server, settings, logger
#if PRO
                  , metricsCollector
#endif
                  )
        {
            this.Address = address;
            this.Port = port;
            this.NetworkListenerManager = server.NetworkListenerManager;
        }

        /// <summary>
        ///     Creates new load data for a <see cref="NetworkListener"/>.
        /// </summary>
        /// <param name="name">The name of the listener.</param>
        /// <param name="settings">The settings to pass the listener.</param>
        /// <param name="serverInfo">The runtime details about the server.</param>
        /// <param name="threadHelper">The server's thread helper.</param>
        /// <param name="logger">The logger this plugin will use.</param>
        /// <remarks>
        ///     This constructor ensures that the legacy <see cref="WriteEventHandler"/> field is initialised to <see cref="Logger.Log(string, LogType, Exception)"/> for backwards compatibility.
        /// </remarks>
        public NetworkListenerLoadData(string name, NameValueCollection settings, DarkRiftInfo serverInfo, DarkRiftThreadHelper threadHelper, Logger logger)
            : base(name, settings, serverInfo, threadHelper, logger)
        {
        }

        /// <summary>
        ///     Creates new load data for a <see cref="NetworkListener"/>.
        /// </summary>
        /// <param name="name">The name of the listener.</param>
        /// <param name="settings">The settings to pass the listener.</param>
        /// <param name="serverInfo">The runtime details about the server.</param>
        /// <param name="threadHelper">The server's thread helper.</param>
        /// <param name="writeEventHandler"><see cref="WriteEventHandler"/> for logging.</param>
        [Obsolete("Use the constructor accepting Logger instead. This is kept for plugins using the legacy WriteEvent methods only.")]
        public NetworkListenerLoadData(string name, NameValueCollection settings, DarkRiftInfo serverInfo, DarkRiftThreadHelper threadHelper, WriteEventHandler writeEventHandler)
            : base(name, settings, serverInfo, threadHelper, writeEventHandler)
        {
        }
    }
}
