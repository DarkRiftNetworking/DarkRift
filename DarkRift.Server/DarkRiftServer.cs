/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using DarkRift.Dispatching;
using System.Threading;
using DarkRift.Server.Plugins.Chat;
using System.Collections.Specialized;
using DarkRift.Server.Metrics;

namespace DarkRift.Server
{
    /// <summary>
    ///     The main server class.
    /// </summary>
    public sealed class DarkRiftServer : IDisposable
    {
        /// <summary>
        ///     The manager for logs.
        /// </summary>
        /// <remarks>
        ///     Pro only.
        /// </remarks>
#if PRO
        public
#else
        internal
#endif
            ILogManager LogManager => logManager;

#if PRO
        /// <summary>
        ///     The manager for metrics.
        /// </summary>
        /// <remarks>
        ///     Pro only.
        /// </remarks>
        public IMetricsManager MetricsManager => metricsManager;

#endif
        /// <summary>
        ///     The client manager handling all clients on this server.
        /// </summary>
        public IClientManager ClientManager => InternalClientManager;

        /// <summary>
        ///     The manager for all plugins.
        /// </summary>
        public IPluginManager PluginManager => InternalPluginManager;

        /// <summary>
        ///     The manager for all listeners.
        /// </summary>
        public INetworkListenerManager NetworkListenerManager => networkListenerManager;

        /// <summary>
        ///     The manager for databases.
        /// </summary>
        [Obsolete("No direct replacement.")]
        public IDatabaseManager DatabaseManager { get; }

        /// <summary>
        ///     The server's dispatcher.
        /// </summary>
        public IDispatcher Dispatcher => dispatcher;

        /// <summary>
        ///     The dispatcher's wait handle.
        /// </summary>
        public WaitHandle DispatcherWaitHandle => dispatcher.WaitHandle;

        /// <summary>
        ///     Whether events are executed through the dispatcher or not.
        /// </summary>
        public bool EventsFromDispatcher => ThreadHelper.EventsFromDispatcher;

        /// <summary>
        ///     The thread helper for use with events.
        /// </summary>
        public DarkRiftThreadHelper ThreadHelper { get; }

        /// <summary>
        ///     Information about this server.
        /// </summary>
        public DarkRiftInfo ServerInfo { get; } = new DarkRiftInfo(DateTime.Now);

#if PRO
        /// <summary>
        ///     Helper plugin for filtering bad words out of text.
        /// </summary>
        /// <remarks>
        ///     Pro only.
        /// </remarks>
        public IBadWordFilter BadWordFilter => PluginManager.GetPluginByType<BadWordFilter>();

        /// <summary>
        ///     The server registry connector manager.
        /// </summary>
        public IServerRegistryConnectorManager ServerRegistryConnectorManager => internalServerRegistryConnectorManager;

        /// <summary>
        ///     The server manager for remote servers.
        /// </summary>
        public IRemoteServerManager RemoteServerManager => InternalRemoteServerManager;
#endif

        /// <summary>
        ///     Whether this server has been loaded yet.
        /// </summary>
        public bool Loaded { get; set; }

        /// <summary>
        ///     The server plugin manager.
        /// </summary>
        internal PluginManager InternalPluginManager { get; }

        /// <summary>
        ///     The client manager handling all clients on this server.
        /// </summary>
        internal ClientManager InternalClientManager { get; }

        /// <summary>
        ///     The handler for all commands issued from the user.
        /// </summary>
        internal CommandEngine CommandEngine { get; }

        /// <summary>
        ///     The manager for server data.
        /// </summary>
        internal DataManager DataManager { get; }

#if PRO
        /// <summary>
        ///     The server manager for remote servers.
        /// </summary>
        internal RemoteServerManager InternalRemoteServerManager { get; }

        /// <summary>
        ///     The server registry connector manager.
        /// </summary>
        private readonly ServerRegistryConnectorManager internalServerRegistryConnectorManager;
#endif

        /// <summary>
        ///     The server listener manager.
        /// </summary>
        private readonly NetworkListenerManager networkListenerManager;

        /// <summary>
        ///     The manager for logs.
        /// </summary>
        private readonly LogManager logManager;

#if PRO
        /// <summary>
        ///     The manager for metrics.
        /// </summary>
        private readonly MetricsManager metricsManager;
#endif

        /// <summary>
        ///     The factory for plugins.
        /// </summary>
        private readonly PluginFactory pluginFactory;

        /// <summary>
        ///     The dispatcher for the server.
        /// </summary>
        private readonly Dispatcher dispatcher;

        /// <summary>
        ///     Whether this server has been disposed yet.
        /// </summary>
        public bool Disposed { get => disposed; private set => disposed = value; }

        /// <summary>
        ///     Whether this server has been disposed yet.
        /// </summary>
        private volatile bool disposed;

        /// <summary>
        ///     The server's main logger.
        /// </summary>
        private readonly Logger logger;

#if PRO
        /// <summary>
        ///     Creates a new server given spawn details and a default cluster.
        /// </summary>
        /// <param name="spawnData">The details of how to start the server.</param>
        public DarkRiftServer(ServerSpawnData spawnData)
            : this (spawnData, ClusterSpawnData.CreateDefault())
        {

        }
#endif

#if PRO
        /// <summary>
        ///     Creates a new server given spawn details.
        /// </summary>
        /// <param name="spawnData">The details of how to start the server.</param>
        /// <param name="clusterSpawnData">The details of the cluster this server is part of.</param>
        public DarkRiftServer(ServerSpawnData spawnData, ClusterSpawnData clusterSpawnData)
#else
        /// <summary>
        ///     Creates a new server given spawn details.
        /// </summary>
        /// <param name="spawnData">The details of how to start the server.</param>
        public DarkRiftServer(ServerSpawnData spawnData)
#endif
        {
            //Initialize log manager and set initial writer
            logManager = new LogManager(this, spawnData.Logging);

            //Initialize data manager
            DataManager = new DataManager(spawnData.Data, logManager.GetLoggerFor(nameof(Server.DataManager)));

            //Initialize object caches before we shoot ourselves in the foot
            bool initializedCache = ObjectCache.Initialize(spawnData.Cache.ServerObjectCacheSettings)
                & ServerObjectCache.Initialize(spawnData.Cache.ServerObjectCacheSettings);

            //Set before loading plugins so plugins override this setting
            dispatcher = new Dispatcher(false, spawnData.DispatcherExecutorThreadID);
            ThreadHelper = new DarkRiftThreadHelper(spawnData.EventsFromDispatcher, dispatcher);

            //Load plugin factory so we can load log writers
            pluginFactory = new PluginFactory(logManager.GetLoggerFor(nameof(PluginFactory)));
            pluginFactory.AddFromSettings(spawnData.PluginSearch);
            pluginFactory.AddTypes(
                new Type[]
                {
                    typeof(Plugins.LogWriters.ConsoleWriter),
                    typeof(Plugins.LogWriters.DebugWriter),
                    typeof(Plugins.LogWriters.FileWriter)
                }
            );
            pluginFactory.AddTypes(
                new Type[]
                {
                    typeof(Plugins.Commands.HelpCommand),
                    typeof(Plugins.Commands.PluginController),
                    typeof(Plugins.Commands.Sniffer),
                    typeof(Plugins.Commands.DemoCommand),
                    typeof(Plugins.Commands.MessageCommand),
                    typeof(Plugins.Commands.MockCommand),
                    typeof(Plugins.Commands.DebugCommand),
                    typeof(Plugins.Commands.ClientCommand),
                    typeof(Plugins.Commands.ClearCommand)
                }
            );
            pluginFactory.AddTypes(
                new Type[]
                {
                    typeof(Plugins.Performance.ObjectCacheMonitor)
                }
            );
            pluginFactory.AddTypes(
                new Type[]
                {
                    typeof(Plugins.HealthCheck.HttpHealthCheck)
                }
            );
#if PRO
            pluginFactory.AddTypes(
                new Type[]
                {
                    typeof(Plugins.Chat.BadWordFilter),
                    typeof(Plugins.Metrics.Prometheus.PrometheusEndpoint)
                }
            );
#endif

            //Fix network listeners in
            pluginFactory.AddTypes(
                new Type[]
                {
                    typeof(Plugins.Listeners.Bichannel.BichannelListener),
#pragma warning disable CS0618 // Type or member is obsolete
                    typeof(Plugins.Listeners.Bichannel.CompatibilityBichannelListener)
#pragma warning restore CS0618 // Type or member is obsolete
                }
            );


            //Load log writers from plugin factory
            logManager.Clear();
            logManager.LoadWriters(spawnData.Logging, pluginFactory);

            logger = logManager.GetLoggerFor(nameof(DarkRiftServer));

            //Write system details to logs
            logger.Trace($"System Details:\n\tOS: {Environment.OSVersion}\n\tCLS Version: {Environment.Version}\n\tDarkRift: {ServerInfo.Version} - {ServerInfo.Type}");

            //Write whether the cache was initialized
            if (!initializedCache)
                logger.Trace("Cache already initialized, cannot update settings. The server will continue using the pre-existing cache.");

            //Load later stage things

#if PRO
            metricsManager = new MetricsManager(this, spawnData.Metrics);
            metricsManager.LoadWriters(spawnData.Metrics, pluginFactory, logManager);
            internalServerRegistryConnectorManager = new ServerRegistryConnectorManager(this, pluginFactory, logManager, metricsManager);
#endif

            networkListenerManager = new NetworkListenerManager(
                this,
                logManager,
#if PRO
                metricsManager,
#endif
                DataManager,
                pluginFactory
            );
#if PRO
            InternalRemoteServerManager = new RemoteServerManager(
                spawnData.Server,
                spawnData.ServerRegistry,
                clusterSpawnData,
                networkListenerManager,
                ThreadHelper,
                internalServerRegistryConnectorManager,
                logManager,
                logManager.GetLoggerFor(nameof(RemoteServerManager)),
                metricsManager
            );
#endif
            InternalClientManager = new ClientManager(
                spawnData.Server,
                networkListenerManager,
                ThreadHelper,
                logManager.GetLoggerFor(nameof(ClientManager)),
                logManager.GetLoggerFor(nameof(Client))
#if PRO
                , metricsManager.GetMetricsCollectorFor(nameof(ClientManager)),
                metricsManager.GetPerMessageMetricsCollectorFor(nameof(Client))
#endif
            );

#pragma warning disable CS0618 // Type or member is obsolete
            DatabaseManager = new DatabaseManager(spawnData.Databases);
#pragma warning restore CS0618 // Type or member is obsolete

            // Now we have the prerequisites loaded we can start loading plugins
            InternalPluginManager = new PluginManager(
                this,
                DataManager,
                logManager,
#if PRO
                metricsManager,
#endif
                pluginFactory,
                logManager.GetLoggerFor(nameof(PluginManager))
            );

#if PRO
            internalServerRegistryConnectorManager.LoadPlugins(spawnData.ServerRegistry);
#endif
            InternalPluginManager.LoadPlugins(spawnData.Plugins);
            networkListenerManager.LoadNetworkListeners(spawnData.Listeners);

            //Load default if no other listeners are present
            if (spawnData.Listeners.NetworkListeners.Count == 0)
            {
                NameValueCollection listenerSettings = new NameValueCollection();

                // Warnings disabled as we're implementing obsolete functionality
#pragma warning disable
                if (spawnData.Server.UseFallbackNetworking)
                {
                    networkListenerManager.LoadNetworkListener(
                        typeof(Plugins.Listeners.Bichannel.CompatibilityBichannelListener),
                        "Default",
                        spawnData.Server.Address,
                        spawnData.Server.Port,
                        listenerSettings
                    );
                }
                else
                {
                    networkListenerManager.LoadNetworkListener(
                        typeof(Plugins.Listeners.Bichannel.BichannelListener),
                        "Default",
                        spawnData.Server.Address,
                        spawnData.Server.Port,
                        listenerSettings
                    );
                }
#pragma warning restore
            }

            CommandEngine = new CommandEngine(ThreadHelper, InternalPluginManager, logManager.GetLoggerFor(nameof(CommandEngine)));

            //Inform plugins we have loaded
            Loaded = true;
            InternalPluginManager.Loaded();

            // Now we've loaded, wire up the listeners
#if PRO
            if (string.IsNullOrEmpty(RemoteServerManager.Group) || RemoteServerManager.Visibility == ServerVisibility.External)
            {
#endif
                logger.Trace("Binding listeners to ClientManager as server is externally visible.");
                InternalClientManager.SubscribeToListeners();
            #if PRO
            }
            else
            {
                logger.Trace("Binding listeners to RemoteServerManager as server is internally visible.");
                InternalRemoteServerManager.SubscribeToListeners();

                logger.Warning("Server clustering is in beta and is not currently considered suitable for production use.");
            }
#endif
        }

        /// <summary>
        ///     Starts the server.
        /// </summary>
        [Obsolete("User StartServer instead for better error propagation.")]
        public void Start()
        {
            try
            {
                StartServer();
            }
            catch (Exception)
            {
                return;
            }
        }

        /// <summary>
        ///     Starts the server propagating any exceptions raised during startup.
        /// </summary>
        public void StartServer()
        {
#if PRO
            try
            {
                InternalRemoteServerManager.RegisterServer();
            }
            catch (Exception e)
            {
                logger.Fatal("An exception was thrown whilst registering the server with the registry, the server cannot be started.", e);
                throw;
            }
#endif

            try
            {
                networkListenerManager.StartListening();
            }
            catch (Exception e)
            {
                logger.Fatal("A listener threw an exception while starting, the server cannot be started.", e);
                throw;
            }
        }

        /// <summary>
        ///     Executes all tasks waiting in the dispatcher.
        /// </summary>
        /// <remarks>
        ///     This must be invoked from the same thread that constructs the server since this is deemed the 'main' thread.
        /// </remarks>
        public void ExecuteDispatcherTasks()
        {
            dispatcher.ExecuteDispatcherTasks();
        }

        /// <summary>
        ///     Executes a given command on the server.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        public void ExecuteCommand(string command)
        {
            CommandEngine.HandleCommand(command);
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

        /// <summary>
        ///     Forces the server to invoke events through the dispatcher.
        /// </summary>
        internal void MakeThreadSafe()
        {
            ThreadHelper.EventsFromDispatcher = true;

            logger.Trace($"Switched into thread safe mode. All events will be invoked from the main thread. This may affect server performance.");
        }

        /// <summary>
        ///     Disposes of the server.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
#if PRO
                InternalRemoteServerManager.DeregisterServer();
#endif
                InternalClientManager.Dispose();

                InternalPluginManager.Dispose();

                networkListenerManager.Dispose();

#if PRO
                internalServerRegistryConnectorManager.Dispose();

                InternalRemoteServerManager.Dispose();

                metricsManager.Dispose();
#endif

                DataManager.Dispose();

                logManager.Dispose();
                dispatcher.Dispose();

                disposed = true;
            }
        }
    }
}
