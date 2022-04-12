/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace DarkRift.Server.Plugins.HealthCheck
{
    /// <summary>
    ///     Implements a simple HTTP health check.
    /// </summary>
    internal class HttpHealthCheck : Plugin
    {
        public override bool ThreadSafe => true;

        public override Version Version => new Version(1, 0, 0);

        internal override bool Hidden => true;

        /// <summary>
        ///     Static empty array to reduce GC.
        /// </summary>
        private static readonly byte[] emptyArray = new byte[0];

        /// <summary>
        ///     The HTTP listener in use.
        /// </summary>
        private readonly HttpListener httpListener = new HttpListener();

        /// <summary>
        ///     The HTTP host we are listening on.
        /// </summary>
        private readonly string host;

        /// <summary>
        ///     The HTTP port we are listening on.
        /// </summary>
        private readonly ushort port;

        /// <summary>
        ///     The HTTP path we are listening on.
        /// </summary>
        private readonly string path;

        /// <summary>
        ///     The background thread listening for health check requests.
        /// </summary>
        private Thread listenThread;

        /// <summary>
        ///     If the serevr is still running or not.
        /// </summary>
        private volatile bool running = true;

        public HttpHealthCheck(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            host = pluginLoadData.Settings["host"] ?? "localhost";

            port = 10666;
            if(pluginLoadData.Settings["port"] != null)
            {
                if (!ushort.TryParse(pluginLoadData.Settings["port"], out port))
                    Logger.Error($"Health check port not a valid value. Using a value of {port} instead.");
            }

            path = pluginLoadData.Settings["path"] ?? "/health";

            httpListener.Prefixes.Add($"http://{host}:{port}/");
        }

#if PRO
        protected
#endif
            internal override void Loaded(LoadedEventArgs args)
        {
            base.Loaded(args);

            httpListener.Start();
            
            listenThread = new Thread(Listen);
            listenThread.Start();

            Logger.Trace($"HTTP health check started at 'http://{host}:{port}{path}'");
        }

        private void Listen()
        {
            while (running)
            {
                HttpListenerContext context;
                try
                {
                    context = httpListener.GetContext();
                }
                catch (HttpListenerException e)
                {
                    if (e.ErrorCode != 500)
                        Logger.Warning("HTTP health check has exited prematurely as the HTTP server has reported an error.", e);
                    return;
                }

                if (context.Request.HttpMethod != "GET")
                {
                    context.Response.StatusCode = 405;
                    context.Response.Close(emptyArray, false);
                }
                else if (context.Request.Url.AbsolutePath != path)
                {
                    context.Response.StatusCode = 404;
                    context.Response.Close(emptyArray, false);
                }
                else
                {
                    context.Response.ContentType = "application/json";

                    using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
                        writer.WriteLine($"{{\"listening\": true, \"startTime\": \"{ServerInfo.StartTime:yyyy-MM-ddTHH:mm:ss.fffZ}\", \"type\": \"{ServerInfo.Type}\", \"version\": \"{ServerInfo.Version}\"}}");
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            running = false;
            httpListener.Close();
        }
    }
}
