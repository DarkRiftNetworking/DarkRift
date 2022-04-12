/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Dispatching;
using System;

namespace DarkRift.Server
{
    /// <summary>
    ///     Base class for all DarkRift plugins.
    /// </summary>
    public abstract class PluginBase : IDisposable
    {
        /// <summary>
        ///     The name assigned to this plugin.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The version of this plugin.
        /// </summary>
        public abstract Version Version { get; }

        /// <summary>
        ///     Should this plugin be hidden from output?
        /// </summary>
        internal virtual bool Hidden { get; }

        /// <summary>
        ///     The database manager for the server.
        /// </summary>
        [Obsolete("Use plugin configuration settings.")]
        public IDatabaseManager DatabaseManager { get; }

        /// <summary>
        ///     The dispatcher for this server.
        /// </summary>
        public IDispatcher Dispatcher { get; }
        
        /// <summary>
        ///     Information about this server.
        /// </summary>
        public DarkRiftInfo ServerInfo { get; }
        
        /// <summary>
        ///     The thread helper for the server.
        /// </summary>
        public DarkRiftThreadHelper ThreadHelper { get; }

        /// <summary>
        ///     The server's log manager.
        /// </summary>
#if PRO
    public
#else
        internal
#endif
        ILogManager LogManager { get; }

        /// <summary>
        ///     Default logger for the plugin.
        /// </summary>
        /// <seealso cref="ILogManager.GetLoggerFor(string)"/>
        protected Logger Logger { get; }

        /// <summary>
        ///     The DarkRift server we belong to.
        /// </summary>
        internal DarkRiftServer Server { get; set; }

        /// <summary>
        ///     Creates a new plugin base using the given plugin load data.
        /// </summary>
        /// <param name="pluginLoadData"></param>
        public PluginBase(PluginBaseLoadData pluginLoadData)
        {
            this.Name = pluginLoadData.Name;
#pragma warning disable CS0618 // Type or member is obsolete
            this.DatabaseManager = pluginLoadData.DatabaseManager;
#pragma warning restore CS0618 // Type or member is obsolete
            this.Dispatcher = pluginLoadData.Dispatcher;
            this.ServerInfo = pluginLoadData.ServerInfo;
            this.ThreadHelper = pluginLoadData.ThreadHelper;
            this.LogManager = pluginLoadData.LogManager;
            this.Logger = pluginLoadData.Logger;
            this.Server = pluginLoadData.Server;
        }

#if PRO
        /// <summary>
        ///     Creates a new timer that will invoke the callback a single time.
        /// </summary>
        /// <param name="delay">The delay in milliseconds before invoking the callback.</param>
        /// <param name="callback">The callback to invoke.</param>
        /// <returns>The new timer.</returns>
        public Timer CreateOneShotTimer(int delay, Action<Timer> callback)
        {
            return ThreadHelper.RunAfterDelay(delay, callback);
        }

        /// <summary>
        ///     Creates a new timer that will invoke the callback repeatedly until stopped.
        /// </summary>
        /// <param name="initialDelay">The delay in milliseconds before invoking the callback the first time.</param>
        /// <param name="repetitionPeriod">The delay in milliseconds between future invocations.</param>
        /// <param name="callback">The callback to invoke.</param>
        /// <returns>The new timer.</returns>
        public Timer CreateTimer(int initialDelay, int repetitionPeriod, Action<Timer> callback)
        {
            return ThreadHelper.CreateTimer(initialDelay, repetitionPeriod, callback);
        }
#endif

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        ///     Handles disposing of the plugin.
        /// </summary>
        /// <param name="disposing">If the plugin is disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    
                }

                disposedValue = true;
            }
        }

        /// <summary>
        ///     Disposes of the plugin.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
