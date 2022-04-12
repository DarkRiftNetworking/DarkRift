/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkRift.Server.Plugins.Commands
{
    /// <summary>
    ///     Helper plugin for sending messages using commands.
    /// </summary>
    internal class MessageCommand : Plugin
    {
        public override bool ThreadSafe => true;

        public override Version Version => new Version(1, 0, 0);

        public override Command[] Commands => new Command[]
        {
            new Command("message", "Sends messages for testing the client.", "message <client> <sendMode> <tag> <data> <data> <data> ...", CommandHandler)
        };

        internal override bool Hidden => true;

        public MessageCommand(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
        }

        private void CommandHandler(object sender, CommandEventArgs e)
        {
            if (e.Arguments.Length < 3)
                throw new CommandSyntaxException($"Expected 3 arguments but found {e.Arguments.Length}.");

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                ushort clientID;
                try
                {
                    clientID = ushort.Parse(e.Arguments[0]);
                }
                catch (FormatException)
                {
                    throw new CommandSyntaxException($"Unable to parse the client ID. Expected a number but got '{e.Arguments[0]}'.");
                }

                SendMode sendMode;
                switch (e.Arguments[1].ToLower())
                {
                    case "unreliable":
                    case "u":
                        sendMode = SendMode.Unreliable;
                        break;

                    case "reliable":
                    case "r":
                        sendMode = SendMode.Reliable;
                        break;

                    default:
                        throw new CommandSyntaxException($"Expected 'unreliable' or 'reliable' but got '{e.Arguments[1]}'.");
                }

                ushort tag;
                try
                {
                    tag = ushort.Parse(e.Arguments[2]);
                }
                catch (FormatException)
                {
                    throw new CommandSyntaxException($"Unable to parse the tag. Expected a number but got '{e.Arguments[2]}'.");
                }

                try {
                    IEnumerable<byte> bytes =
                        e.Arguments
                            .Skip(3)
                            .Select((a) => byte.Parse(a));

                    foreach (byte b in bytes)
                        writer.Write(b);
                }
                catch (FormatException)
                {
                    throw new CommandSyntaxException("An argument was unable to be parsed to a number.");
                }

                using (Message message = Message.Create(tag, writer))
                {
                    try
                    {
                        ClientManager[clientID].SendMessage(message, sendMode);
                    }
                    catch (KeyNotFoundException)
                    {
                        Logger.Error("No client with id " + clientID);
                    }
                }
            }
        }
    }
}
