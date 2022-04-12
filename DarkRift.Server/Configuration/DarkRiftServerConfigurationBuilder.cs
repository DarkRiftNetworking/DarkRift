/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;

namespace DarkRift.Server.Configuration
{
    /// <summary>
    /// Builder for DarkRift server configurations. Helps build a <see cref="ServerSpawnData"/> object.
    /// </summary>
    public class DarkRiftServerConfigurationBuilder
    {
        /// <summary>
        /// The <see cref="ServerSpawnData"/> being constructed.
        /// </summary>
        public ServerSpawnData ServerSpawnData { get; }

        // TODO Cache (not added yet as likely to change in an upcoming version)

        // TODO add to docs

        // TODO test

        private DarkRiftServerConfigurationBuilder(ServerSpawnData serverSpawnData)
        {
            ServerSpawnData = serverSpawnData;
        }

        /// <summary>
        /// Creates a blank builder to begin configuration.
        /// </summary>
        /// <returns>The created builder.</returns>
        public static DarkRiftServerConfigurationBuilder Create()
        {
            return new DarkRiftServerConfigurationBuilder(new ServerSpawnData());
        }

        /// <summary>
        /// Creates a builder from the given XML server configuration to begin configuration.
        /// </summary>
        /// <param name="path">The path to the XML config.</param>
        /// <returns>The created builder.</returns>
        public static DarkRiftServerConfigurationBuilder CreateFromXml(string path)
        {
            return CreateFromXml(path, new NameValueCollection());
        }

        /// <summary>
        /// Creates a builder from the given XML server configuration to begin configuration.
        /// </summary>
        /// <param name="path">The path to the XML config.</param>
        /// <param name="variables">The variable to substitute into the configuration.</param>
        /// <returns>The created builder.</returns>
        public static DarkRiftServerConfigurationBuilder CreateFromXml(string path, NameValueCollection variables)
        {
            return new DarkRiftServerConfigurationBuilder(ServerSpawnData.CreateFromXml(path, variables));
        }

        /// <summary>
        /// Sets the data directory DarkRift will use to store persistent data.
        /// </summary>
        /// <param name="directory">The directory to use.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder WithDataDirectory(string directory)
        {
            ServerSpawnData.Data.Directory = directory;

            return this;
        }

        /// <summary>
        /// Sets the ID of the thread the dispatcher is expecting tasks to be executed from.
        /// </summary>
        /// <param name="threadID">The thread ID.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder WithDispatcherExecutorThreadID(int threadID)
        {
            ServerSpawnData.DispatcherExecutorThreadID = threadID;

            return this;
        }

        /// <summary>
        /// Sets whether events should be invoked from the dispatcher or not.
        /// </summary>
        /// <param name="enable">If event should be invoked from the dispatcher.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder WithEventsFromDispatcher(bool enable)
        {
            ServerSpawnData.EventsFromDispatcher = enable;

            return this;
        }

        /// <summary>
        /// Configures a listener on the DarkRift server.
        /// </summary>
        /// <param name="name">The name of the listener.</param>
        /// <param name="type">The type of listener.</param>
        /// <param name="address">The address the listener will listen on.</param>
        /// <param name="port">The port the listener will listen on.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder AddListener(string name, string type, IPAddress address, ushort port)
        {
            return AddListener(name, type, address, port, new NameValueCollection());
        }

        /// <summary>
        /// Configures a listener on the DarkRift server.
        /// </summary>
        /// <param name="name">The name of the listener.</param>
        /// <param name="type">The type of listener.</param>
        /// <param name="address">The address the listener will listen on.</param>
        /// <param name="port">The port the listener will listen on.</param>
        /// <param name="settings">The settings to pass to the listener.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder AddListener(string name, string type, IPAddress address, ushort port, NameValueCollection settings)
        {
            ServerSpawnData.ListenersSettings.NetworkListenerSettings networkListenerSettings = new ServerSpawnData.ListenersSettings.NetworkListenerSettings
            {
                Name = name,
                Type = type,
                Address = address,
                Port = port,
            };

            foreach (string key in settings.AllKeys)
                networkListenerSettings.Settings[key] = settings[key];

            ServerSpawnData.Listeners.NetworkListeners.Add(networkListenerSettings);

            return this;
        }

        /// <summary>
        /// Sets the initial log levels DarkRift will use before loading listeners.
        /// </summary>
        /// <param name="logLevels">The initial log levels to use.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder WithStartupLogLevels(params LogType[] logLevels)
        {
            ServerSpawnData.Logging.StartupLogLevels = logLevels;

            return this;
        }

        /// <summary>
        /// Configures a log writer on the DarkRift server.
        /// </summary>
        /// <param name="name">The name of the log writer.</param>
        /// <param name="type">The type of log writer.</param>
        /// <param name="logLevels">The log levels to use.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder AddLogWriter(string name, string type, params LogType[] logLevels)
        {
            return AddLogWriter(name, type, new NameValueCollection(), logLevels);
        }

        /// <summary>
        /// Configures a log writer on the DarkRift server.
        /// </summary>
        /// <param name="name">The name of the log writer.</param>
        /// <param name="type">The type of log writer.</param>
        /// <param name="logLevels">The log levels to use.</param>
        /// <param name="settings">The settings to pass to the log writer.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder AddLogWriter(string name, string type, NameValueCollection settings, params LogType[] logLevels)
        {
            ServerSpawnData.LoggingSettings.LogWriterSettings logWriterSettings = new ServerSpawnData.LoggingSettings.LogWriterSettings
            {
                Name = name,
                Type = type,
                LogLevels = logLevels
            };

            foreach (string key in settings.AllKeys)
                logWriterSettings.Settings[key] = settings[key];

            ServerSpawnData.Logging.LogWriters.Add(logWriterSettings);

            return this;
        }

        /// <summary>
        /// Sets whether metrics should be emitted per message or not.
        /// </summary>
        /// <param name="enable">Whether metrics should be emitted per message.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder WithPerMessageMetrics(bool enable)
        {
            ServerSpawnData.Metrics.EnablePerMessageMetrics = enable;

            return this;
        }

        /// <summary>
        /// Configures the metrics writer on the DarkRift server.
        /// </summary>
        /// <param name="type">The type of metrics writer.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder WithMetricsWriter(string type)
        {
            return WithMetricsWriter(type, new NameValueCollection());
        }

        /// <summary>
        /// Configures the metrics writer on the DarkRift server.
        /// </summary>
        /// <param name="type">The type of metrics writer.</param>
        /// <param name="settings">The settings to pass to the metrics writer.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder WithMetricsWriter(string type, NameValueCollection settings)
        {
            ServerSpawnData.Metrics.MetricsWriter.Type = type;

            foreach (string key in settings.AllKeys)
                ServerSpawnData.Metrics.MetricsWriter.Settings[key] = settings[key];

            return this;
        }

        /// <summary>
        /// Sets whether plugins should be loaded by default or if they must be explicitly specified.
        /// </summary>
        /// <param name="enable">Whether plugins should be loaded by default.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder WithLoadPluginsByDefault(bool enable)
        {
            ServerSpawnData.Plugins.LoadByDefault = enable;

            return this;
        }

        /// <summary>
        /// Configures a plugin on the DarkRift server.
        /// </summary>
        /// <param name="type">The type of plugin.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder AddPlugin(string type)
        {
            return AddPlugin(type, new NameValueCollection());
        }

        /// <summary>
        /// Configures a plugin on the DarkRift server.
        /// </summary>
        /// <param name="type">The type of plugin.</param>
        /// <param name="settings">The settings to pass to the plugin.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder AddPlugin(string type, NameValueCollection settings)
        {
            ServerSpawnData.PluginsSettings.PluginSettings pluginSettings = new ServerSpawnData.PluginsSettings.PluginSettings
            {
                Type = type,
                Load = true
            };

            foreach (string key in settings.AllKeys)
                pluginSettings.Settings[key] = settings[key];

            ServerSpawnData.Plugins.Plugins.Add(pluginSettings);

            return this;
        }

        /// <summary>
        /// Indicates that a plugin should not be loaded by the DarkRift server.
        /// </summary>
        /// <param name="type">The type of plugin to exclude.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder ExceptPlugin(string type)
        {
            ServerSpawnData.PluginsSettings.PluginSettings pluginSettings = new ServerSpawnData.PluginsSettings.PluginSettings
            {
                Type = type,
                Load = false
            };

            ServerSpawnData.Plugins.Plugins.Add(pluginSettings);

            return this;
        }

        /// <summary>
        /// Adds a path to the DarkRift server that will be searched for plugins.
        /// </summary>
        /// <param name="source">The path to search.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder AddPluginSearchPath(string source)
        {
            ServerSpawnData.PluginSearchSettings.PluginSearchPath pluginSearchPath = new ServerSpawnData.PluginSearchSettings.PluginSearchPath
            {
                Source = source
            };

            ServerSpawnData.PluginSearch.PluginSearchPaths.Add(pluginSearchPath);

            return this;
        }

        /// <summary>
        /// Adds a path to the DarkRift server that will be searched for plugins.
        /// </summary>
        /// <param name="source">The path to search.</param>
        /// <param name="createDirectory">Whether a directory should be created if it does not exist.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder AddPluginSearchPath(string source, bool createDirectory)
        {
            ServerSpawnData.PluginSearchSettings.PluginSearchPath pluginSearchPath = new ServerSpawnData.PluginSearchSettings.PluginSearchPath
            {
                Source = source,
                CreateDirectory = createDirectory
            };

            ServerSpawnData.PluginSearch.PluginSearchPaths.Add(pluginSearchPath);

            return this;
        }

        /// <summary>
        /// Adds a path to the DarkRift server that will be searched for plugins.
        /// </summary>
        /// <param name="source">The path to search.</param>
        /// <param name="dependencyResolutionStrategy">How dependencies for plugins loaded from this path should be resolved.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder AddPluginSearchPath(string source, DependencyResolutionStrategy dependencyResolutionStrategy)
        {
            ServerSpawnData.PluginSearchSettings.PluginSearchPath pluginSearchPath = new ServerSpawnData.PluginSearchSettings.PluginSearchPath
            {
                Source = source,
                DependencyResolutionStrategy = dependencyResolutionStrategy
            };

            ServerSpawnData.PluginSearch.PluginSearchPaths.Add(pluginSearchPath);

            return this;
        }

        /// <summary>
        /// Adds a path to the DarkRift server that will be searched for plugins.
        /// </summary>
        /// <param name="source">The path to search.</param>
        /// <param name="dependencyResolutionStrategy">How dependencies for plugins loaded from this path should be resolved.</param>
        /// <param name="createDirectory">Whether a directory should be created if it does not exist.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder AddPluginSearchPath(string source, DependencyResolutionStrategy dependencyResolutionStrategy, bool createDirectory)
        {
            ServerSpawnData.PluginSearchSettings.PluginSearchPath pluginSearchPath = new ServerSpawnData.PluginSearchSettings.PluginSearchPath
            {
                Source = source,
                DependencyResolutionStrategy = dependencyResolutionStrategy,
                CreateDirectory = createDirectory
            };

            ServerSpawnData.PluginSearch.PluginSearchPaths.Add(pluginSearchPath);

            return this;
        }

        /// <summary>
        /// Add a specific type as a plugin to the DarkRift server.
        /// </summary>
        /// <param name="type">The type to load.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder AddPluginType(Type type)
        {
            ServerSpawnData.PluginSearch.PluginTypes.Add(type);

            return this;
        }

        /// <summary>
        /// Sets the maximum number of strikes a client can get before being kicked from the server.
        /// </summary>
        /// <param name="maxStrikes">The maximum number of strikes.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder WithMaxStrikes(byte maxStrikes)
        {
            ServerSpawnData.Server.MaxStrikes = maxStrikes;

            return this;
        }

        /// <summary>
        /// Sets the maximum number of times a server will attempted to be reconnected to before being considered lsot form the cluster.
        /// </summary>
        /// <param name="reconnectAttempts">The maximum number of reconnections.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder WithMaxReconnectAttempts(ushort reconnectAttempts)
        {
            ServerSpawnData.Server.ReconnectAttempts = reconnectAttempts;

            return this;
        }

#if PRO
        /// <summary>
        /// Sets the group that this server belongs to.
        /// </summary>
        /// <param name="serverGroup">The group the server belong to.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder WithServerGroup(string serverGroup)
        {
            ServerSpawnData.Server.ServerGroup = serverGroup;

            return this;
        }
#endif

        /// <summary>
        /// Sets the host the server advertises itself on to other servers.
        /// </summary>
        /// <param name="host">The host the server advertises itself on.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder WithAdvertisedHost(string host)
        {
            ServerSpawnData.ServerRegistry.AdvertisedHost = host;

            return this;
        }

        /// <summary>
        /// Sets the port the server advertises itself on to other servers.
        /// </summary>
        /// <param name="port">The port the server advertises itself on.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder WithAdvertisedPort(ushort port)
        {
            ServerSpawnData.ServerRegistry.AdvertisedPort = port;

            return this;
        }

        /// <summary>
        /// Configures the server registry connector on the DarkRift server.
        /// </summary>
        /// <param name="type">The type of server registry connector .</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder WithServerRegistryConnector(string type)
        {
            return WithServerRegistryConnector(type, new NameValueCollection());
        }

        /// <summary>
        /// Configures the server registry connector on the DarkRift server.
        /// </summary>
        /// <param name="type">The type of server registry connector.</param>
        /// <param name="settings">The settings to pass to the server registry connector.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftServerConfigurationBuilder WithServerRegistryConnector(string type, NameValueCollection settings)
        {
            ServerSpawnData.ServerRegistry.ServerRegistryConnector.Type = type;

            foreach (string key in settings.AllKeys)
                ServerSpawnData.ServerRegistry.ServerRegistryConnector.Settings[key] = settings[key];

            return this;
        }
    }
}
