/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Server.Metrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace DarkRift.Server
{
    /// <summary>
    ///     Base class for all plugins providing network functionality.
    /// </summary>
    //TODO this inherits PluginBase but the load data inherits ExtendedPluginBaseLoadData?!?
    public abstract class NetworkListener : PluginBase
    {
        /// <summary>
        ///     The address this listener is operating on.
        /// </summary>
        public IPAddress Address { get; protected set; }

        /// <summary>
        ///     The port this listener is operating on.
        /// </summary>
        public ushort Port { get; protected set; }

#if PRO
        /// <summary>
        /// The server's metrics manager.
        /// </summary>
        /// <remarks>
        ///     Pro only.
        /// </remarks>
        protected IMetricsManager MetricsManager { get; }

        /// <summary>
        ///     Metrics collector for the plugin.
        /// </summary>
        /// <remarks>
        ///     Pro only.
        /// </remarks>
        protected MetricsCollector MetricsCollector { get; }
#endif

        /// <summary>
        ///     Event fired when a new connection is registered.
        /// </summary>
        // TODO improve event handler type?
        internal event Action<NetworkServerConnection> RegisteredConnection;

        /// <summary>
        ///     Constructor for a network listener.
        /// </summary>
        /// <param name="pluginLoadData">The load data for the listener plugin.</param>
        public NetworkListener(NetworkListenerLoadData pluginLoadData) : base(pluginLoadData)
        {
            Address = pluginLoadData.Address;
            Port = pluginLoadData.Port;
#if PRO
            MetricsManager = pluginLoadData.MetricsManager;
            MetricsCollector = pluginLoadData.MetricsCollector;
#endif
        }

        /// <summary>
        ///     Writes an event to the server's logs.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="logType">The type of message to write.</param>
        /// <param name="exception">The exception that occurred (if there was one).</param>
        [Obsolete("Use the Logger class to write logs. Use the Logger property to continue using your plugin's default logger or see ILogManager.GetLoggerFor(string) to create a logger for a specific purpose.")]
        protected void WriteEvent(string message, LogType logType, Exception exception = null)
        {
            Logger.Log(message, logType, exception);
        }

        /// <summary>
        ///     Registers a new connection to the server.
        /// </summary>
        /// <param name="connection">The new connection.</param>
        protected void RegisterConnection(NetworkServerConnection connection)
        {
            Action<NetworkServerConnection> handler = RegisteredConnection;
            if (handler != null)
            {
                handler.Invoke(connection);
            }
            else
            {
                Logger.Error("A connection was registered by the network listener while no hooks were subscribed to handle the registration. The connection has been dropped. This suggests the network listener is erroneously accepting connections before the StartListening() method has been called.");

                connection.Disconnect();
            }
        }

        /// <summary>
        ///     Starts the listener listening on the network.
        /// </summary>
        public abstract void StartListening();
    }
}
