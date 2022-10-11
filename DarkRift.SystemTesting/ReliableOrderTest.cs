using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml.Schema;
using DarkRift.Client;
using DarkRift.Server;
using DarkRift.Server.Plugins.Listeners.Bichannel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DarkRift.SystemTesting
{
    [TestClass]
    public class ReliableOrderTest
    {
        [TestMethod]
        public void SendBunchOfMessagesAndExpectOrderedArrivalAtClient()
        {
            void SetupReceiver(DarkRiftClient client, DarkRiftServer server, Action<Message> handler)
            {
                void MessageReceived(object sender, Client.MessageReceivedEventArgs e)
                {
                    using var message = e.GetMessage();
                    handler(message);
                }

                client.MessageReceived += MessageReceived;
            }

            void Send(DarkRiftClient client, DarkRiftServer server, Message message)
            {
                server.ClientManager.GetAllClients()[0].SendMessage(message, SendMode.Reliable);
            }

            SendBunchOfMessagesToAndExpectOrderedArrival(SetupReceiver, Send);
        }

        [TestMethod]
        public void SendBunchOfMessagesAndExpectOrderedArrivalAtServer()
        {
            void SetupReceiver(DarkRiftClient client, DarkRiftServer server, Action<Message> handler)
            {
                void ClientConnected(object sender, ClientConnectedEventArgs e)
                {
                    void MessageReceived(object sender, Server.MessageReceivedEventArgs e)
                    {
                        using var message = e.GetMessage();
                        handler(message);
                    }

                    e.Client.MessageReceived += MessageReceived;
                }

                server.ClientManager.ClientConnected += ClientConnected;
            }

            void Send(DarkRiftClient client, DarkRiftServer server, Message message)
            {
                client.SendMessage(message, SendMode.Reliable);
            }

            SendBunchOfMessagesToAndExpectOrderedArrival(SetupReceiver, Send);
        }

        private void SendBunchOfMessagesToAndExpectOrderedArrival(Action<DarkRiftClient, DarkRiftServer, Action<Message>> setupReceiver, Action<DarkRiftClient, DarkRiftServer, Message> send)
        {
            var spawnData = ServerSpawnData.CreateFromXml("Configurations/Server/Server.config", new NameValueCollection());
            spawnData.EventsFromDispatcher = false;

            using var client = new DarkRiftClient();
            using var server = new DarkRiftServer(spawnData);

            const int NoDiff = -1;

            int receiveCount = 0;
            int sendCount = 0;
            int diffAt = NoDiff;

            void MessageReceived(Message message)
            {
                using var reader = message.GetReader();

                int receivedNumber = reader.ReadInt32();
                int expectedReceive = receiveCount;

                if (expectedReceive != receivedNumber && diffAt == NoDiff)
                {
                    //can't do asserts on other thread so defer to main thread
                    diffAt = expectedReceive;
                }

                Interlocked.Increment(ref receiveCount);
            }

            setupReceiver(client, server, MessageReceived);

            server.StartServer();
            var listener = (AbstractBichannelListener)server.NetworkListenerManager.GetAllNetworkListeners().First();
            int port = listener.Port;
            int udpPort = listener.UdpPort;
            client.Connect(IPAddress.Parse("127.0.0.1"), port, udpPort, true);

            Assert.AreEqual(ConnectionState.Connected, client.ConnectionState);
            Assert.AreEqual(1, server.ClientManager.Count);

            for (int i = 0; i < 100000; ++i)
            {
                using var writer = DarkRiftWriter.Create(40);

                writer.Write(sendCount);
                Interlocked.Increment(ref sendCount);

                using var message = Message.Create(0, writer);

                send(client, server, message);
            }

            Thread.Sleep(2000); //give receiver time to catch up

            Assert.AreEqual(sendCount, receiveCount, "Not all messages sent!");
            Assert.AreEqual(NoDiff, diffAt, "Received in different order");
        }
    }
}
