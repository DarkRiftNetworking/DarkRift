/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Server.Plugins.Listeners.Bichannel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TechTalk.SpecFlow;

namespace DarkRift.SystemTesting
{
    /// <summary>
    /// Steps for sending partial messages and gettting finer control.
    /// </summary>
    [Binding]
    public class PartialMessagingSteps
    {
        /// <summary>
        /// The TCP socket.
        /// </summary>
        private Socket tcpSocket;

        /// <summary>
        /// The UDP socket.
        /// </summary>
        private Socket udpSocket;

        /// <summary>
        /// The world to store state in.
        /// </summary>
        private readonly World world;

        /// <summary>
        /// Way of asserting messages.
        /// </summary>
        private readonly MessageAssertions messageAssertions;

        public PartialMessagingSteps(World world, MessageAssertions messageAssertions)
        {
            this.world = world;
            this.messageAssertions = messageAssertions;
        }

        /// <summary>
        /// Connects raw socket to the server.
        /// </summary>
        [Given(@"TCP and UDP sockets connected")]
        public void GivenTCPSocketConnected()
        {
            tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpSocket.Connect(new IPEndPoint(IPAddress.Loopback, world.GetServer(0).ClientManager.Port));

            udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.Bind(new IPEndPoint(((IPEndPoint)tcpSocket.LocalEndPoint).Address, 0));
            udpSocket.Connect(new IPEndPoint(IPAddress.Loopback, world.GetServer(0).NetworkListenerManager.GetNetworkListenersByType<AbstractBichannelListener>()[0].UdpPort));
        }

        /// <summary>
        /// Completes the DR bichannel handshake down this socket.
        /// </summary>
        [Given(@"the handshake has completed")]
        public void GivenTheHandshakeHasCompeleted()
        {
            // Receive token
            byte[] buffer = new byte[9];
            int receivedTcp = tcpSocket.Receive(buffer);

            Assert.AreEqual(9, receivedTcp);
            Assert.AreEqual(0, buffer[0]);

            // Return token
            udpSocket.Send(buffer);

            // Receive punchthrough
            byte[] buffer2 = new byte[1];
            int receivedUdp = udpSocket.Receive(buffer);

            Assert.AreEqual(1, receivedUdp);
            Assert.AreEqual(0, buffer2[0]);

            // Stupid race condition to attach the MessageReceived handler
            System.Threading.Thread.Sleep(100);
        }

        /// <summary>
        /// Enables NoDelay on the TCP socket.
        /// </summary>
        [Given(@"no delay is enabled")]
        public void GivenNoDelayIsEnabled()
        {
            tcpSocket.NoDelay = true;
        }

        /// <summary>
        /// Sends bytes down the TCP socket.
        /// </summary>
        [When(@"bytes are sent via TCP (.+)")]
        public void WhenBytesAreSentViaTcp(string byteLine)
        {
            tcpSocket.Send(byteLine.Split(", ").Select(b => byte.Parse(b)).ToArray());
        }

        /// <summary>
        /// Checks the TCP socket is connected.
        /// </summary>
        [Then(@"the TCP socket is connected")]
        public void ThenTheTcpSocketIsConnected()
        {
            Assert.IsTrue(tcpSocket.Connected);
        }

        /// <summary>
        /// Checks string received in a message on the server.
        /// </summary>
        [Then(@"I receive string on the server from TCP '(.+)'")]
        public void ThenIReceiveStringOnTheServerFromTCP(string text)
        {
            messageAssertions.ExpectMessageOnServer(new ReceivedMessage(text, 0, 0, 0, SendMode.Reliable));
            messageAssertions.ThenAllMessagesAreAccountedFor();
        }
    }
}
