/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Client;
using DarkRift.Server;
#if PRO
using DarkRift.SystemTesting.Plugins;
#endif
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace DarkRift.SystemTesting
{
    /// <summary>
    ///     General place to hold test data.
    /// </summary>
    [Binding]
    public class World
    {
        /// <summary>
        ///     Event fired whenever the server receives a message.
        /// </summary>
        public event EventHandler<Server.MessageReceivedEventArgs> ServerMessageReceived;

#if PRO
        /// <summary>
        ///     Event fired whenever a server connects to another.
        /// </summary>
        public event EventHandler<ServerJoinedEventArgs> ServerJoined;

        /// <summary>
        ///     Event fired whenever a server disconnects from another.
        /// </summary>
        public event EventHandler<ServerLeftEventArgs> ServerLeft;
#endif

        /// <summary>
        /// The number of times the ServerConnected event has been fired.
        /// </summary>
        public int ServerConnectedEvents => Volatile.Read(ref serverConnectedEvents);
        private int serverConnectedEvents;

        /// <summary>
        /// The number of times the ServerDisconnected event has been fired.
        /// </summary>
        public int ServerDisconnectedEvents => Volatile.Read(ref serverDisconnectedEvents);
        private int serverDisconnectedEvents;

        /// <summary>
        /// The delay between a client connecting and message handlers being assigned on the server.
        /// </summary>
        public int ClientConnectionDelay { get; internal set; }

        /// <summary>
        ///     The servers in use.
        /// </summary>
        private readonly Dictionary<ushort, DarkRiftServer> servers = new Dictionary<ushort, DarkRiftServer>();
        
        /// <summary>
        ///     The clients in use.
        /// </summary>
        private readonly Dictionary<ushort, DarkRiftClient> clients = new Dictionary<ushort, DarkRiftClient>();

        /// <summary>
        ///     Class for asserting messages received.
        /// </summary>
        private readonly MessageAssertions messageAssertions;

        public World(MessageAssertions messageAssertions)
        {
            this.messageAssertions = messageAssertions;
        }

        /// <summary>
        ///     Clears up the world data.
        /// </summary>
        [AfterScenario]
        public void AfterScenario()
        {
            foreach (DarkRiftServer server in servers.Values)
            {
                try
                {
                    server.Dispose();
                }
                catch (Exception e)
                {
                    Console.WriteLine("An Exception was thrown while disposing the server.\n" + e);
                }
            }

            foreach (DarkRiftClient client in clients.Values)
                client.Dispose();

            clients.Clear();
            servers.Clear();

#if PRO
            Interlocked.Exchange(ref serverConnectedEvents, 0);
            Interlocked.Exchange(ref serverDisconnectedEvents, 0);

            // Reset server registry
            InMemoryServerRegistryConnector.Reset();
#endif

            ClientConnectionDelay = 0;
        }

        /// <summary>
        ///     Adds a new client to the world.
        /// </summary>
        /// <param name="client">The client to add.</param>
        public void AddClient(DarkRiftClient client)
        {
            clients.Add(client.ID, client);

            client.MessageReceived += ClientMessageReceived;
        }

        /// <summary>
        ///     Adds a new server to the world.
        /// </summary>
        /// <param name="server">The serverto add.</param>
        public void AddServer(DarkRiftServer server)
        {
#if PRO
            servers.Add(server.RemoteServerManager.ServerID, server);

            server.ClientManager.ClientConnected += (s, a) => ClientConnected(s, a, server.RemoteServerManager.ServerID);

            foreach (IServerGroup group in server.RemoteServerManager.GetAllGroups())
            {
                group.ServerJoined += (s, a) => ServerJoinedGroup(s, a, server.RemoteServerManager.ServerID);
                group.ServerLeft += (s, a) => ServerLeftGroup(s, a, server.RemoteServerManager.ServerID);
            }
#else
            servers.Add(0, server);

            server.ClientManager.ClientConnected += (s, a) => ClientConnected(s, a, 0);
#endif
        }

        /// <summary>
        ///     Gets a client by ID.
        /// </summary>
        /// <param name="id">The ID to get.</param>
        public DarkRiftClient GetClient(ushort id)
        {
            return clients[id];
        }

        /// <summary>
        ///     Gets an enumerable of all clienst.
        /// </summary>
        /// <returns>All clients.</returns>
        public IEnumerable<DarkRiftClient> GetClients()
        {
            return clients.Values;
        }

        /// <summary>
        ///     Gets a server by ID.
        /// </summary>
        /// <param name="id">The ID to get.</param>
        public DarkRiftServer GetServer(ushort id)
        {
            return servers[id];
        }

        /// <summary>
        ///     Gets an enumerable of all servers.
        /// </summary>
        /// <returns>All servers.</returns>
        public IEnumerable<DarkRiftServer> GetServers()
        {
            return servers.Values;
        }

        /// <summary>
        ///     Removes the specified server.
        /// </summary>
        /// <param name="id">The server to remove.</param>
        public void RemoveServer(ushort id)
        {
            servers.Remove(id);
        }

        /// <summary>
        ///     Event handler for new clients connecting.
        /// </summary>
        /// <param name="_">The client manager.</param>
        /// <param name="args">The event args.</param>
        /// <param name="serverID">The ID of the server the client connected to.</param>
        private void ClientConnected(object _, ClientConnectedEventArgs args, ushort serverID)
        {
            if (ClientConnectionDelay > 0)
                Thread.Sleep(ClientConnectionDelay);

            args.Client.MessageReceived += (s, a) => ServerMessageReceivedFromClient(s, a, serverID);
        }

        /// <summary>
        ///     Event handler for messages arriving at a client.
        /// </summary>
        /// <param name="sender">The client.</param>
        /// <param name="args">The event args.</param>
        private void ClientMessageReceived(object sender, Client.MessageReceivedEventArgs args)
        {
            using Message message = args.GetMessage();
            string str;
            using (DarkRiftReader reader = message.GetReader())
            {
                if (reader.Length > 0)
                    str = reader.ReadString();
                else
                    str = null;
            }

            messageAssertions.AddMessageOnClient(new ReceivedMessage(str, ushort.MaxValue, ((DarkRiftClient)sender).ID, message.Tag, args.SendMode));

        }

        /// <summary>
        ///     Event handler for messages arriving at the server from a client.
        /// </summary>
        /// <param name="sender">The client that sent.</param>
        /// <param name="args">The event args.</param>
        /// <param name="serverID">The ID of the server that received this message.</param>
        private void ServerMessageReceivedFromClient(object sender, Server.MessageReceivedEventArgs args, ushort serverID)
        {
            // Record message
            using (Message message = args.GetMessage())
            {
                string str;
                using (DarkRiftReader reader = message.GetReader())
                {
                    if (reader.Length > 0)
                        str = reader.ReadString();
                    else
                        str = null;
                }

                messageAssertions.AddMessageOnServer(new ReceivedMessage(str, args.Client.ID, serverID, message.Tag, args.SendMode));
            }

            // Call to other event handlers
            ServerMessageReceived.Invoke(sender, args);
        }

        #if PRO
        /// <summary>
        ///     Handles a server joining a group.
        /// </summary>
        /// <param name="sender">The <see cref="ServerManager"/></param>
        /// <param name="e">The event args.</param>
        /// <param name="serverID">The ID of the server the server connected to.</param>
        private void ServerJoinedGroup(object sender, ServerJoinedEventArgs e, ushort serverID)
        {
            e.RemoteServer.MessageReceived += (s, a) => ServerMessageReceivedFromServer(s, a, serverID);
            e.RemoteServer.ServerConnected += (s, a) => ServerConnected(s, a, serverID);
            e.RemoteServer.ServerDisconnected += ServerDisconnected;

            ServerJoined.Invoke(sender, e);
        }

        /// <summary>
        ///     Handles a server leaving a group.
        /// </summary>
        /// <param name="sender">The <see cref="ServerManager"/></param>
        /// <param name="e">The event args.</param>
        /// <param name="serverID">The ID of the server the server connected to.</param>
        private void ServerLeftGroup(object sender, ServerLeftEventArgs e, ushort _)
        {
            // Can't unsubscribe MessageReceived as we're using a lambda, would need to assign it to a reference
            //e.RemoteServer.ServerConnected -= ServerConnected;
            e.RemoteServer.ServerDisconnected -= ServerDisconnected;

            ServerLeft.Invoke(sender, e);
        }

        /// <summary>
        /// Handles a server connecting.
        /// </summary>
        /// <param name="sender">The <see cref="ServerManager"/></param>
        /// <param name="e">The event args.</param>
        private void ServerConnected(object sender, ServerConnectedEventArgs e, int serverID)
        {
            Interlocked.Increment(ref serverConnectedEvents);
        }

        /// <summary>
        /// Handles a server disconnecting.
        /// </summary>
        /// <param name="sender">The <see cref="ServerManager"/></param>
        /// <param name="e">The event args.</param>
        private void ServerDisconnected(object sender, ServerDisconnectedEventArgs e)
        {
            Interlocked.Increment(ref serverDisconnectedEvents);
        }

        /// <summary>
        ///     Event handler for messages arriving at the server from another server.
        /// </summary>
        /// <param name="sender">The remote server that sent.</param>
        /// <param name="args">The event args.</param>
        /// <param name="serverID">The ID of the server that received this message.</param>
        private void ServerMessageReceivedFromServer(object _, Server.ServerMessageReceivedEventArgs args, ushort serverID)
        {
            // Record message
            using Message message = args.GetMessage();

            string str;
            using (DarkRiftReader reader = message.GetReader()) {
                if (reader.Length > 0)
                    str = reader.ReadString();
                else
                    str = null;
            }

            messageAssertions.AddMessageOnServer(new ReceivedMessage(str, args.RemoteServer.ID, serverID, message.Tag, args.SendMode));
        }
#endif
    }
}
