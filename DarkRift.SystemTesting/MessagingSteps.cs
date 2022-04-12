/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using DarkRift.Client;
using DarkRift.Server;
using DarkRift.Server.Plugins.Listeners.Bichannel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace DarkRift.SystemTesting
{
    /// <summary>
    ///     Steps for basic message sending/receiveing.
    /// </summary>
    [Binding]
    public class MessagingSteps
    {
        /// <summary>
        ///     The world to store state in.
        /// </summary>
        private readonly World world;

        /// <summary>
        ///     The delay the server should acknowledge ping messages after, or -1 if not.
        /// </summary>
        private int serverAcknowledgesPingsAfter = -1;

        /// <summary>
        ///     Class for asserting messages received.
        /// </summary>
        private readonly MessageAssertions messageAssertions;


        /// <summary>
        ///     The performance related test steps.
        /// </summary>
        private readonly PerformanceSteps performanceSteps;

        public MessagingSteps(World world, MessageAssertions messageAssertions, PerformanceSteps performanceSteps)
        {
            this.world = world;
            this.messageAssertions = messageAssertions;
            this.performanceSteps = performanceSteps;

            world.ServerMessageReceived += ServerMessageReceived;
        }

        private void ServerMessageReceived(object sender, Server.MessageReceivedEventArgs args)
        {
            // Make sure any ping messages get acknowledged
            using Message message = args.GetMessage();
            if (serverAcknowledgesPingsAfter != -1 && message.IsPingMessage)
            {
                using Message acknowledgment = Message.CreateEmpty(message.Tag);
                acknowledgment.MakePingAcknowledgementMessage(message);

                Thread.Sleep(serverAcknowledgesPingsAfter);

                bool success = args.Client.SendMessage(acknowledgment, args.SendMode);
                Assert.IsTrue(success);
                messageAssertions.ExpectMessageOnClient(new ReceivedMessage(null, ushort.MaxValue, args.Client.ID, message.Tag, args.SendMode));
            }
        }

        /// <summary>
        ///     Sends a message from a client.
        /// </summary>
        /// <param name="client">The client to send from.</param>
        /// <param name="str">The message to send as a string.</param>
        /// <param name="tag">The tag to send the message with.</param>
        [When(@"client (\d+) sends '([\w\s]+)' with tag (\d+) (reliably|unreliably)( as a ping)?")]
        public void WhenClientSendsWithTag(ushort client, string str, ushort tag, string mode, string isPing)
        {
            SendMode sendMode = mode == "reliably" ? SendMode.Reliable : SendMode.Unreliable;

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(str);
                using Message message = Message.Create(tag, writer);
                if (!string.IsNullOrEmpty(isPing))
                    message.MakePingMessage();

                bool success = this.world.GetClient(client).SendMessage(message, sendMode);
                Assert.IsTrue(success);
            }

            messageAssertions.ExpectMessageOnServer(new ReceivedMessage(str, client, 0, tag, sendMode));
        }

        /// <summary>
        ///     Sends a message from a client with a specific length.
        /// </summary>
        /// <param name="client">The client to send from.</param>
        /// <param name="length">The number of characters to send.</param>
        /// <param name="tag">The tag to send the message with.</param>
        [When(@"^client (\d+) sends (\d+) characters with tag (\d+) (reliably|unreliably)( as a ping)?$")]
        public void WhenClientSendsBytesWithTag(ushort client, int length, ushort tag, string mode, string isPing)
        {
            SendMode sendMode = mode == "reliably" ? SendMode.Reliable : SendMode.Unreliable;
            string str = new string('*', length);

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(str);
                using Message message = Message.Create(tag, writer);
                if (!string.IsNullOrEmpty(isPing))
                    message.MakePingMessage();

                this.world.GetClient(client).SendMessage(message, sendMode);
            }

            messageAssertions.ExpectMessageOnServer(new ReceivedMessage(str, client, 0, tag, sendMode));
        }

        /// <summary>
        ///     Sends a message from the server to a client.
        /// </summary>
        /// <param name="serverID">The ID of the server to send from.</param>
        /// <param name="str">The string to send the client.</param>
        /// <param name="client">The client to send to.</param>
        /// <param name="tag">The tag to send the message with.</param>
        /// <param name="mode">The send mode to send the message via.</param>
        [When(@"server (\d+) sends '([\w\s]+)' to client (\d+) with tag (\d+) (reliably|unreliably)( as a ping)?")]
        public void WhenTheServerSendsToClientWithTag(ushort serverID, string str, ushort client, ushort tag, string mode, string isPing)
        {
            SendMode sendMode = mode == "reliably" ? SendMode.Reliable : SendMode.Unreliable;

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(str);
                using Message message = Message.Create(tag, writer);
                if (!string.IsNullOrEmpty(isPing))
                    message.MakePingMessage();

                bool success = this.world.GetServer(serverID).ClientManager[client].SendMessage(message, sendMode);
                Assert.IsTrue(success);
            }

            messageAssertions.ExpectMessageOnClient(new ReceivedMessage(str, ushort.MaxValue, client, tag, sendMode));
        }

        /// <summary>
        ///     Sends a message from a server to another server. 
        /// </summary>
        /// <param name="server">The server to send from.</param>
        /// <param name="str">The message to send as a string.</param>
        /// <param name="remoteServer">The server to send to.</param>
        /// <param name="group">The group of the server to send to.</param>     //TODO We should be able to send without this?
        /// <param name="tag">The tag to send the message with.</param>
        [When(@"server (\d+) sends '([\w\s]+)' to server (\d+) in (.+) with tag (\d+) (reliably|unreliably)")]
        public void WhenServerSendsToServerInWithTag(ushort server, string str, ushort remoteServer, string group, ushort tag, string mode)
        {
#if PRO
            SendMode sendMode = mode == "reliably" ? SendMode.Reliable : SendMode.Unreliable;

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(str);
                using Message message = Message.Create(tag, writer);
                bool success = this.world.GetServer(server).RemoteServerManager.GetGroup(group).GetRemoteServer(remoteServer).SendMessage(message, sendMode);
                Assert.IsTrue(success);

            }

            messageAssertions.ExpectMessageOnServer(new ReceivedMessage(str, server, remoteServer, tag, sendMode));
#endif
        }

        /// <summary>
        ///     Instructs the server to send a ping acknowledgment for any ping messages received.
        /// </summary>
        [Given(@"the server acknowledges ping messages after ([0-9]+)ms")]
        public void GivenTheServerAcknowledgesPingMessages(int delay)
        {
            serverAcknowledgesPingsAfter = delay;
        }
        
        /// <summary>
        ///     Instructs the specified client to send a ping acknowledgment for any ping messages received.
        /// </summary>
        /// <param name="client">The client to setup for.</param>
        [Given(@"client ([0-9]+) acknowledges ping messages after ([0-9]+)ms")]
        public void GivenClientAcknowledgesPingMessages(ushort client, int delay)
        {
            world.GetClient(client).MessageReceived += (sender, args) =>
            {
                using Message message = args.GetMessage();
                if (message.IsPingMessage)
                {
                    using Message acknowledgment = message.Clone();
                    acknowledgment.MakePingAcknowledgementMessage(message);

                    Thread.Sleep(delay);

                    string str;
                    using (DarkRiftReader reader = message.GetReader())
                        str = reader.ReadString();

                    bool success = ((DarkRiftClient)sender).SendMessage(acknowledgment, args.SendMode);
                    Assert.IsTrue(success);
                    messageAssertions.ExpectMessageOnServer(new ReceivedMessage(str, 0, client, message.Tag, args.SendMode));
                }
            };
        }

        /// <summary>
        ///     Checks that the server has a correct ping to the specified client.
        /// </summary>
        /// <param name="serverID">The server to query.</param>
        /// <param name="expectedPing">The expected ping.</param>
        /// <param name="client">The client to check against.</param>
        [Then(@"server (\d+) has a ping of around ([0-9]+)ms to client ([0-9]+)")]
        public void ThenServerHasAPingOfAroundToClient(ushort serverID, float expectedPing, ushort client)
        {
            float actualPing = world.GetServer(serverID).ClientManager.GetClient(client).RoundTripTime.LatestRtt * 1000;
            float delta = expectedPing / 2;

            Assert.AreEqual(expectedPing, actualPing, delta, $"Expected a ping of {expectedPing}ms +/- {delta}ms but was actually {Math.Round(actualPing)}ms.");
        }

        /// <summary>
        ///     Checks that the given client has a correct ping to the server.
        /// </summary>
        /// <param name="expectedPing">The expected ping.</param>
        /// <param name="client">The client to query.</param>
        [Then(@"client ([0-9]+) has a ping of around ([0-9]+)ms to the server")]
        public void ThenClientHasAPingOfAroundToTheServer(ushort client, float expectedPing)
        {
            float actualPing = world.GetClient(client).RoundTripTime.LatestRtt * 1000;
            float delta = expectedPing / 2;

            Assert.AreEqual(expectedPing, actualPing, delta, $"Expected a ping of {expectedPing}ms +/- {delta}ms but was actually {Math.Round(actualPing)}ms.");
        }

        /// <summary>
        ///     Sends a number of random messages from both server and client all simultaniously.
        /// </summary>
        /// <param name="count">The number of messages to send.</param>
        [When(@"I stress test (server to client|client to server|both) with (\d+) per client")]
        public void WhenIStressTestWithPerClient(string direction, int count)
        {
            List<Thread> threads = new List<Thread>();

            // Send from clients
            if (direction == "client to server" || direction == "both")
            {
                foreach (DarkRiftClient client in world.GetClients())
                {
                    threads.Add(new Thread(() =>
                    {
                        Random random = new Random();

                        for (int i = 0; i < count; i++)
                        {
                            string value = random.NextDouble().ToString();
                            ushort tag = (ushort)random.Next(65536);
                            //SendMode sendMode = random.Next(2) == 0 ? SendMode.Reliable : SendMode.Unreliable;
                            SendMode sendMode = SendMode.Reliable;      //TODO test unreliable but with less data

                            using DarkRiftWriter writer = DarkRiftWriter.Create();
                            writer.Write(value);

                            using Message message = Message.Create(tag, writer);
                            bool success = client.SendMessage(message, sendMode);
                            Assert.IsTrue(success);
                            messageAssertions.ExpectMessageOnServer(new ReceivedMessage(value, client.ID, 0, tag, sendMode));
                        }
                    }));
                }
            }

            // Send from server
            if (direction == "server to client" || direction == "both")
            {
                foreach (IClient client in world.GetServer(0).ClientManager.GetAllClients())
                {
                    threads.Add(new Thread(() =>
                    {
                        Random random = new Random();

                        for (int i = 0; i < count; i++)
                        {
                            string value = random.NextDouble().ToString();
                            ushort tag = (ushort)random.Next(65536);
                            //SendMode sendMode = random.Next(2) == 0 ? SendMode.Reliable : SendMode.Unreliable;
                            SendMode sendMode = SendMode.Reliable;      //TODO test unreliable but with less data

                            using DarkRiftWriter writer = DarkRiftWriter.Create();
                            writer.Write(value);

                            using Message message = Message.Create(tag, writer);
                            bool success = client.SendMessage(message, sendMode);
                            Assert.IsTrue(success);
                            messageAssertions.ExpectMessageOnClient(new ReceivedMessage(value, ushort.MaxValue, client.ID, tag, sendMode));
                        }
                    }));
                }
            }

            // Start all the threads
            foreach (Thread thread in threads)
                thread.Start();
            
            // Join all the threads
            foreach (Thread thread in threads)
                thread.Join();
        }
        
        /// <summary>
        ///     Sends lots of reliable data in reproduction of issue #75.
        /// </summary>
        [When(@"I send (\d+) messages reliably")]
        public void WhenISendMessagesReliably(int noMessages)
        {
            for (int i = 0; i < noMessages; i++)
            {
                using DarkRiftWriter writer = DarkRiftWriter.Create();
                ushort tag = (ushort)i;
                string value = i.ToString();
                for (int j = 0; j < 30; j++)
                    writer.Write(value);

                using Message message = Message.Create(tag, writer);
                bool success = world.GetClient(0).SendMessage(message, SendMode.Reliable);
                Assert.IsTrue(success);

                messageAssertions.ExpectMessageOnServer(new ReceivedMessage(value, 0, 0, tag, SendMode.Reliable));
            }
        }

        /// <summary>
        ///     Connects client to the server and instantly sends a message.
        /// </summary>
        /// <param name="numberOfClients">The number of clients to connect.</param>
        [When(@"a client connects and immediately sends a message")]
        public void WhenAClientConnectsAndImmedtiatelySendsAMessages()
        {
            DarkRiftClient client = new DarkRiftClient();
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write("Hello");
                using Message message = Message.Create(0, writer);

                client.Connect(
                    new BichannelClientConnection(
                        IPAddress.Loopback,
                        world.GetServer(0).ClientManager.Port,
                        world.GetServer(0).NetworkListenerManager.GetNetworkListenersByType<AbstractBichannelListener>()[0].UdpPort,
                        true
                    )
                );

                bool success = client.SendMessage(message, SendMode.Reliable);
                Assert.IsTrue(success);
            }


            world.AddClient(client);

#if DEBUG
            // We've just requested a load of objects that wont be returned until we close
            // UDP receive TCP receive in client; TCP receive in server
            performanceSteps.ExpectedUnaccountedForSocketAsyncEventArgs += 3;
#endif

            messageAssertions.ExpectMessageOnServer(new ReceivedMessage("Hello", client.ID, 0, 0, SendMode.Reliable));
        }
    }
}
