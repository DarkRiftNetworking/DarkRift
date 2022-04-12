/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DarkRift.Server.Plugins.Commands
{
    internal class ClientCommand : Plugin
    {
        public override bool ThreadSafe => true;

        public override Version Version => new Version(1, 0, 0);

        public override Command[] Commands => new Command[]
        {
            new Command("client", "Creates mock clients for testing.", "client add (-ip=<ip>) (-port|p=<port>) (-h)\nclient remove <id>", ClientCommandHandler)
        };

        internal override bool Hidden => true;

        /// <summary>
        ///     The current mock clients.
        /// </summary>
        private readonly Dictionary<int, MockedNetworkServerConnection> connections = new Dictionary<int, MockedNetworkServerConnection>();

        public ClientCommand(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {

        }
        
        private void ClientCommandHandler(object sender, CommandEventArgs e)
        {
            if (e.Arguments.Length < 1)
                throw new CommandSyntaxException();

            if (e.Arguments[0].ToLower() == "add")
            {
                if (e.Arguments.Length != 1)
                    throw new CommandSyntaxException($"Expected 1 argument to client command but found {e.Arguments.Length}.");

                IPAddress ip;
                if (e.Flags["ip"] != null)
                {
                    if (!IPAddress.TryParse(e.Flags["ip"], out ip))
                        throw new CommandSyntaxException("Could not parse the IP address of the client to create.");
                }
                else
                {
                    ip = IPAddress.Loopback;
                }

                ushort port;
                if (e.Flags["port"] != null)
                {
                    if (!ushort.TryParse(e.Flags["port"], out port))
                        throw new CommandSyntaxException("Could not parse the port of the client to create.");
                }
                else if (e.Flags["p"] != null)
                {
                    if (!ushort.TryParse(e.Flags["p"], out port))
                        throw new CommandSyntaxException("Could not parse the port of the client to create.");
                }
                else
                {
                    port = 0;
                }

                bool outputData = e.HasFlag("h");

                MockedNetworkServerConnection connection = new MockedNetworkServerConnection(this, ip, port, outputData);
                Server.InternalClientManager.HandleNewConnection(connection);
                connections.Add(connection.Client.ID, connection);
            }
            else if (e.Arguments[0].ToLower() == "remove")
            {
                if (e.Arguments.Length != 2)
                    throw new CommandSyntaxException($"Expected 2 arguments to client command but found {e.Arguments.Length}.");

                if (!int.TryParse(e.Arguments[1], out int id))
                    throw new CommandSyntaxException("Could not parse the ID of the client to disconnect.");

                if (!connections.TryGetValue(id, out MockedNetworkServerConnection connection))
                    throw new CommandSyntaxException("You can only disconnect clients previously created with the client command.");

                Server.InternalClientManager.HandleDisconnection(connection.Client, true, SocketError.Success, null);
            }
            else
            {
                throw new CommandSyntaxException("Invalid argument '" + e.Arguments[0] + "'");
            }
        }

        /// <summary>
        ///     Handles a mock connection being disconnected.
        /// </summary>
        internal void HandleDisconnection(MockedNetworkServerConnection connection)
        {
            connections.Remove(connection.Client.ID);
        }

        /// <summary>
        ///     Handles sending to a mock connection.
        /// </summary>
        internal void HandleSend(MockedNetworkServerConnection connection, SendMode sendMode, MessageBuffer message, bool outputData)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(sendMode == SendMode.Reliable ? "Reliable" : "Unreliable");
            builder.Append(" message sent to ");
            builder.Append(connection.Client.ID);

            if (outputData)
            {
                for (int i = 0; i < message.Count; i++)
                {
                    builder.Append(" ");
                    builder.Append(message.Buffer[message.Offset + i].ToString("X2"));

                    if (i % 4 == 3)
                        builder.Append(" ");
                }
            }

            Logger.Info(builder.ToString());
        }
    }
}
