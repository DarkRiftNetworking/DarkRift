/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace DarkRift.Server
{
    /// <summary>
    ///     Base plugin manager for plugin managers handling <see cref="ExtendedPluginBase"/> types.
    /// </summary>
    /// <typeparam name="T">The type of plugin being managed.</typeparam>
    internal abstract class ExtendedPluginManagerBase<T> : PluginManagerBase<T>  where T : ExtendedPluginBase
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
        ///     The logger for this manager.
        /// </summary>
        private readonly Logger logger;

        /// <summary>
        ///     Creates a new ExtendedPluginManagerBase.
        /// </summary>
        /// <param name="server">The server that owns this plugin manager.</param>
        /// <param name="dataManager">The server's datamanager.</param>
        /// <param name="pluginFactory">The server's plugin factory.</param>
        /// <param name="logger">The logger for this manager.</param>
        internal ExtendedPluginManagerBase(DarkRiftServer server, DataManager dataManager, PluginFactory pluginFactory, Logger logger)
            : base(server, dataManager, pluginFactory)
        {
            this.server = server;
            this.dataManager = dataManager;
            this.logger = logger;
        }

        /// <summary>
        ///     Load the plugin given.
        /// </summary>
        /// <param name="name">The name of the plugin instance.</param>
        /// <param name="type">The plugin type to load.</param>
        /// <param name="pluginLoadData">The data for this plugin.</param>
        /// <param name="backupLoadData">The data for this plugin if the first fails.</param>
        /// <param name="createResourceDirectory">Whether to create a resource directory or not.</param>
        protected override T LoadPlugin(string name, Type type, PluginBaseLoadData pluginLoadData, PluginLoadData backupLoadData, bool createResourceDirectory)
        {
            T plugin = base.LoadPlugin(name, type, pluginLoadData, backupLoadData, createResourceDirectory);

            HandleThreadSafe(plugin);

            HandleInstallUpgrade(plugin);

            return plugin;
        }

        /// <summary>
        ///     Load the plugin given.
        /// </summary>
        /// <param name="name">The name of the plugin instance.</param>
        /// <param name="type">The plugin type to load.</param>
        /// <param name="pluginLoadData">The data for this plugin.</param>
        /// <param name="backupLoadData">The data for this plugin if the first fails.</param>
        /// <param name="createResourceDirectory">Whether to create a resource directory or not.</param>
        protected override T LoadPlugin(string name, string type, PluginBaseLoadData pluginLoadData, PluginLoadData backupLoadData, bool createResourceDirectory)
        {
            T plugin = base.LoadPlugin(name, type, pluginLoadData, backupLoadData, createResourceDirectory);

            HandleThreadSafe(plugin);

            HandleInstallUpgrade(plugin);

            return plugin;
        }

        /// <summary>
        ///     Make server threadsafe if necessary
        /// </summary>
        /// <param name="plugin">The plugin to check.</param>
        private void HandleThreadSafe(T plugin)
        {
            if (!plugin.ThreadSafe)
            {
                logger.Trace($"Plugin '{plugin.Name}' has requested that DarkRift operates in thread safe mode.");
                server.MakeThreadSafe();
            }
        }

        /// <summary>
        ///     Install/upgrade the loaded plugin.
        /// </summary>
        /// <param name="plugin">The plugin to check.</param>
        private void HandleInstallUpgrade(T plugin)
        {
            PluginRecord oldRecord = dataManager.ReadAndSetPluginRecord(plugin.Name, plugin.Version);

            if (oldRecord == null)
            {
                plugin.Install(new InstallEventArgs());

                if (!plugin.Hidden)
                    logger.Info($"Installed plugin {plugin.Name} version {plugin.Version}");
            }
            else if (oldRecord.Version == plugin.Version)
            {
                if (!plugin.Hidden)
                    logger.Info($"Loaded plugin {plugin.Name} version {plugin.Version}");
            }
            else
            {
                plugin.Upgrade(new UpgradeEventArgs(oldRecord.Version));

                if (!plugin.Hidden)
                    logger.Info($"Upgraded plugin {plugin.Name} version {plugin.Version} from version {oldRecord.Version}");
            }
        }

        /// <inheritdoc/>
        public Version GetInstalledVersion(string pluginName)
        {
            return dataManager.ReadPluginRecord(pluginName)?.Version;
        }

        /// <summary>
        ///     Uninstalls a plugin by name, it cannot be currently operating.
        /// </summary>
        /// <param name="name">The name of the plugin to uninstall.</param>
        internal void Uninstall(string name)
        {
            if (!ContainsPlugin(name))
            {
                logger.Trace($"Uninstalling plugin {name} from the server.");

                dataManager.DeletePluginRecord(name);

                dataManager.DeleteResourceDirectory(name);
            }
            else
            {
                throw new InvalidOperationException("The plugin cannot be uninstalled as it is currently loaded. Consider restarting the server without this plugin.");
            }
        }

        /// <summary>
        ///     Invokes the Loaded event on all plugins.
        /// </summary>
        /// <remarks>
        ///     <see cref="DarkRiftServer.Loaded"/> must be true when this is invoked.
        /// </remarks>
        internal void Loaded()
        {
            foreach (T plugin in GetPlugins())
                plugin.Loaded(new LoadedEventArgs());
        }
    }
}
