/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Server.Metrics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DarkRift.Server
{
#if PRO
    /// <summary>
    ///     Base class for server groups.
    /// </summary>
    internal abstract class ServerGroup<T> : IModifiableServerGroup, IDisposable where T : IRemoteServer, IDisposable
    {
        /// <inheritdoc />
        public IRemoteServer this[ushort id] => GetRemoteServer(id);

        /// <inheritdoc />
        public int Count
        {
            get
            {
                lock (servers)
                    return servers.Count;
            }
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public ServerVisibility Visibility { get; }

        /// <inheritdoc />
        public abstract ServerConnectionDirection Direction { get; }

        /// <summary>
        ///     Event raised when a server joins the group.
        /// </summary>
        public event EventHandler<ServerJoinedEventArgs> ServerJoined;

        /// <summary>
        ///     Event raised when a server leaves the group.
        /// </summary>
        public event EventHandler<ServerLeftEventArgs> ServerLeft;

        /// <summary>
        ///     The thread helper to use.
        /// </summary>
        private readonly DarkRiftThreadHelper threadHelper;

        /// <summary>
        ///     The servers in this group.
        /// </summary>
        private readonly Dictionary<ushort, T> servers = new Dictionary<ushort, T>();

        /// <summary>
        ///     The logger to use.
        /// </summary>
        private readonly Logger logger;

        /// <summary>
        ///     Gauge metric of the number of servers currently connected.
        /// </summary>
        private readonly IGaugeMetric serversConnectedGauge;

        /// <summary>
        ///     Histogram metric of time taken to execute the <see cref="ServerJoined"/> event.
        /// </summary>
        private readonly IHistogramMetric serverJoinedEventTimeHistogram;

        /// <summary>
        ///     Histogram metric of time taken to execute the <see cref="ServerLeft"/> event.
        /// </summary>
        private readonly IHistogramMetric serverLeftEventTimeHistogram;

        /// <summary>
        ///     Counter metric of failures executing the <see cref="ServerJoined"/> event.
        /// </summary>
        private readonly ICounterMetric serverJoinedEventFailuresCounter;

        /// <summary>
        ///     Counter metric of failures executing the <see cref="ServerLeft"/> event.
        /// </summary>
        private readonly ICounterMetric serverLeftEventFailuresCounter;

        public ServerGroup(string name, ServerVisibility visibility, DarkRiftThreadHelper threadHelper, Logger logger, MetricsCollector metricsCollector)
        {
            this.Name = name;
            this.Visibility = visibility;

            this.threadHelper = threadHelper;
            this.logger = logger;

            serversConnectedGauge = metricsCollector.Gauge("remote_servers_connected", "The number of servers connected to the server in this group.", "group").WithTags(name);
            serverJoinedEventTimeHistogram = metricsCollector.Histogram("remote_server_joined_event_time", "The time taken to execute the ServerJoined event.", "group").WithTags(name);
            serverLeftEventTimeHistogram = metricsCollector.Histogram("remote_server_left_event_time", "The time taken to execute the ServerLeft event.", "group").WithTags(name);
            serverJoinedEventFailuresCounter = metricsCollector.Counter("remote_server_joined_event_failures", "The number of failures executing the ServerJoined event.", "group").WithTags(name);
            serverLeftEventFailuresCounter = metricsCollector.Counter("remote_server_left_event_failures", "The number of failures executing the ServerLeft event.", "group").WithTags(name);
        }

        /// <inheritdoc/>
        public IRemoteServer[] GetAllRemoteServers()
        {
            lock (servers)
                return servers.Values.Cast<IRemoteServer>().ToArray();
        }

        /// <inheritdoc/>
        public IRemoteServer GetRemoteServer(ushort id)
        {
            lock (servers)
                return servers[id];
        }

        /// <inheritdoc />
        public abstract void HandleServerJoin(ushort id, string host, ushort port, IDictionary<string, string> properties);

        /// <summary>
        ///     Handles the event for a new server joining the cluster.
        /// </summary>
        /// <param name="id">The id of the server joining.</param>
        /// <param name="remoteServer">The server joining.</param>
        protected void HandleServerJoinEvent(ushort id, IRemoteServer remoteServer)
        {
            EventHandler<ServerJoinedEventArgs> handler = ServerJoined;
            if (handler != null)
            {
                void DoServerJoinEvent()
                {
                    long startTimestamp = Stopwatch.GetTimestamp();

                    try
                    {
                        handler?.Invoke(this, new ServerJoinedEventArgs(remoteServer, id, this));
                    }
                    catch (Exception e)
                    {
                        serverJoinedEventFailuresCounter.Increment();

                        // TODO this seems bad, shoudln't we disconnect them?
                        logger.Error("A plugin encountered an error whilst handling the ServerJoined event. The server will still be connected. (See logs for exception)", e);
                    }

                    double time = (double)(Stopwatch.GetTimestamp() - startTimestamp) / Stopwatch.Frequency;
                    serverJoinedEventTimeHistogram.Report(time);
                }

                threadHelper.DispatchIfNeeded(DoServerJoinEvent);
            }
        }

        /// <inheritdoc />
        public abstract void HandleServerLeave(ushort id);

        /// <summary>
        ///     Handles the event for a server leaving the cluster.
        /// </summary>
        /// <param name="id">The server leaving.</param>
        /// <param name="remoteServer">The server leaving.</param>
        protected void HandleServerLeaveEvent(ushort id, IRemoteServer remoteServer)
        {
            EventHandler<ServerLeftEventArgs> handler = ServerLeft;
            if (handler != null)
            {
                void DoServerLeaveEvent()
                {
                    long startTimestamp = Stopwatch.GetTimestamp();

                    try
                    {
                        handler?.Invoke(this, new ServerLeftEventArgs(remoteServer, id, this));
                    }
                    catch (Exception e)
                    {
                        serverLeftEventFailuresCounter.Increment();

                        logger.Error("A plugin encountered an error whilst handling the ServerLeft event. (See logs for exception)", e);
                    }

                    double time = (double)(Stopwatch.GetTimestamp() - startTimestamp) / Stopwatch.Frequency;
                    serverLeftEventTimeHistogram.Report(time);
                }

                threadHelper.DispatchIfNeeded(DoServerLeaveEvent);
            }
        }

        /// <summary>
        ///     Adds a server to the group.
        /// </summary>
        /// <param name="remoteServer">The server to add.</param>
        protected void AddServer(T remoteServer)
        {
            int count;
            lock (servers)
            {
                servers.Add(remoteServer.ID, remoteServer);
                count = servers.Count;
            }

            serversConnectedGauge.Report(count);
        }

        /// <summary>
        ///     Removes a server from the group.
        /// </summary>
        /// <param name="id">The id of the server to remove.</param>
        protected T RemoveServer(ushort id)
        {
            T remoteServer;
            int count;
            lock (servers)
            {
                remoteServer = servers[id];
                servers.Remove(id);
                count = servers.Count;
            }

            serversConnectedGauge.Report(count);

            return remoteServer;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    lock (servers)
                    {
                        foreach (T server in servers.Values)
                            server.Dispose();
                    }
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
