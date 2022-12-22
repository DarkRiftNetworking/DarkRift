/*
Copyright (c) 2022 Unordinal AB

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using UnityEngine;
using System.Collections;

using DarkRift.Server;
using DarkRift;
using System;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace DarkRift.Server.Unity
{
    [AddComponentMenu("DarkRift/Server (Legacy)")]
    [Obsolete("The UnityServer component is deprecated in favour of the XmlUnityServer due to better configuration options.")]
	public sealed class UnityServer : MonoBehaviour
	{
        #region Server settings

        /// <summary>
        ///     The address the server listens on.
        /// </summary>
        public IPAddress Address
        {
            get { return address; }
            set { address = value; }
        }

        [SerializeField]
        [Tooltip("The address the server will listen on.")]
        private IPAddress address = IPAddress.Any;

        /// <summary>
        ///     The port the server listens on.
        /// </summary>
        public ushort Port
        {
            get { return port; }
            set { port = value; }
        }

        [SerializeField]
        [Tooltip("The port the server will listen on.")]
        private ushort port = 4296;

        /// <summary>
        ///     The number of strikes that can be received before the client is automatically kicked.
        /// </summary>
        public byte MaxStrikes
        {
            get { return maxStrikes; }
            set { maxStrikes = value; }
        }

        [SerializeField]
        [Tooltip("The number of strikes that can be received before the client is automatically kicked.")]
        private byte maxStrikes = 3;

        #endregion
        
        #region Data settings

        /// <summary>
        ///     The location DarkRift will store persistant data.
        /// </summary>
        public string DataDirectory
        {
            get { return dataDirectory; }
            set { dataDirectory = value; }
        }
            
        [SerializeField]
        [Tooltip("The location DarkRift will store persistant data.")]
        private string dataDirectory = "Data/";

        #endregion

        #region Logging settings

        /// <summary>
        ///     Whether logs should be written to file.
        /// </summary>
        public bool LogToFile
        {
            get { return logToFile; }
            set { logToFile = value; }
        }

        [SerializeField]
        [Tooltip("Indicates whether logs should be written to file.")]
        private bool logToFile = true;

        /// <summary>
        ///     The location that log files will be placed when using recommended logging settings.
        /// </summary>
        public string LogFileString
        {
            get { return logFileString; }
            set { logFileString = value; }
        }

        [SerializeField]
        [Tooltip("The location log files will be written to.")]
        private string logFileString = @"Logs/{0:d-M-yyyy}/{0:HH-mm-ss tt}.txt";

        /// <summary>
        ///     Whether logs should be written to the unity console.
        /// </summary>
        public bool LogToUnityConsole
        {
            get { return logToUnityConsole; }
            set { logToUnityConsole = value; }
        }

        [SerializeField]
        [Tooltip("Indicates whether logs should be written to the unity console.")]
        private bool logToUnityConsole = true;

        /// <summary>
        ///     Whether logs should be written to debug.
        /// </summary>
        public bool LogToDebug
        {
            get { return logToDebug; }
            set { logToDebug = value; }
        }

        [SerializeField]
        [Tooltip("Indicates whether logs should be written to debug.")]
        private bool logToDebug = true;

        #endregion

        #region Plugin settings

        /// <summary>
        ///     Whether plugins should be loaded by default.
        /// </summary>
        public bool LoadByDefault
        {
            get { return loadByDefault; }
            set { loadByDefault = value; }
        }

        [SerializeField]
        [Tooltip("Indicates whether plugins should be loaded by default.")]
        private bool loadByDefault = true;

        /// <summary>
        ///     The plugins that should be loaded.
        /// </summary>
        public List<ServerSpawnData.PluginsSettings.PluginSettings> Plugins
        {
            get { return plugins; }
        }
        
        [HideInInspector]
        private readonly List<ServerSpawnData.PluginsSettings.PluginSettings> plugins = new List<ServerSpawnData.PluginsSettings.PluginSettings>();
        
        #endregion

        #region Database settings

        /// <summary>
        ///     The databases that the server will connect to.
        /// </summary>
        public List<ServerSpawnData.DatabaseSettings.DatabaseConnectionData> Databases
        {
            get { return databases; }
        }

        [HideInInspector]
        private readonly List<ServerSpawnData.DatabaseSettings.DatabaseConnectionData> databases = new List<ServerSpawnData.DatabaseSettings.DatabaseConnectionData>();

        #endregion

        #region Cache settings

        /// <summary>
        ///     The maximum number of <see cref="DarkRiftWriter"/> instances stored per thread.
        /// </summary>
        public int MaxCachedWriters
        {
            get
            {
                return maxCachedWriters;
            }

            set
            {
                maxCachedWriters = value;
            }
        }

        [SerializeField]
        [Tooltip("The maximum number of DarkRiftWriter instances stored per thread.")]
        private int maxCachedWriters = 2;

        /// <summary>
        ///     The maximum number of <see cref="DarkRiftReader"/> instances stored per thread.
        /// </summary>
        public int MaxCachedReaders
        {
            get
            {
                return maxCachedReaders;
            }

            set
            {
                maxCachedReaders = value;
            }
        }

        [SerializeField]
        [Tooltip("The maximum number of DarkRiftReader instances stored per thread.")]
        private int maxCachedReaders = 2;

        /// <summary>
        ///     The maximum number of <see cref="Message"/> instances stored per thread.
        /// </summary>
        public int MaxCachedMessages
        {
            get
            {
                return maxCachedMessages;
            }

            set
            {
                maxCachedMessages = value;
            }
        }

        [SerializeField]
        [Tooltip("The maximum number of Message instances stored per thread.")]
        private int maxCachedMessages = 8;

        /// <summary>
        ///     The maximum number of <see cref="System.Net.Sockets.SocketAsyncEventArgs"/> instances stored per thread.
        /// </summary>
        public int MaxCachedSocketAsyncEventArgs
        {
            get
            {
                return maxCachedSocketAsyncEventArgs;
            }

            set
            {
                maxCachedSocketAsyncEventArgs = value;
            }
        }

        [SerializeField]
        [Tooltip("The maximum number of SocketAsyncEventArg instances stored per thread.")]
        private int maxCachedSocketAsyncEventArgs = 32;

        /// <summary>
        ///     The maximum number of <see cref="DarkRift.Dispatching.ActionDispatcherTask"/> instances stored per thread.
        /// </summary>
        public int MaxCachedActionDispatcherTasks
        {
            get
            {
                return maxCachedActionDispatcherTasks;
            }

            set
            {
                maxCachedActionDispatcherTasks = value;
            }
        }

        [SerializeField]
        [Tooltip("The maximum number of ActionDispatcherTask instances stored per thread.")]
        private int maxCachedActionDispatcherTasks = 16;


        #endregion

#pragma warning disable IDE0044 // Add readonly modifier, Unity can't serialize readonly fields
        [SerializeField]
        [Tooltip("Indicates whether the server will be created in the OnEnable method.")]
        private bool createOnEnable = true;

        [SerializeField]
        [Tooltip("Indicates whether the server events will be routed through the dispatcher or just invoked.")]
        private bool eventsFromDispatcher = true;
#pragma warning restore IDE0044 // Add readonly modifier, Unity can't serialize readonly fields

        /// <summary>
        ///     The actual server.
        /// </summary>
        public DarkRiftServer Server { get; private set; }

        private void OnEnable()
        {
            //If createOnEnable is selected create a server
            if (createOnEnable)
                Create();
        }

        private void Update()
        {
            //Execute all queued dispatcher tasks
            if (Server != null)
                Server.ExecuteDispatcherTasks();
        }

        /// <summary>
        ///     Creates the server.
        /// </summary>
        public void Create()
        {
            if (Server != null)
                throw new InvalidOperationException("The server has already been created! (Is CreateOnEnable enabled?)");

            ServerSpawnData spawnData = new ServerSpawnData();
            spawnData.Server.Address = address;
            spawnData.Server.Port = port;

            //Server settings
            spawnData.Server.MaxStrikes = maxStrikes;
            //This is an obsolete property but is still used if the user is using obsolete Server properties as we are
#pragma warning disable 0618
            spawnData.Server.UseFallbackNetworking = true;      //Unity is broken, work around it...
#pragma warning restore 0618
            spawnData.EventsFromDispatcher = eventsFromDispatcher;

            //Plugin search settings
            spawnData.PluginSearch.PluginTypes.AddRange(UnityServerHelper.SearchForPlugins());
            spawnData.PluginSearch.PluginTypes.Add(typeof(UnityConsoleWriter));

            //Data settings
            spawnData.Data.Directory = dataDirectory;

            //Logging settings
            spawnData.Plugins.LoadByDefault = true;

            if (logToFile)
            {
                ServerSpawnData.LoggingSettings.LogWriterSettings fileWriter = new ServerSpawnData.LoggingSettings.LogWriterSettings {
                    Name = "FileWriter1",
                    Type = "FileWriter",
                    LogLevels = new LogType[] { LogType.Trace, LogType.Info, LogType.Warning, LogType.Error, LogType.Fatal }
                };
                fileWriter.Settings["file"] = logFileString;
                spawnData.Logging.LogWriters.Add(fileWriter);
            }

            if (logToUnityConsole)
            {
                ServerSpawnData.LoggingSettings.LogWriterSettings consoleWriter = new ServerSpawnData.LoggingSettings.LogWriterSettings {
                    Name = "UnityConsoleWriter1",
                    Type = "UnityConsoleWriter",
                    LogLevels = new LogType[] { LogType.Info, LogType.Warning, LogType.Error, LogType.Fatal }
                };
                spawnData.Logging.LogWriters.Add(consoleWriter);
            }

            if (logToDebug)
            {
                ServerSpawnData.LoggingSettings.LogWriterSettings debugWriter = new ServerSpawnData.LoggingSettings.LogWriterSettings {
                    Name = "DebugWriter1",
                    Type = "DebugWriter",
                    LogLevels = new LogType[] { LogType.Warning, LogType.Error, LogType.Fatal }
                };
                spawnData.Logging.LogWriters.Add(debugWriter);
            }

            //Plugins
            spawnData.Plugins.LoadByDefault = loadByDefault;
            spawnData.Plugins.Plugins.AddRange(plugins);

            //Databases
            spawnData.Databases.Databases.AddRange(databases);

            //Cache
            spawnData.Cache.MaxCachedWriters = MaxCachedWriters;
            spawnData.Cache.MaxCachedReaders = MaxCachedReaders;
            spawnData.Cache.MaxCachedMessages = MaxCachedMessages;
            spawnData.Cache.MaxCachedSocketAsyncEventArgs = MaxCachedSocketAsyncEventArgs;
            spawnData.Cache.MaxActionDispatcherTasks = MaxCachedActionDispatcherTasks;

            Server = new DarkRiftServer(spawnData);
            Server.Start();
        }

        private void OnDisable()
        {
            Close();
        }

        private void OnApplicationQuit()
        {
            Close();
        }

        /// <summary>
        ///     Closes the server.
        /// </summary>
        public void Close()
        {
            if (Server != null)
                Server.Dispose();
        }
    }
}
