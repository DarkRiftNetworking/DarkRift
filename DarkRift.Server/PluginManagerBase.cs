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
    internal abstract class PluginManagerBase<T> : IDisposable where T : PluginBase
    {
        /// <summary>
        ///     The plugins that have been loaded.
        /// </summary>
        private readonly Dictionary<string, T> plugins = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     The DarkRift server.
        /// </summary>
        private readonly DarkRiftServer server;

        /// <summary>
        ///     The server's datamanager.
        /// </summary>
        private readonly DataManager dataManager;

        /// <summary>
        ///     The server's plugin factory.
        /// </summary>
        private readonly PluginFactory pluginFactory;

        /// <summary>
        ///     Creates a new PluginManagerBase.
        /// </summary>
        /// <param name="server">The server we are part of.</param>
        /// <param name="dataManager">The server's data manager.</param>
        /// <param name="pluginFactory">The server's plugin factory.</param>
        internal PluginManagerBase(DarkRiftServer server, DataManager dataManager, PluginFactory pluginFactory)
        {
            this.server = server;
            this.dataManager = dataManager;
            this.pluginFactory = pluginFactory;
        }
        
        /// <summary>
        ///     Load the plugin given.
        /// </summary>
        /// <param name="name">The name of the plugin instance.</param>
        /// <param name="type">The plugin type to load.</param>
        /// <param name="pluginLoadData">The data for this plugin.</param>
        /// <param name="backupLoadData">The data for this plugin if the first fails.</param>
        /// <param name="createResourceDirectory">Whether to create a resource directory or not.</param>
        protected virtual T LoadPlugin(string name, Type type, PluginBaseLoadData pluginLoadData, PluginLoadData backupLoadData, bool createResourceDirectory)
        {
            //Ensure the resource directory is present
            if (createResourceDirectory)
                dataManager.CreateResourceDirectory(type.Name);
            
            T plugin = pluginFactory.Create<T>(type, pluginLoadData);

            plugins.Add(name, plugin);

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
        protected virtual T LoadPlugin(string name, string type, PluginBaseLoadData pluginLoadData, PluginLoadData backupLoadData, bool createResourceDirectory)
        {
            //Ensure the resource directory is present
            if (createResourceDirectory)
                dataManager.CreateResourceDirectory(type);

            T plugin = pluginFactory.Create<T>(type, pluginLoadData);

            plugins.Add(name, plugin);

            return plugin;
        }

        /// <summary>
        ///     The plugins loaded.
        /// </summary>
        protected IEnumerable<T> GetPlugins()
        {
            if (!server.Loaded)
                throw new InvalidOperationException($"You cannot search for plugins during initialization, use the Loaded event instead: {server.ServerInfo.DocumentationRoot}advanced/installs_and_upgrades.html#loaded");
            
            return plugins.Values;
        }

        /// <summary>
        ///     Gets a plugin by name.
        /// </summary>
        protected T GetPlugin(string name)
        {
            if (!server.Loaded)
                throw new InvalidOperationException($"You cannot search for plugins during initialization, use the Loaded event instead: {server.ServerInfo.DocumentationRoot}advanced/installs_and_upgrades.html#loaded");

            return plugins[name];
        }

        /// <summary>
        ///     Searches for the given plugin name.
        /// </summary>
        /// <param name="name">The name of the plugin.</param>
        /// <returns>Whether the plugins was found.</returns>
        protected bool ContainsPlugin(string name)
        {
            return plugins.ContainsKey(name);
        }

        /// <summary>
        ///     Disposes of this PluginManager.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Disposes of this PluginManager.
        /// </summary>
        /// <param name="disposing">Are we disposing?</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (T plugin in plugins.Values)
                    plugin.Dispose();
            }
        }
    }
}
