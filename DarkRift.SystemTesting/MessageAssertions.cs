/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace DarkRift.SystemTesting
{
    [Binding]
    public class MessageAssertions
    {
        /// <summary>
        ///     The messages received by the server so far.
        /// </summary>
        private ConcurrentQueue<ReceivedMessage> messagesToServer;

        /// <summary>
        ///     The messages received by the clients so far.
        /// </summary>
        private ConcurrentQueue<ReceivedMessage> messagesToClients;

        /// <summary>
        ///     The messages expected to be received by the server.
        /// </summary>
        private ConcurrentBag<ReceivedMessage> expectedToServer;

        /// <summary>
        ///     The messages expected to be received by the clients.
        /// </summary>
        private ConcurrentBag<ReceivedMessage> expectedToClients;

        /// <summary>
        ///     Sets up the assertions.
        /// </summary>
        [BeforeScenario]
        public void BeforeScenario()
        {
            messagesToServer = new ConcurrentQueue<ReceivedMessage>();
            messagesToClients = new ConcurrentQueue<ReceivedMessage>();

            expectedToServer = new ConcurrentBag<ReceivedMessage>();
            expectedToClients = new ConcurrentBag<ReceivedMessage>();
        }

        /// <summary>
        ///     Adds a message expected to be received on the client.
        /// </summary>
        /// <param name="message">The message expected to be recieved.</param>
        public void ExpectMessageOnClient(ReceivedMessage message)
        {
            expectedToClients.Add(message);
        }

        /// <summary>
        ///     Adds a message expected to be received on the server.
        /// </summary>
        /// <param name="message">The message expected to be recieved.</param>
        public void ExpectMessageOnServer(ReceivedMessage message)
        {
            expectedToServer.Add(message);
        }

        /// <summary>
        ///     Adds a message received on the client.
        /// </summary>
        /// <param name="message">The message recieved.</param>
        public void AddMessageOnClient(ReceivedMessage message)
        {
            messagesToClients.Enqueue(message);
        }

        /// <summary>
        ///     Adds a message received on the server.
        /// </summary>
        /// <param name="message">The message recieved.</param>
        public void AddMessageOnServer(ReceivedMessage message)
        {
            messagesToServer.Enqueue(message);
        }

        /// <summary>
        ///     Asserts that all messages exected to be received from other steps have been received, and no more.
        /// </summary>
        [Then(@"all messages are accounted for")]
        public void ThenAllMessagesAreAccountedFor()
        {
            WaitUtility.WaitUntil("Not all messages received by the server in the given time.",
                () => Assert.AreEqual(0, expectedToServer.Except(messagesToServer).Count()),
                TimeSpan.FromMinutes(1));
            WaitUtility.WaitUntil("Not all messages received by the clients in the given time.",
                () => Assert.AreEqual(0, expectedToClients.Except(messagesToClients).Count()),
                TimeSpan.FromMinutes(1));

            Assert.AreEqual(0, messagesToServer.Except(expectedToServer).Count(), "Additional, unexpected messages received by the server.");
            Assert.AreEqual(0, messagesToClients.Except(expectedToClients).Count(), "Additional, unexpected messages received by the clients.");
        }

        /// <summary>
        ///     Waits for the specified client to have recieved the specified number of messages.
        /// </summary>
        /// <param name="client">The client to wait on.</param>
        /// <param name="numberOfMessages">The number of messages to wait for.</param>
        [When(@"client (\d+) has received (\d+) messages?")]
        public void WhenTheClientHasReceivedMessages(ushort client, int numberOfMessages)
        {
            WaitUtility.WaitUntil($"Not enough messages were received on the client within the time limit. Expected at least <{numberOfMessages}> Actual <{messagesToClients.Where(m => m.Source == client).Count()}>",
                () => messagesToClients.Where(m => m.Destination == client).Count() >= numberOfMessages);
        }

        /// <summary>
        ///     Waits for the server to receive the given number of messages.
        /// </summary>
        /// <param name="server">The sever to wait on.</param>
        /// <param name="numberOfMessages">The number of messages to wait for.</param>
        [When(@"^server (\d+) has received (\d+) messages?$")]
        public void WhenTheServerHasReceivedMessage(ushort server, int numberOfMessages)
        {
            WaitUtility.WaitUntil($"Not enough messages were received on the server within the time limit. Expected as least <{numberOfMessages}> Actual <{messagesToServer.Where(m => m.Destination == server).Count()}>",
                () => messagesToServer.Where(m => m.Destination == server).Count() >= numberOfMessages);
        }
    }
}
