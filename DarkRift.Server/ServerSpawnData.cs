/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Xml;
using System.Configuration;
using System.IO;

using System.Net;
using System.Collections.Specialized;

namespace DarkRift.Server
{
    /// <summary>
    ///     Details of how to start a new server.
    /// </summary>
    [Serializable]
    // TODO DR3 rename to ServerConfiguration
    public class ServerSpawnData
    {
        /// <summary>
        ///     The general settings for the server.
        /// </summary>
        public ServerSettings Server { get; set; } = new ServerSettings();

        /// <summary>
        ///     The locations to search for plugins in.
        /// </summary>
        public PluginSearchSettings PluginSearch { get; set; } = new PluginSearchSettings();

        /// <summary>
        ///     The settings for the data handler plugins and general persistent storage.
        /// </summary>
        public DataSettings Data { get; set; } = new DataSettings();

        /// <summary>
        ///     The settings for the log writer plugins and general logging.
        /// </summary>
        public LoggingSettings Logging { get; set; } = new LoggingSettings();
        
        /// <summary>
        ///     The settings for resolving and loading plugins.
        /// </summary>
        public PluginsSettings Plugins { get; set; } = new PluginsSettings();

        /// <summary>
        ///     The settings for database connections.
        /// </summary>
        [Obsolete("Use configuration settings under the plugin that requires the database connection string.")]
        public DatabaseSettings Databases { get; set; } = new DatabaseSettings();

        /// <summary>
        ///     The settings for the object cache.
        /// </summary>
        public CacheSettings Cache { get; set; } = new CacheSettings();

        /// <summary>
        ///     The settings for the server's listeners.
        /// </summary>
        public ListenersSettings Listeners { get; set; } = new ListenersSettings();

        /// <summary>
        ///     The settings for the server regirsty.
        /// </summary>
        public ServerRegistrySettings ServerRegistry { get; set; } = new ServerRegistrySettings();

        /// <summary>
        ///     The settings for the metrics writer plugins and general metrics.
        /// </summary>
        public MetricsSettings Metrics { get; set; } = new MetricsSettings();

        /// <summary>
        ///     Whether events are executed through the dispatcher or not.
        /// </summary>
        public bool EventsFromDispatcher { get; set; }

        /// <summary>
        ///     The ID of the thread that will be executing dispatcher tasks for deadlock protection. Setting this to -1 will disable this.
        /// </summary>
        public int DispatcherExecutorThreadID { get; set; } = -1;

        /// <summary>
        ///     Holds settings related to the overall server.
        /// </summary>
        [Serializable]
        public class ServerSettings
        {
            /// <summary>
            ///     The address the server will listen on.
            /// </summary>
            [Obsolete("Address is obsolete, use listeners system instead. These properties will only have effect if no listeners are defined.")]
            public IPAddress Address { get; set; }

            /// <summary>
            ///     The port number that the server should listen on.
            /// </summary>
            [Obsolete("Port is obsolete, use listeners system instead. These properties will only have effect if no listeners are defined.")]
            public ushort Port { get; set; }

            /// <summary>
            ///     The IP version to host the server on.
            /// </summary>
            [Obsolete("IPVersion is obsolete, use listeners system instead. These properties will only have effect if no listeners are defined.")]
            public IPVersion IPVersion { get; set; }

            /// <summary>
            ///     Whether to disable Nagle's algorithm.
            /// </summary>
            [Obsolete("NoDelay is obsolete, use listeners system instead. These properties will only have effect if no listeners are defined.")]
            public bool NoDelay { get; set; }

            /// <summary>
            ///     The number of strikes that can be received before the client is automatically kicked.
            /// </summary>
            public byte MaxStrikes { get; set; }

            /// <summary>
            ///     Whether the fallback networking system should be used for compatability with Unity.
            /// </summary>
            [Obsolete("UseFallbackNetworking is obsolete, use CombatabilityBichannelListener instead. These properties will only have effect if no listeners are defined.")]
            public bool UseFallbackNetworking { get; set; }

#if PRO
            /// <summary>
            ///     The server group this server belongs to.
            /// </summary>
            public string ServerGroup { get; set; }
#endif

            /// <summary>
            ///     The number of times to try to reconnect to a server before considering it unconnectable.
            /// </summary>
            public ushort ReconnectAttempts { get; set; } = 5;

            /// <summary>
            ///     Loads the server settings from the specified XML element.
            /// </summary>
            /// <param name="element">The XML element to load from.</param>
            /// <param name="helper">The XML configuration helper being used.</param>
            internal void LoadFromXmlElement(XElement element, ConfigurationFileHelper helper)
            {
                if (element == null)
                    return;

                // Warnings disabled as we're implementing obsolete functionality
#pragma warning disable
                //Read address
                Address = helper.ReadIPAttributeOrDefault(
                    element, 
                    "address",
                    IPAddress.Any
                );

                //Read port
                Port = helper.ReadUInt16AttributeOrDefault(
                    element,
                    "port",
                    4296
                );

                //Read the ip type
                IPVersion = helper.ReadIPVersionAttributeOrDefault(
                    element,
                    "ipVersion",
                    IPVersion.IPv4
                );

                //Read no delay
                NoDelay = helper.ReadBooleanAttribute(
                    element,
                    "noDelay",
                    false
                );

                //Read use fallback networking
                UseFallbackNetworking = helper.ReadBooleanAttribute(
                    element,
                    "useFallback",
                    false
                );
#pragma warning restore

                //Read max strikes
                MaxStrikes = helper.ReadByteAttribute(
                    element,
                    "maxStrikes"
                );

#if PRO
                //Read server group
                ServerGroup = helper.ReadStringAttributeOrDefault(
                    element,
                    "serverGroup",
                    null
                );
#endif

                //Read reconnect attempts
                ReconnectAttempts = helper.ReadUInt16AttributeOrDefault(
                    element,
                    "reconnectAttempts",
                    5
                );
            }
        }

        /// <summary>
        ///     Holds the paths to search for plugins from.
        /// </summary>
        [Serializable]
        public class PluginSearchSettings
        {
            /// <summary>
            ///     The paths to search.
            /// </summary>
            public List<PluginSearchPath> PluginSearchPaths { get; } = new List<PluginSearchPath>();

            /// <summary>
            ///     Individual types of plugins that should be loaded.
            /// </summary>
            public List<Type> PluginTypes { get; } = new List<Type>();

            /// <summary>
            ///     A path to search.
            /// </summary>
            public class PluginSearchPath
            {
                /// <summary>
                ///     The path.
                /// </summary>
                public string Source { get; set; }

                /// <summary>
                ///     Whether the directory should be created if missing.
                /// </summary>
                /// <remarks>This has no effect when the path is a file.</remarks>
                public bool CreateDirectory { get; set; }

                /// <summary>
                /// The way to resolve dependencies for the plugin.
                /// </summary>
                public DependencyResolutionStrategy DependencyResolutionStrategy { get; set; }

                /// <summary>
                ///     Loads the path from the specified XML element.
                /// </summary>
                /// <param name="element">The XML element to load from.</param>
                /// <param name="helper">The XML configuration helper being used.</param>
                internal void LoadFromXmlElement(XElement element, ConfigurationFileHelper helper)
                {
                    if (element == null)
                        return;

                    //Read source
                    Source = helper.ReadStringAttribute(
                        element,
                        "src"
                    );

                    //Read port
                    CreateDirectory = helper.ReadBooleanAttribute(
                        element,
                        "createDir",
                        false
                    );

                    // Read dependency resolution strategy 
                    DependencyResolutionStrategy = helper.ReadDependencyResolutionStrategy(
                        element,
                        "dependencyResolutionStrategy",
                        DependencyResolutionStrategy.RecursiveFromFile
                    );
                }
            }

            /// <summary>
            ///     Loads the server settings from the specified XML element.
            /// </summary>
            /// <param name="element">The XML element to load from.</param>
            /// <param name="helper">The XML configuration helper being used.</param>
            internal void LoadFromXmlElement(XElement element, ConfigurationFileHelper helper)
            {
                if (element == null)
                    return;

                //Read search paths
                helper.ReadElementCollectionTo(
                    element, 
                    "pluginSearchPath", 
                    e =>
                    {
                        PluginSearchPath psp = new PluginSearchPath();
                        psp.LoadFromXmlElement(e, helper);
                        return psp;
                    },
                    PluginSearchPaths
                );
            }
        }

        /// <summary>
        ///     Holds settings for persistent data storage.
        /// </summary>
        [Serializable]
        public class DataSettings
        {
            /// <summary>
            ///     The directory to store data in.
            /// </summary>
            public string Directory { get; set; } = "Data/";
            
            /// <summary>
            ///     Loads the server settings from the specified XML element.
            /// </summary>
            /// <param name="element">The XML element to load from.</param>
            /// <param name="helper">The XML configuration helper being used.</param>
            internal void LoadFromXmlElement(XElement element, ConfigurationFileHelper helper)
            {
                if (element == null)
                    return;

                //Read data directory
                Directory = helper.ReadStringAttribute(
                    element,
                    "directory"
                );
            }
        }

        /// <summary>
        ///     Holds settings related to loading the logging system.
        /// </summary>
        [Serializable]
        public class LoggingSettings
        {
            /// <summary>
            ///     The log writers to use.
            /// </summary>
            public List<LogWriterSettings> LogWriters { get; } = new List<LogWriterSettings>();

            /// <summary>
            ///     Log levels to log out to console before log writers are loaded.
            /// </summary>
            public LogType[] StartupLogLevels { get; set; }

            /// <summary>
            ///     Holds settings about a log writer.
            /// </summary>
            [Serializable]
            public class LogWriterSettings
            {
                /// <summary>
                ///     The name of the log writer.
                /// </summary>
                public string Name { get; set; }

                /// <summary>
                ///     The type of log writer.
                /// </summary>
                public string Type { get; set; }

                /// <summary>
                ///     The types of logs to be directed to this writer.
                /// </summary>
                public LogType[] LogLevels { get; set; }

                /// <summary>
                ///     Settings that should be loaded for this writer.
                /// </summary>
                public NameValueCollection Settings { get; } = new NameValueCollection();

                /// <summary>
                ///     Creates a new LoggingSettings object.
                /// </summary>
                public LogWriterSettings()
                {

                }

                /// <summary>
                ///     Loads the log writer settings from the specified XML element.
                /// </summary>
                /// <param name="element">The XML element to load from.</param>
                /// <param name="helper">The XML configuration helper being used.</param>
                internal void LoadFromXmlElement(XElement element, ConfigurationFileHelper helper)
                {
                    if (element == null)
                        return;

                    Type = helper.ReadStringAttribute(
                        element,
                        "type"
                    );

                    Name = helper.ReadStringAttribute(
                        element,
                        "name"
                    );

                    LogLevels = helper.ReadLogLevelsAttribute(
                        element,
                        "levels"
                    );

                    helper.ReadAttributeCollectionTo(
                        element.Element("settings"),
                        Settings
                    );
                }
            }

            /// <summary>
            ///     Creates a new LoggingSettings object.
            /// </summary>
            public LoggingSettings()
            {

            }

            /// <summary>
            ///     Loads the logging settings from the specified XML element.
            /// </summary>
            /// <param name="element">The XML element to load from.</param>
            /// <param name="helper">The XML configuration helper being used.</param>
            internal void LoadFromXmlElement(XElement element, ConfigurationFileHelper helper)
            {
                if (element == null)
                    return;

                StartupLogLevels = helper.ReadLogLevelsAttributeOrDefault(
                        element,
                        "startupLevels",
                        new LogType[] { LogType.Info, LogType.Warning, LogType.Error, LogType.Fatal }
                );

                //Load writers
                var logWritersElement = helper.GetRequiredElement(element, "logWriters");
                helper.ReadElementCollectionTo(
                    logWritersElement,
                    "logWriter",
                    e => {
                        LogWriterSettings s = new LogWriterSettings();
                        s.LoadFromXmlElement(e, helper);
                        return s;
                    },
                    LogWriters
                );
            }
        }

        

        /// <summary>
        ///     Handles the settings for plugins.
        /// </summary>
        [Serializable]
        public class PluginsSettings
        {
            /// <summary>
            ///     The action to perform on all unlisted plugins.
            /// </summary>
            public bool LoadByDefault { get; set; }

            /// <summary>
            ///     The list of plugins to load.
            /// </summary>
            public List<PluginSettings> Plugins { get; } = new List<PluginSettings>();

            /// <summary>
            ///     Holds settings about a plugin.
            /// </summary>
            [Serializable]
            public class PluginSettings
            {
                /// <summary>
                ///     The type of plugin.
                /// </summary>
                public string Type { get; set; }

                /// <summary>
                ///     Whether to load or ignore this plugin.
                /// </summary>
                public bool Load { get; set; } = true;

                /// <summary>
                ///     Settings that should be loaded for this plugin.
                /// </summary>
                public NameValueCollection Settings { get; } = new NameValueCollection();

                /// <summary>
                ///     Loads the plugin settings from the specified XML element.
                /// </summary>
                /// <param name="element">The XML element to load from.</param>
                /// <param name="helper">The XML configuration helper being used.</param>
                internal void LoadFromXmlElement(XElement element, ConfigurationFileHelper helper)
                {
                    if (element == null)
                        return;

                    //Type attribute.
                    Type = helper.ReadStringAttribute(
                        element,
                        "type"
                    );
                    
                    //Load attribute.
                    Load = helper.ReadBooleanAttribute(
                        element,
                        "load",
                        true
                    );

                    helper.ReadAttributeCollectionTo(
                        element.Element("settings"),
                        Settings
                    );
                }
            }
            
            /// <summary>
            ///     Loads the plugins settings from the specified XML element.
            /// </summary>
            /// <param name="element">The XML element to load from.</param>
            /// <param name="helper">The XML configuration helper being used.</param>
            internal void LoadFromXmlElement(XElement element, ConfigurationFileHelper helper)
            {
                if (element == null)
                    return;

                //LoadByDefault attribute.
                LoadByDefault = helper.ReadBooleanAttribute(
                    element,
                    "loadByDefault",
                    false
                );

                //Load plugins
                helper.ReadElementCollectionTo(
                    element,
                    "plugin",
                    e =>
                    {
                        PluginSettings pluginSettings = new PluginSettings();
                        pluginSettings.LoadFromXmlElement(e, helper);
                        return pluginSettings;
                    },
                    Plugins
                );
            }
        }

        /// <summary>
        ///     Holds settings related to loading databases for plugins.
        /// </summary>
        [Serializable]
        [Obsolete("Use configuration settings under the plugin that requires the database connection string.")]
        public class DatabaseSettings
        {
            /// <summary>
            ///     The databases to connect to.
            /// </summary>
            public List<DatabaseConnectionData> Databases { get; } = new List<DatabaseConnectionData>();

            /// <summary>
            ///     Holds data relating to a specific connection.
            /// </summary>
            [Serializable]
            [Obsolete("Use configuration settings under the plugin that requires the database connection string.")]
            public class DatabaseConnectionData
            {
                /// <summary>
                ///     The name of the connection.
                /// </summary>
                public string Name { get; set; }

                /// <summary>
                ///     The connection string to create the connection with.
                /// </summary>
                public string ConnectionString { get; set; }

                /// <summary>
                ///     Creates a new Database Connection data object.
                /// </summary>
                /// <param name="name">The name of the connection.</param>
                /// <param name="connectionString">The connection string for the connection.</param>
                public DatabaseConnectionData(string name, string connectionString)
                {
                    this.Name = name;
                    this.ConnectionString = connectionString;
                }
            }

            /// <summary>
            ///     Loads the database settings from the specified XML element.
            /// </summary>
            /// <param name="element">The XML element to load from.</param>
            /// <param name="helper">The XML configuration helper being used.</param>
            internal void LoadFromXmlElement(XElement element, ConfigurationFileHelper helper)
            {
                if (element == null)
                    return;

                //Load databases
                helper.ReadElementCollectionTo(
                    element,
                    "database",
                    e =>
                    {
                        string name = helper.ReadStringAttribute(
                            e,
                            "name"
                        );

                        string connectionString = helper.ReadStringAttribute(
                            e,
                            "connectionString"
                        );
                        
                        return new DatabaseConnectionData(
                            name,
                            connectionString
                        );
                    },
                    Databases
                );
            }
        }

        /// <summary>
        ///     Holds settings related to the object cache.
        /// </summary>
        [Serializable]
        public class CacheSettings
        {
            /// <summary>
            ///     The maximum number of <see cref="DarkRiftWriter"/> instances stored per thread.
            /// </summary>
            public int MaxCachedWriters
            {
                get => ServerObjectCacheSettings.MaxWriters;
                set => ServerObjectCacheSettings.MaxWriters = value;
            }

            /// <summary>
            ///     The maximum number of <see cref="DarkRiftReader"/> instances stored per thread.
            /// </summary>
            public int MaxCachedReaders
            {
                get => ServerObjectCacheSettings.MaxReaders;
                set => ServerObjectCacheSettings.MaxReaders = value;
            }

            /// <summary>
            ///     The maximum number of <see cref="Message"/> instances stored per thread.
            /// </summary>
            public int MaxCachedMessages
            {
                get => ServerObjectCacheSettings.MaxMessages;
                set => ServerObjectCacheSettings.MaxMessages = value;
            }

            /// <summary>
            ///     The maximum number of <see cref="System.Net.Sockets.SocketAsyncEventArgs"/> instances stored per thread.
            /// </summary>
            public int MaxCachedSocketAsyncEventArgs
            {
                get => ServerObjectCacheSettings.MaxSocketAsyncEventArgs;
                set => ServerObjectCacheSettings.MaxSocketAsyncEventArgs = value;
            }

            /// <summary>
            ///     The maximum number of <see cref="DarkRift.Dispatching.ActionDispatcherTask"/> instances stored per thread.
            /// </summary>
            public int MaxActionDispatcherTasks
            {
                get => ServerObjectCacheSettings.MaxActionDispatcherTasks;
                set => ServerObjectCacheSettings.MaxActionDispatcherTasks = value;
            }

            /// <summary>
            ///     The maximum number of <see cref="AutoRecyclingArray"/> instances stored per thread.
            /// </summary>
            public int MaxAutoRecyclingArrays
            {
                get => ServerObjectCacheSettings.MaxAutoRecyclingArrays;
                set => ServerObjectCacheSettings.MaxAutoRecyclingArrays = value;
            }

            /// <summary>
            ///     The settings for the object cache.
            /// </summary>
            [Obsolete("Use ServerObjectCacheSettings instead.")]
            public ObjectCacheSettings ObjectCacheSettings
            {
                get => ServerObjectCacheSettings;
                set
                {
                    if (value is ServerObjectCacheSettings)
                    {
                        ServerObjectCacheSettings = (ServerObjectCacheSettings)value;
                    }
                    else
                    {
                        ServerObjectCacheSettings.MaxWriters = value.MaxWriters;
                        ServerObjectCacheSettings.MaxReaders = value.MaxReaders;
                        ServerObjectCacheSettings.MaxMessages = value.MaxMessages;
                        ServerObjectCacheSettings.MaxMessageBuffers = value.MaxMessageBuffers;
                        ServerObjectCacheSettings.MaxSocketAsyncEventArgs = value.MaxSocketAsyncEventArgs;
                        ServerObjectCacheSettings.MaxActionDispatcherTasks = value.MaxActionDispatcherTasks;
                        ServerObjectCacheSettings.MaxAutoRecyclingArrays = value.MaxAutoRecyclingArrays;
                        ServerObjectCacheSettings.MaxMessageReceivedEventArgs = 4;

                        ServerObjectCacheSettings.ExtraSmallMemoryBlockSize = value.ExtraSmallMemoryBlockSize;
                        ServerObjectCacheSettings.MaxExtraSmallMemoryBlocks = value.MaxExtraSmallMemoryBlocks;
                        ServerObjectCacheSettings.SmallMemoryBlockSize = value.SmallMemoryBlockSize;
                        ServerObjectCacheSettings.MaxSmallMemoryBlocks = value.MaxSmallMemoryBlocks;
                        ServerObjectCacheSettings.MediumMemoryBlockSize = value.MediumMemoryBlockSize;
                        ServerObjectCacheSettings.MaxMediumMemoryBlocks = value.MaxMediumMemoryBlocks;
                        ServerObjectCacheSettings.LargeMemoryBlockSize = value.LargeMemoryBlockSize;
                        ServerObjectCacheSettings.MaxLargeMemoryBlocks = value.MaxLargeMemoryBlocks;
                        ServerObjectCacheSettings.ExtraLargeMemoryBlockSize = value.ExtraLargeMemoryBlockSize;
                        ServerObjectCacheSettings.MaxExtraLargeMemoryBlocks = value.MaxExtraLargeMemoryBlocks;
                    }
                }
            }

            /// <summary>
            ///     The settings for the object cache.
            /// </summary>
            public ServerObjectCacheSettings ServerObjectCacheSettings { get; set; } = new ServerObjectCacheSettings();

            /// <summary>
            ///     Loads the cache settings from the specified XML element.
            /// </summary>
            /// <param name="element">The XML element to load from.</param>
            /// <param name="helper">The XML configuration helper being used.</param>
            internal void LoadFromXmlElement(XElement element, ConfigurationFileHelper helper)
            {
                if (element == null)
                {
                    ServerObjectCacheSettings.MaxWriters = 4;
                    ServerObjectCacheSettings.MaxReaders = 4;
                    ServerObjectCacheSettings.MaxMessages = 4;
                    ServerObjectCacheSettings.MaxMessageBuffers = 4;
                    ServerObjectCacheSettings.MaxSocketAsyncEventArgs = 32;
                    ServerObjectCacheSettings.MaxActionDispatcherTasks = 16;
                    ServerObjectCacheSettings.MaxAutoRecyclingArrays = 4;
                    ServerObjectCacheSettings.MaxMessageReceivedEventArgs = 4;

                    ServerObjectCacheSettings.ExtraSmallMemoryBlockSize = 16;
                    ServerObjectCacheSettings.MaxExtraSmallMemoryBlocks = 4;
                    ServerObjectCacheSettings.SmallMemoryBlockSize = 64;
                    ServerObjectCacheSettings.MaxSmallMemoryBlocks = 4;
                    ServerObjectCacheSettings.MediumMemoryBlockSize = 256;
                    ServerObjectCacheSettings.MaxMediumMemoryBlocks = 4;
                    ServerObjectCacheSettings.LargeMemoryBlockSize = 1024;
                    ServerObjectCacheSettings.MaxLargeMemoryBlocks = 4;
                    ServerObjectCacheSettings.ExtraLargeMemoryBlockSize = 4096;
                    ServerObjectCacheSettings.MaxExtraLargeMemoryBlocks = 4;
                }
                else
                {
                    ServerObjectCacheSettings.MaxWriters = helper.ReadUInt16AttributeOrDefault(element, "maxCachedWriters", 4);
                    ServerObjectCacheSettings.MaxReaders = helper.ReadUInt16AttributeOrDefault(element, "maxCachedReaders", 4);
                    ServerObjectCacheSettings.MaxMessages = helper.ReadUInt16AttributeOrDefault(element, "maxCachedMessages", 4);
                    ServerObjectCacheSettings.MaxMessageBuffers = helper.ReadUInt16AttributeOrDefault(element, "maxMessageBuffers", 4);
                    ServerObjectCacheSettings.MaxSocketAsyncEventArgs = helper.ReadUInt16AttributeOrDefault(element, "maxCachedSocketAsyncEventArgs", 32);
                    ServerObjectCacheSettings.MaxActionDispatcherTasks = helper.ReadUInt16AttributeOrDefault(element, "maxActionDispatcherTasks", 16);
                    ServerObjectCacheSettings.MaxAutoRecyclingArrays = helper.ReadUInt16AttributeOrDefault(element, "maxAutoRecyclingArrays", 4);
                    ServerObjectCacheSettings.MaxMessageReceivedEventArgs= helper.ReadUInt16AttributeOrDefault(element, "maxMessageReceivedEventArgs", 4);

                    ServerObjectCacheSettings.ExtraSmallMemoryBlockSize = helper.ReadUInt16AttributeOrDefault(element, "extraSmallMemoryBlockSize", 16);
                    ServerObjectCacheSettings.MaxExtraSmallMemoryBlocks = helper.ReadUInt16AttributeOrDefault(element, "maxExtraSmallMemoryBlocks", 4);
                    ServerObjectCacheSettings.SmallMemoryBlockSize = helper.ReadUInt16AttributeOrDefault(element, "smallMemoryBlockSize", 64);
                    ServerObjectCacheSettings.MaxSmallMemoryBlocks = helper.ReadUInt16AttributeOrDefault(element, "maxSmallMemoryBlocks", 4);
                    ServerObjectCacheSettings.MediumMemoryBlockSize = helper.ReadUInt16AttributeOrDefault(element, "mediumMemoryBlockSize", 256);
                    ServerObjectCacheSettings.MaxMediumMemoryBlocks = helper.ReadUInt16AttributeOrDefault(element, "maxMediumMemoryBlocks", 4);
                    ServerObjectCacheSettings.LargeMemoryBlockSize = helper.ReadUInt16AttributeOrDefault(element, "largeMemoryBlockSize", 1024);
                    ServerObjectCacheSettings.MaxLargeMemoryBlocks = helper.ReadUInt16AttributeOrDefault(element, "maxLargeMemoryBlocks", 4);
                    ServerObjectCacheSettings.ExtraLargeMemoryBlockSize = helper.ReadUInt16AttributeOrDefault(element, "extraLargeMemoryBlockSize", 4096);
                    ServerObjectCacheSettings.MaxExtraLargeMemoryBlocks = helper.ReadUInt16AttributeOrDefault(element, "maxExtraLargeMemoryBlocks", 4);
                }
            }
        }

        /// <summary>
        ///     Holds settings related to loading the listeners system.
        /// </summary>
        [Serializable]
        public class ListenersSettings
        {
            /// <summary>
            ///     The listeners to use.
            /// </summary>
            public List<NetworkListenerSettings> NetworkListeners { get; } = new List<NetworkListenerSettings>();

            /// <summary>
            ///     Holds settings about a network listener.
            /// </summary>
            [Serializable]
            public class NetworkListenerSettings
            {
                /// <summary>
                ///     The name of the listener.
                /// </summary>
                public string Name { get; set; }

                /// <summary>
                ///     The type of listener.
                /// </summary>
                public string Type { get; set; }

                /// <summary>
                ///     The IP address this listener will listen on.
                /// </summary>
                public IPAddress Address { get; set; }

                /// <summary>
                ///     The port this listener will listen on.
                /// </summary>
                public ushort Port { get; set; }

                /// <summary>
                ///     Settings that should be loaded for this listener.
                /// </summary>
                public NameValueCollection Settings { get; } = new NameValueCollection();

                /// <summary>
                ///     Creates a new NetworkListenerSettings object.
                /// </summary>
                // TODO add constructors to these objects
                public NetworkListenerSettings()
                {

                }

                /// <summary>
                ///     Loads the listener settings from the specified XML element.
                /// </summary>
                /// <param name="element">The XML element to load from.</param>
                /// <param name="helper">The XML configuration helper being used.</param>
                internal void LoadFromXmlElement(XElement element, ConfigurationFileHelper helper)
                {
                    if (element == null)
                        return;

                    Type = helper.ReadStringAttribute(
                        element,
                        "type"
                    );

                    Name = helper.ReadStringAttribute(
                        element,
                        "name"
                    );

                    Address = helper.ReadIPAttributeOrDefault(
                        element,
                        "address",
                        null
                    );

                    Port = helper.ReadUInt16AttributeOrDefault(
                        element,
                        "port",
                        0
                    );
                    
                    helper.ReadAttributeCollectionTo(
                        element.Element("settings"),
                        Settings
                    );
                }
            }

            /// <summary>
            ///     Creates a new ListenerSettings object.
            /// </summary>
            public ListenersSettings()
            {

            }

            /// <summary>
            ///     Loads the listener settings from the specified XML element.
            /// </summary>
            /// <param name="element">The XML element to load from.</param>
            /// <param name="helper">The XML configuration helper being used.</param>
            internal void LoadFromXmlElement(XElement element, ConfigurationFileHelper helper)
            {
                if (element == null)
                    return;

                helper.ReadElementCollectionTo(
                    element,
                    "listener",
                    e => {
                        NetworkListenerSettings s = new NetworkListenerSettings();
                        s.LoadFromXmlElement(e, helper);
                        return s;
                    },
                    NetworkListeners
                );
            }
        }

        /// <summary>
        ///     Holds settings related to loading the metrics system.
        /// </summary>
        [Serializable]
        public class MetricsSettings
        {
            /// <summary>
            ///     The metrics writer to use.
            /// </summary>
            public MetricsWriterSettings MetricsWriter { get; } = new MetricsWriterSettings();

            /// <summary>
            ///     Whether to enable metrics that get emitted per message.
            /// </summary>
            public bool EnablePerMessageMetrics { get; set; }

            /// <summary>
            ///     Holds settings about a metrics writer.
            /// </summary>
            [Serializable]
            public class MetricsWriterSettings
            {
                /// <summary>
                ///     The type of plugin.
                /// </summary>
                public string Type { get; set; }

                /// <summary>
                ///     Settings that should be loaded for this metrics writer.
                /// </summary>
                public NameValueCollection Settings { get; } = new NameValueCollection();

                /// <summary>
                ///     Loads the metrics writer settings from the specified XML element.
                /// </summary>
                /// <param name="element">The XML element to load from.</param>
                /// <param name="helper">The XML configuration helper being used.</param>
                internal void LoadFromXmlElement(XElement element, ConfigurationFileHelper helper)
                {
                    if (element == null)
                        return;

                    //Type attribute.
                    Type = helper.ReadStringAttribute(
                        element,
                        "type"
                    );

                    helper.ReadAttributeCollectionTo(
                        element.Element("settings"),
                        Settings
                    );
                }
            }

            /// <summary>
            ///     Creates a new MetricsSettings object.
            /// </summary>
            public MetricsSettings()
            {

            }

            /// <summary>
            ///     Loads the metrics settings from the specified XML element.
            /// </summary>
            /// <param name="element">The XML element to load from.</param>
            /// <param name="helper">The XML configuration helper being used.</param>
            internal void LoadFromXmlElement(XElement element, ConfigurationFileHelper helper)
            {
                if (element == null)
                    return;

                // Load writer
                MetricsWriter.LoadFromXmlElement(element.Element("metricsWriter"), helper);

                EnablePerMessageMetrics = helper.ReadBooleanAttribute(
                    element,
                    "enablePerMessageMetrics",
                    false
                );
            }
        }

        /// <summary>
        ///     Holds settings related to loading the server registry.
        /// </summary>
        [Serializable]
        public class ServerRegistrySettings
        {
            /// <summary>
            ///     The server registry connector to use.
            /// </summary>
            public ServerRegistryConnectorSettings ServerRegistryConnector { get; } = new ServerRegistryConnectorSettings();

            /// <summary>
            ///     The host the server is advertised on.
            /// </summary>
            public string AdvertisedHost { get; set; }

            /// <summary>
            ///     The port the server is advertised on.
            /// </summary>
            public ushort AdvertisedPort { get; set; }

            /// <summary>
            ///     Holds settings about a server registry connector.
            /// </summary>
            [Serializable]
            public class ServerRegistryConnectorSettings
            {
                /// <summary>
                ///     The type of plugin.
                /// </summary>
                public string Type { get; set; }

                /// <summary>
                ///     Settings that should be loaded for this server registry connector.
                /// </summary>
                public NameValueCollection Settings { get; } = new NameValueCollection();

                /// <summary>
                ///     Loads the server registry connector settings from the specified XML element.
                /// </summary>
                /// <param name="element">The XML element to load from.</param>
                /// <param name="helper">The XML configuration helper being used.</param>
                internal void LoadFromXmlElement(XElement element, ConfigurationFileHelper helper)
                {
                    if (element == null)
                        return;

                    //Type attribute.
                    Type = helper.ReadStringAttribute(
                        element,
                        "type"
                    );

                    helper.ReadAttributeCollectionTo(
                        element.Element("settings"),
                        Settings
                    );
                }
            }

            /// <summary>
            ///     Creates a new ServerRegistrySettings object.
            /// </summary>
            public ServerRegistrySettings()
            {

            }

            /// <summary>
            ///     Loads the server registry settings from the specified XML element.
            /// </summary>
            /// <param name="element">The XML element to load from.</param>
            /// <param name="helper">The XML configuration helper being used.</param>
            internal void LoadFromXmlElement(XElement element, ConfigurationFileHelper helper)
            {
                if (element == null)
                    return;

                // Load writer
                ServerRegistryConnector.LoadFromXmlElement(element.Element("serverRegistryConnector"), helper);


                //Read address
                AdvertisedHost = helper.ReadStringAttribute(
                    element,
                    "advertisedHost"
                );

                //Read port
                AdvertisedPort = helper.ReadUInt16AttributeOrDefault(
                    element,
                    "advertisedPort",
                    4298
                );
            }
        }

        /// <summary>
        ///     Creates a server spawn data from specified XML configuration file.
        /// </summary>
        /// <param name="filePath">The path of the XML file.</param>
        /// <param name="variables">The variables to inject into the configuration.</param>
        /// <returns>The ServerSpawnData created.</returns>
        public static ServerSpawnData CreateFromXml(string filePath, NameValueCollection variables)
        {
            return CreateFromXml(XDocument.Load(filePath, LoadOptions.SetLineInfo), variables);
        }

        /// <summary>
        ///     Creates a server spawn data from specified XML configuration file.
        /// </summary>
        /// <param name="document">The XML file.</param>
        /// <param name="variables">The variables to inject into the configuration.</param>
        /// <returns>The ServerSpawnData created.</returns>
        public static ServerSpawnData CreateFromXml(XDocument document, NameValueCollection variables)
        {
            //Create a new server spawn data.
            ServerSpawnData spawnData = new ServerSpawnData();

            ConfigurationFileHelper helper = new ConfigurationFileHelper(variables, $"{new DarkRiftInfo(DateTime.Now).DocumentationRoot}configuration/server/", $"{new DarkRiftInfo(DateTime.Now).DocumentationRoot}advanced/configuration_variables.html");

            XElement root = document.Root;

            spawnData.Server.LoadFromXmlElement(helper.GetRequiredElement(root, "server"), helper);
            spawnData.PluginSearch.LoadFromXmlElement(helper.GetRequiredElement(root, "pluginSearch"), helper);
            spawnData.Data.LoadFromXmlElement(root.Element("data"), helper);
            spawnData.Logging.LoadFromXmlElement(helper.GetRequiredElement(root, "logging"), helper);
            spawnData.Plugins.LoadFromXmlElement(helper.GetRequiredElement(root, "plugins"), helper);
#pragma warning disable CS0618 // Type or member is obsolete
            spawnData.Databases.LoadFromXmlElement(root.Element("databases"), helper);
#pragma warning restore CS0618 // Type or member is obsolete
            spawnData.ServerRegistry.LoadFromXmlElement(root.Element("serverRegistry"), helper);
            spawnData.Cache.LoadFromXmlElement(root.Element("cache"), helper);
            spawnData.Listeners.LoadFromXmlElement(helper.GetRequiredElement(root, "listeners"), helper);
            spawnData.Metrics.LoadFromXmlElement(root.Element("metrics"), helper);

            //Return the new spawn data!
            return spawnData;
        }

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public ServerSpawnData()
        {

        }

        /// <summary>
        ///     Creates a new server spawn data with necessary settings.
        /// </summary>
        /// <param name="address">The address the server should listen on.</param>
        /// <param name="port">The port the server should listen on.</param>
        /// <param name="ipVersion">The IP protocol the server should listen on.</param>
        [Obsolete("Specify these properties using a network listener.")]
        public ServerSpawnData(IPAddress address, ushort port, IPVersion ipVersion)
        {
            this.Server.Address = address;
            this.Server.Port = port;
            this.Server.IPVersion = ipVersion;
        }
    }
}
