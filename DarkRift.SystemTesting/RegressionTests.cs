using DarkRift.Client;
using DarkRift.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading;
using static DarkRift.Client.DarkRiftClient;

namespace DarkRift.SystemTesting
{
    [TestClass]
    public class RegressionTests
    {
#pragma warning disable S1123 // "Obsolete" attributes should include explanations
        [Obsolete]
#pragma warning restore S1123 // "Obsolete" attributes should include explanations
        [TestMethod]
        public void TestConnectionSpamSecurityIssue20220802()
        {
            const int NumConnections = 1111;
            var ip = IPAddress.Parse("127.0.0.1");

            DateTime start = DateTime.Now;

            int actualClientsConnected = 0;
            void ClientManager_ClientConnected(object sender, ClientConnectedEventArgs e)
            {
                start = DateTime.Now;
                Interlocked.Increment(ref actualClientsConnected);
            }

            DarkRiftServer server = new DarkRiftServer(ServerSpawnData.CreateFromXml("Configurations/Server/SpammedServer.config", new NameValueCollection()));
            server.ThreadHelper.EventsFromDispatcher = true;
            server.ClientManager.ClientConnected += ClientManager_ClientConnected;
            server.StartServer();
            int port = server.NetworkListenerManager.GetNetworkListeners().First().Port;

            int actualConnectionsCompleted = 0;
            int actualConnectionsSuccessful = 0;

            for (int x = 0; x < NumConnections; x++)
            {
                ConnectInBackground(ip, port, exception =>
                {
                    start = DateTime.Now;
                    Interlocked.Increment(ref actualConnectionsCompleted);

                    if (exception == null)
                    {
                        Interlocked.Increment(ref actualConnectionsSuccessful);
                    }
                    else
                    {
                        Console.WriteLine(exception.Message);
                    }
                });
            }

            while (actualConnectionsCompleted < NumConnections
                || actualClientsConnected < NumConnections
                || server.Dispatcher.Count > 0)
            {
                if (DateTime.Now - start > TimeSpan.FromSeconds(10))
                    Assert.Fail($"Timed out {actualConnectionsCompleted}/{actualClientsConnected}/{actualConnectionsSuccessful}");

                if (server.ThreadHelper.EventsFromDispatcher)
                    server.ExecuteDispatcherTasks();

                Thread.Sleep(1);
            }

            Assert.AreEqual(NumConnections, actualConnectionsCompleted);
            Assert.AreEqual(NumConnections, actualClientsConnected);
            Assert.AreEqual(NumConnections, actualConnectionsSuccessful);
        }

#pragma warning disable S1123 // "Obsolete" attributes should include explanations
        [Obsolete]
#pragma warning restore S1123 // "Obsolete" attributes should include explanations
        private void ConnectInBackground(IPAddress ip, int port, ConnectCompleteHandler callback = null)
        {
            var connection = new BichannelClientConnection(IPVersion.IPv4, ip, port, true);

            new Thread((ThreadStart)delegate
            {
                try
                {
                    connection.Connect();
                }
                catch (Exception e)
                {
                    callback?.Invoke(e);
                    return;
                }

                callback?.Invoke(null);
            }).Start();
        }
    }
}
