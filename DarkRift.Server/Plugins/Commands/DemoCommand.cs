/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace DarkRift.Server.Plugins.Commands
{
    /// <summary>
    ///     Demo command for showing off basic server routing in tutorials.
    /// </summary>
    internal class DemoCommand : Plugin
    {
        public override Version Version => new Version(1, 0, 0);

        public override Command[] Commands => new Command[]
        {
            new Command("demo", "Redirects all traffic to all other clients for demonstration purposes.", "demo", CommandHandler)
        };

        public override bool ThreadSafe => true;

        internal override bool Hidden => true;

        private volatile bool demo = false;

        public DemoCommand(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            
        }

        private void CommandHandler(object sender, CommandEventArgs e)
        {
            demo = !demo;

            if (demo)
            {
                ClientManager.ClientConnected += ClientManager_ClientConnected;

                foreach (Client client in ClientManager.GetAllClients())
                    client.MessageReceived += Client_MessageReceived;

                Logger.Info("Enabled demonstration mode.\n\nAll messages received by the server will now be broadcast out to all clients.\n\nThis should not be use in production code.");
            }
            else
            {
                ClientManager.ClientConnected -= ClientManager_ClientConnected;

                foreach (Client client in ClientManager.GetAllClients())
                    client.MessageReceived -= Client_MessageReceived;

                Logger.Info("Disabled demonstration mode.");
            }
        }

        private void ClientManager_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            e.Client.MessageReceived += Client_MessageReceived;
        }

        private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (demo)
            {
                using (Message message = e.GetMessage())
                {
                    foreach (Client client in ClientManager.GetAllClients())
                        client.SendMessage(message, e.SendMode);
                }
            }
        }
    }
}
