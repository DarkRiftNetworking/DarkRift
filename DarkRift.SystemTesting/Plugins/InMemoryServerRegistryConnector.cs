/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DarkRift.Server;
#if PRO
using Timer = DarkRift.Server.Timer;

namespace DarkRift.SystemTesting.Plugins
{
    /// <summary>
    ///     Simple server registry that operates in static memory.
    /// </summary>
    internal class InMemoryServerRegistryConnector : ServerRegistryConnector
    {
        public override bool ThreadSafe => true;

        public override Version Version => new Version(1, 0, 0);

        /// <summary>
        ///     The lookup of servers in the registry.
        /// </summary>
        private static readonly ConcurrentDictionary<ushort, RegistryEntry> registry = new ConcurrentDictionary<ushort, RegistryEntry>();

        /// <summary>
        ///     The last ID allocated.
        /// </summary>
        private static int lastId = -1;

        /// <summary>
        /// The timer this plugin is polling with.
        /// </summary>
        private readonly Timer timer;

        public InMemoryServerRegistryConnector(ServerRegistryConnectorLoadData serverRegistryConnectorLoadData) : base(serverRegistryConnectorLoadData)
        {
            timer = CreateTimer(1000, 1000, FetchServices);
        }

        private void FetchServices(Timer obj)
        {
            var knownServices = RemoteServerManager.GetAllGroups().SelectMany(g => g.GetAllRemoteServers()).Select(s => s.ID);
            var joined = registry.Keys.Except(knownServices);
            var left = knownServices.Except(registry.Keys);

            foreach (ushort joinedID in joined)
            {
                if (joinedID != RemoteServerManager.ServerID)
                {
                    RegistryEntry service = registry[joinedID];
                    string group = service.Group;

                    Logger.Trace($"Discovered server {joinedID} from group '{group}'.");

                    HandleServerJoin(joinedID, group, service.Host, service.Port, new Dictionary<string, string>(service.Properties));
                }
            }

            //TODO consider just a set method instead of/as well as join/leave
            foreach (ushort leftID in left)
            {
                if (leftID != RemoteServerManager.ServerID)
                {
                    Logger.Trace($"Server {leftID} has left the cluster.");

                    HandleServerLeave(leftID);
                }
            }
        }

        protected override void DeregisterServer()
        {
            bool success = registry.TryRemove(RemoteServerManager.ServerID, out _);
            if (!success)
                throw new InvalidOperationException("Failed to add an entry into the server registry as the ID already existed.");
        }

        protected override ushort RegisterServer(string group, string host, ushort port, IDictionary<string, string> properties)
        {
            ushort id = (ushort)Interlocked.Increment(ref lastId);

            bool success = registry.TryAdd(id, new RegistryEntry(group, host, port, properties));
            if (!success)
                throw new InvalidOperationException("Failed to remove entry in the server registry as the ID does not exist.");

            return id;
        }

        /// <summary>
        ///     Resets the registry back to its original state ready for the next test.
        /// </summary>
        public static void Reset()
        {
            registry.Clear();
            Interlocked.Exchange(ref lastId, -1);
        }

        /// <summary>
        ///     An entry in the server registry.
        /// </summary>
        private class RegistryEntry
        {
            public string Group { get; }
            public string Host { get; }
            public ushort Port { get; }
            public IReadOnlyDictionary<string, string> Properties { get; }

            public RegistryEntry(string group, string address, ushort port, IDictionary<string, string> properties)
            {
                this.Group = group;
                this.Host = address;
                this.Port = port;
                this.Properties = new Dictionary<string, string>(properties);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            timer.Dispose();
        }
    }
}
#endif
