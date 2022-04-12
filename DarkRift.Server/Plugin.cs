/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Dispatching;
using DarkRift.Server.Plugins.Chat;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;

namespace DarkRift.Server
{
    /// <summary>
    ///     Base class for DarkRift control plugins.
    /// </summary>
    public abstract class Plugin : ExtendedPluginBase
    {
        /// <summary>
        ///     The manager for all clients on this server.
        /// </summary>
        public IClientManager ClientManager { get; }

        /// <summary>
        ///     The manager for all plugins on this server.
        /// </summary>
        public IPluginManager PluginManager { get; }

        /// <summary>
        ///     The manager for all network listeners on this server.
        /// </summary>
        public INetworkListenerManager NetworkListenerManager { get; }

#if PRO
        /// <summary>
        ///     The manager for all cluster connectors on this server.
        /// </summary>
        public IServerRegistryConnectorManager ServerRegistryConnectorManager { get; }

        /// <summary>
        ///     The server manager for remote servers.
        /// </summary>
        public IRemoteServerManager RemoteServerManager { get; }
#endif

        /// <summary>
        ///     The location of this plugins resource store.
        /// </summary>
        /// <remarks>
        ///     The resource directory can be used to store any external resources your plugin requires such as web 
        ///     files etc. It will be removed when uninstalling your plugin so you should not store any files elsewhere.
        ///     
        ///     This location may not exist if called from the constructor, use the <see cref="ExtendedPluginBase.Loaded(LoadedEventArgs)"/> event instead.
        /// </remarks>
        protected string ResourceDirectory { get; }

#if PRO
        /// <summary>
        ///     Helper plugin for filtering bad words out of text.
        /// </summary>
        /// <remarks>
        ///     <c>Pro only.</c> 
        /// </remarks>
        public IBadWordFilter BadWordFilter => PluginManager.GetPluginByType<BadWordFilter>();
#endif

        /// <summary>
        ///     Creates a new plugin using the given plugin load data.
        /// </summary>
        /// <param name="pluginLoadData">The plugin load data for this plugin.</param>
        public Plugin(PluginLoadData pluginLoadData)
            : base (pluginLoadData)
        {
            ClientManager = pluginLoadData.ClientManager;
            PluginManager = pluginLoadData.PluginManager;
            NetworkListenerManager = pluginLoadData.NetworkListenerManager;
#if PRO
            ServerRegistryConnectorManager = pluginLoadData.ServerRegistryConnectorManager;
            RemoteServerManager = pluginLoadData.RemoteServerManager;
#endif
            ResourceDirectory = pluginLoadData.ResourceDirectory;
        }
    }
}
