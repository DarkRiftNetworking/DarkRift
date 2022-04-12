/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Client;
using DarkRift.Server;
using DarkRift.Server.Configuration;
using DarkRift.Server.Plugins.Listeners.Bichannel;
#if PRO
using DarkRift.SystemTesting.Plugins;
#endif
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TechTalk.SpecFlow;

namespace DarkRift.SystemTesting
{
    /// <summary>
    ///     Steps for basic DarkRift client/server connection operations.
    /// </summary>
    [Binding]
    public class ConnectingSteps
    {
        /// <summary>
        ///     The world to store state in.
        /// </summary>
        private readonly World world;

        /// <summary>
        ///     The performance related test steps.
        /// </summary>
        private readonly PerformanceSteps performanceSteps;

        /// <summary>
        ///     The thread executing dispatcher tasks.
        /// </summary>
        private Thread dispatcherThread;

        public ConnectingSteps(World world, PerformanceSteps performanceSteps)
        {
            this.world = world;
            this.performanceSteps = performanceSteps;
#if PRO
            world.ServerJoined += ServerJoined;
            world.ServerLeft += ServerLeft;
#endif
        }
#if PRO
        private void ServerJoined(object sender, ServerJoinedEventArgs e) {
#if DEBUG
            // We've just requested a load of objects that wont be returned until we disconnect the servers again
            // UDP receive TCP receive in the connecting server; TCP receive in listening server
            // Only do this once, this event will be fired from both the upstream and downstream server
            if (e.ServerGroup.Direction == ServerConnectionDirection.Downstream)
                performanceSteps.ExpectedUnaccountedForSocketAsyncEventArgs += 3;
#endif
        }

        private void ServerLeft(object sender, ServerLeftEventArgs e)
        {
#if DEBUG
            // We've just returned a load of objects that we don't normally expect
            // UDP receive TCP receive in the connecting server; TCP receive in listening server
            // Only do this once, this event will be fired from both the upstream and downstream server
            if (e.ServerGroup.Direction == ServerConnectionDirection.Downstream)
                performanceSteps.ExpectedUnaccountedForSocketAsyncEventArgs -= 3;
#endif
        }
#endif

            /// <summary>
            ///     Starts a server.
            /// </summary>
            /// <param name="serverConfig">The configuration file for the server.</param>
        [Given(@"I have a running server from ([\w\.]+)")]
        public void GivenIHaveARunningServer(string serverConfig)
        {
            NameValueCollection parameters = new NameValueCollection {
                { "port", GetFreePort().ToString() }
            };

            DarkRiftServer server = new DarkRiftServer(ServerSpawnData.CreateFromXml("Configurations/Server/" + serverConfig, parameters));
            server.StartServer();

            world.AddServer(server);

#if DEBUG
            // We've just requested a load of objects that wont be returned until we close
            // UDP receive TCP accept
            performanceSteps.ExpectedUnaccountedForSocketAsyncEventArgs += 2;
#endif
        }

        /// <summary>
        ///     Starts a server inside a cluster.
        /// </summary>
        /// <param name="serverConfig">The configuration file for the server.</param>
        /// <param name="clusterConfig">The configuration file for the cluster.</param>
        [Given(@"I have a running server from ([\w\.]+) and ([\w\.]+)")]
        [When(@"I have a running server from ([\w\.]+) and ([\w\.]+)")]
        public void GivenIHaveARunningServer(string serverConfig, string clusterConfig)
        {
            NameValueCollection parameters = new NameValueCollection {
                { "port", GetFreePort().ToString() }
            };

            DarkRiftServerConfigurationBuilder serverConfigurationBuilder = DarkRiftServerConfigurationBuilder.CreateFromXml("Configurations/Server/" + serverConfig, parameters);
#if PRO
            serverConfigurationBuilder.AddPluginType(typeof(InMemoryServerRegistryConnector));

            DarkRiftServer server = new DarkRiftServer(
                serverConfigurationBuilder.ServerSpawnData,
                ClusterSpawnData.CreateFromXml("Configurations/Cluster/" + clusterConfig, new NameValueCollection())
            );
#else
            DarkRiftServer server = new DarkRiftServer(
                serverConfigurationBuilder.ServerSpawnData
            );
#endif

            server.StartServer();

            world.AddServer(server);

#if DEBUG
            // We've just requested a load of objects that wont be returned until we close
            // UDP receive TCP accept
            performanceSteps.ExpectedUnaccountedForSocketAsyncEventArgs += 2;
#endif
        }

        /// <summary>
        ///     The specified server is using the dispatcher.
        /// </summary>
        [Given(@"server (\d+) is using the dispatcher")]
        public void GivenTheServerIsUsingTheDispatcher(ushort serverID)
        {
            world.GetServer(serverID).ThreadHelper.EventsFromDispatcher = true;

            // Create a snapshot view of the server so we have a consistent reference to it between tests
            DarkRiftServer server = world.GetServer(serverID);
            dispatcherThread = new Thread(() =>
            {
                while (!server.Disposed)
                {
                    server.DispatcherWaitHandle.WaitOne();

                    server.ExecuteDispatcherTasks();
                }
            });
            dispatcherThread.Start();
        }

        /// <summary>
        ///     Connects a given number of clients to the server.
        /// </summary>
        /// <param name="numberOfClients">The number of clients to connect.</param>
        [Given(@"(\d+) clients? connected")]
        public void GivenConnectedClients(int numberOfClients)
        {
            for (int i = 0; i < numberOfClients; i++)
            {
                DarkRiftClient client = new DarkRiftClient();
                client.Connect(
                    new BichannelClientConnection(
                        IPAddress.Loopback,
                        world.GetServer(0).ClientManager.Port,
                        world.GetServer(0).NetworkListenerManager.GetNetworkListenersByType<AbstractBichannelListener>()[0].UdpPort,
                        true
                    )
                );
                world.AddClient(client);

#if DEBUG
                // We've just requested a load of objects that wont be returned until we close
                // UDP receive TCP receive in client; TCP receive in server
                performanceSteps.ExpectedUnaccountedForSocketAsyncEventArgs += 3;
#endif
            }
        }

        /// <summary>
        ///     Connects a given number of clients to the server expecting them to fail to connect.
        /// </summary>
        /// <param name="numberOfClients">The number of clients to fail to connect.</param>
        [Given(@"(\d+) clients? that fails? to connect")]
        public void GivenClientsThatFailToConnect(int numberOfClients)
        {
            for (int i = 0; i < numberOfClients; i++)
            {
                DarkRiftClient client = new DarkRiftClient();
                try
                {
                    client.Connect(
                        new BichannelClientConnection(
                            IPAddress.Loopback,
                            4296,   // Don't want to be able to connect so any port is fine
                            true
                        )
                    );

                    Assert.Fail("Did not expect client to connect successfully.");
                }
                catch (SocketException)
                {
                    // Expected
                }

                world.AddClient(client);
            }
        }

        /// <summary>
        ///     Connects a given number of clients to the server over IPv6.
        /// </summary>
        /// <param name="numberOfClients">The number of clients to connect.</param>
        [Given(@"^(\d+) clients? connected over IPv6$")]
        public void GivenConnectedClientsOverIPv6(int numberOfClients)
        {
            for (int i = 0; i < numberOfClients; i++)
            {
                DarkRiftClient client = new DarkRiftClient();
                client.Connect(
                    new BichannelClientConnection(
                        IPAddress.IPv6Loopback,
                        world.GetServer(0).ClientManager.Port,
                        world.GetServer(0).NetworkListenerManager.GetNetworkListenersByType<AbstractBichannelListener>()[0].UdpPort,
                        true
                    )
                );

                world.AddClient(client);
#if DEBUG
                // We've just requested a load of objects that wont be returned until we close
                // UDP receive TCP receive in client; TCP receive in server
                performanceSteps.ExpectedUnaccountedForSocketAsyncEventArgs += 3;
#endif
            }
        }

        /// <summary>
        ///     Checks that all clients created so far are connected to the server.
        /// </summary>
        [Then(@"all clients should be (connected|connecting|disconnected|disconnecting|interrupted)")]
        public void AllClientsShouldBe(string expectedState)
        {
            ThenClientsShouldBe(world.GetClients().Count(), expectedState);
        }

        /// <summary>
        ///     Checks that the given number of clients are connected to the server.
        /// </summary>
        /// <param name="serverID">The ID of the server to query.</param>
        /// <param name="numberOfClients">The number of clients to check for.</param>
        [Then(@"server (\d+) should have (\d+) clients?")]
        public void ThenTheServerShouldHaveClients(ushort serverID, int numberOfClients)
        {
            WaitUtility.WaitUntil("Expected server " + serverID + " to have " + numberOfClients + " clients connected.", () =>
            {
                Assert.AreEqual(numberOfClients, world.GetServer(serverID).ClientManager.Count);
            });
        }

        /// <summary>
        ///     Disconnects the specified client.
        /// </summary>
        /// <param name="client">The client to disconnect.</param>
        [When(@"I disconnect client (\d+)")]
        public void WhenIDisconnectClient(ushort client)
        {
            world.GetClient(client).Disconnect();

#if DEBUG
            // We've just returned a load of objects that we don't normally expect
            // UDP receive TCP receive in client; TCP receive in server
            performanceSteps.ExpectedUnaccountedForSocketAsyncEventArgs -= 3;
#endif
        }

        [Then(@"(\d+) clients? should be (connected|connecting|disconnected|disconnecting|interrupted)")]
        public void ThenClientsShouldBe(int numberOfClients, string state)
        {
            ConnectionState expectedState;
            if (state == "connected")
                expectedState = ConnectionState.Connected;
            else if (state == "connecting")
                expectedState = ConnectionState.Connecting;
            else if (state == "disconnected")
                expectedState = ConnectionState.Disconnected;
            else if (state == "disconnecting")
                expectedState = ConnectionState.Disconnecting;
            else if (state == "interrupted")
                expectedState = ConnectionState.Interrupted;
            else
                throw new ArgumentException("Invalid expected state");

            int count = world.GetClients().Count(c => c.ConnectionState == expectedState);
            Assert.AreEqual(numberOfClients, count);
        }

        //TODO the below step is pointless as we access by ID
        [Then(@"client (\d+) has an ID of (\d+)")]
        public void ThenClientHasAnIDOf(ushort client, int id)
        {
            Assert.AreEqual(id, world.GetClient(client).ID);
        }

        [Then(@"I can start a new server from ([\w\.]+)")]
        public void ThenICanStartANewServerFrom(string config)
        {
            GivenIHaveARunningServer(config);
        }

#if PRO
        [Then(@"server (\d)+ should synchronise to have (\d+) servers? in (\w+)")]
        public void ThenServerShouldSynchroniseToHaveServersInGroup(ushort serverID, ushort numberOfServers, string group)
        {
            // 30 second wait as we only update every 10
            WaitUtility.WaitUntil("Incorrect number of servers present in group " + group + ".", () =>
            {
                Assert.AreEqual(numberOfServers, world.GetServer(serverID).RemoteServerManager.GetGroup(group).Count);
            }, TimeSpan.FromSeconds(30));

            WaitUtility.WaitUntil("Servers in group " + group + " are not all connected.", () =>
            {
                Assert.IsTrue(
                    world.GetServer(serverID)
                         .RemoteServerManager.GetGroup(group)
                         .GetAllRemoteServers()
                         .All(s => s.ConnectionState == ConnectionState.Connected)
                );
            });
        }
#endif

        [When(@"I close (and forget )?server (\d+)")]
        public void WhenICloseServer(string andForget, ushort server)
        {
            world.GetServer(server).Dispose();

            if (andForget != "")
                world.RemoveServer(server);

#if DEBUG
            // We've just returned a load of objects that we don't normally expect
            // UDP receive, TCP accept
            performanceSteps.ExpectedUnaccountedForSocketAsyncEventArgs -= 2;
#endif
        }

        [Then(@"the ServerConnected event has been fired (\d) times?$")]
        public void ThenTheServerConnectedEventHasBeenFired(int times)
        {
            Assert.AreEqual(times, world.ServerConnectedEvents);
        }

        [Then(@"the ServerDisconnected event has been fired (\d) times?$")]
        public void ThenTheServerDisconnectedEventHasBeenFired(int times)
        {
            Assert.AreEqual(times, world.ServerDisconnectedEvents);
        }

        [Then(@"I can close client (\d+)")]
        public void ThenICanClose(ushort client)
        {
            world.GetClient(client).Dispose();
        }

        [Given(@"a delay of (\d+)ms when a client connects before assigning message handlers")]
        public void GivenADelayWhenAClientConnectsBeforeAssigningMessageHandlers(int delayMs)
        {
            world.ClientConnectionDelay = delayMs;
        }

        /// <summary>
        ///     Returns a port that is unallocated.
        /// </summary>
        /// <returns>The port found.</returns>
        private ushort GetFreePort()
        {
            using Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            socket.Bind(new IPEndPoint(IPAddress.Any, 0));
            return (ushort)((IPEndPoint)socket.LocalEndPoint).Port;
        }
    }
}
