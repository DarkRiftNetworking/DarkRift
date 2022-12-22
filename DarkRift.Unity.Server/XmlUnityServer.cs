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
using System;
using System.Xml.Linq;
using System.Collections.Specialized;
using System.Threading;

namespace DarkRift.Server.Unity
{
    [AddComponentMenu("DarkRift/Server")]
    public class XmlUnityServer : MonoBehaviour
    {
        /// <summary>
        ///     The actual server.
        /// </summary>
        public DarkRiftServer Server { get; private set; }

#pragma warning disable IDE0044 // Add readonly modifier, Unity can't serialize readonly fields
        [SerializeField]
        [Tooltip("The configuration file to use.")]
        private TextAsset configuration;

        [SerializeField]
        [Tooltip("Indicates whether the server will be created in the OnEnable method.")]
        private bool createOnEnable = true;

        [SerializeField]
        [Tooltip("Indicates whether the server events will be routed through the dispatcher or just invoked.")]
        private bool eventsFromDispatcher = true;
#pragma warning restore IDE0044 // Add readonly modifier, Unity can't serialize readonly fields

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
            Create(new NameValueCollection());
        }

        /// <summary>
        ///     Creates the server.
        /// </summary>
        public void Create(NameValueCollection variables)
        {
            if (Server != null)
                throw new InvalidOperationException("The server has already been created! (Is CreateOnEnable enabled?)");
            
            if (configuration != null)
            {
                // Create spawn data from config
                ServerSpawnData spawnData = ServerSpawnData.CreateFromXml(XDocument.Parse(configuration.text), variables);

                // Allow only this thread to execute dispatcher tasks to enable deadlock protection
                spawnData.DispatcherExecutorThreadID = Thread.CurrentThread.ManagedThreadId;

                // Inaccessible from XML, set from inspector
                spawnData.EventsFromDispatcher = eventsFromDispatcher;

                // Unity is broken, work around it...
                // This is an obsolete property but is still used if the user is using obsolete <server> tag properties
#pragma warning disable 0618
                spawnData.Server.UseFallbackNetworking = true;
#pragma warning restore 0618

                // Add types
                spawnData.PluginSearch.PluginTypes.AddRange(UnityServerHelper.SearchForPlugins());
                spawnData.PluginSearch.PluginTypes.Add(typeof(UnityConsoleWriter));

                // Create server
                Server = new DarkRiftServer(spawnData);
                Server.StartServer();
            }
            else
                Debug.LogError("No configuration file specified! Please ensure there is a configuration file set on the XmlUnityServer component before starting the server.");
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
