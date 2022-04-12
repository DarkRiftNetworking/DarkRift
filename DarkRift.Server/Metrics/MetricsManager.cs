/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;

namespace DarkRift.Server.Metrics
{
#if PRO
    internal sealed class MetricsManager : IDisposable, IMetricsManager
    {
        /// <summary>
        ///     The metric writer in use.
        /// </summary>
        /// <remarks>
        ///     Pro only.
        /// </remarks>
        public MetricsWriter MetricsWriter { get; private set; }

        /// <summary>
        ///     Whether to enable metrics that get emitted per message.
        /// </summary>
        public bool EnablePerMessageMetrics { get; }

        /// <summary>
        ///     The server this belongs to.
        /// </summary>
        private readonly DarkRiftServer server;

        /// <summary>
        ///     Creates a new metrics manager.
        /// </summary>
        /// <param name="server">The server we belong to.</param>
        /// <param name="settings">The settings to load from.</param>
        internal MetricsManager(DarkRiftServer server, ServerSpawnData.MetricsSettings settings)
        {
            this.server = server;
            this.EnablePerMessageMetrics = settings.EnablePerMessageMetrics;
        }

        /// <summary>
        ///     Loads the writers found by the plugin factory.
        /// </summary>
        /// <param name="settings">The settings to load the writer from.</param>
        /// <param name="pluginFactory">The server's plugin factory.</param>
        /// <param name="logManager">The server's log manager.</param>
        internal void LoadWriters(ServerSpawnData.MetricsSettings settings, PluginFactory pluginFactory, LogManager logManager)
        {
            if (MetricsWriter != null)
                throw new InvalidOperationException("Cannot load writers if writer is already present. This suggests that writers have already been loaded into the server.\n\nThis is likely an internal DR issue, please consider creating an issue here: https://github.com/DarkRiftNetworking/DarkRift/issues");

            if (settings.MetricsWriter != null && settings.MetricsWriter.Type != null)
            {
                MetricsWriterLoadData loadData = new MetricsWriterLoadData(settings.MetricsWriter.Type, server, settings.MetricsWriter.Settings, logManager.GetLoggerFor(settings.MetricsWriter.Type));

                MetricsWriter = pluginFactory.Create<MetricsWriter>(settings.MetricsWriter.Type, loadData, null);
            }
        }

        /// <inheritdoc />
        public MetricsCollector GetMetricsCollectorFor(string name)
        {
            return new MetricsCollector(name, MetricsWriter);
        }

        /// <inheritdoc />
        public MetricsCollector GetNoOpMetricsCollectorFor(string name)
        {
            return new MetricsCollector(name, null);
        }

        /// <inheritdoc />
        public MetricsCollector GetPerMessageMetricsCollectorFor(string name)
        {
            if (EnablePerMessageMetrics)
                return GetMetricsCollectorFor(name);
            else
                return GetNoOpMetricsCollectorFor(name);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    MetricsWriter?.Dispose();
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
