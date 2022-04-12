/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Server.Metrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkRift.Server
{
    /// <summary>
    ///     Base class for plugins with additional privileges.
    /// </summary>
    public abstract class ExtendedPluginBase : PluginBase
    {
        /// <summary>
        ///     Is this plugin able to handle multithreaded events?
        /// </summary>
        /// <remarks>
        ///     Enabling this option allows DarkRift to send messages to your plugin from multiple threads simultaneously, 
        ///     greatly increasing performance. Do not enable this unless you are confident that you understand 
        ///     multithreading else you will find yourself with a variety of unfriendly problems to fix!
        /// </remarks>
        public abstract bool ThreadSafe { get; }

        /// <summary>
        ///     The commands the plugin has.
        /// </summary>
        /// <remarks>
        ///     This is an array of commands that can be executed by this plugin and will be searched through when the 
        ///     command is executed. Changes to this array will be reflected instantly by the command system.
        /// </remarks>
        public virtual Command[] Commands => new Command[0];

#if PRO
        /// <summary>
        /// The server's metrics manager.
        /// </summary>
        /// <remarks>
        ///     Pro only.
        /// </remarks>
        public IMetricsManager MetricsManager { get; }

        /// <summary>
        ///     Metrics collector for the plugin.
        /// </summary>
        /// <remarks>
        ///     Pro only.
        /// </remarks>
        protected MetricsCollector MetricsCollector { get; }
#endif

        /// <summary>
        ///     The handler for writing events.
        /// </summary>
        private readonly WriteEventHandler writeEventHandler;

        /// <summary>
        ///     Constructor taking extended load data.
        /// </summary>
        /// <param name="pluginLoadData">The load data for the plugins.</param>
        public ExtendedPluginBase(ExtendedPluginBaseLoadData pluginLoadData)
            : base(pluginLoadData)
        {
#pragma warning disable CS0618 // Implementing obsolete functionality
            writeEventHandler = pluginLoadData.WriteEventHandler;
#pragma warning restore CS0618

#if PRO
            MetricsManager = pluginLoadData.MetricsManager;
            MetricsCollector= pluginLoadData.MetricsCollector;
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
            writeEventHandler(message, logType, exception);
        }

        /// <summary>
        ///     Method that will be called when the server and all plugins have loaded.
        /// </summary>
        /// <param name="args">The details of the load.</param>
        /// <remarks>Pro only.</remarks>
#if PRO
        protected
#endif
        internal virtual void Loaded(LoadedEventArgs args)
        { }

        /// <summary>
        ///     Method that will be called when the plugin is installed.
        /// </summary>
        /// <param name="args">The details of the installation.</param>
        /// <remarks>Pro only.</remarks>
#if PRO
        protected
#endif
        internal virtual void Install(InstallEventArgs args)
        { }

        /// <summary>
        ///     Method that will be called when the plugin is upgraded.
        /// </summary>
        /// <param name="args">The details of the upgrade.</param>
        /// <remarks>Pro only.</remarks>
#if PRO
        protected
#endif
        internal virtual void Upgrade(UpgradeEventArgs args)
        { }
    }
}
