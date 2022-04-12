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
    ///     Help command!
    /// </summary>
    // TODO might be good to add a "help" (no args) which links you to docs etc.
    internal sealed class HelpCommand : Plugin
    {
        public override Version Version => new Version(1, 0, 0);

        public override bool ThreadSafe => true;

        internal override bool Hidden => true;

        public override Command[] Commands => new Command[]
        {
            new Command("help", "Retrieves the documentation for a specified command.", "help <command>\nhelp -l|-l=<plugin-name>", Help)
        };

        public HelpCommand(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
        }

        private void Help(object sender, CommandEventArgs e)
        {
            if (e.Arguments.Length == 1)
            {
                Command command;
                try
                {
                    command = Server.CommandEngine.FindCommand(e.Arguments[0]);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception.Message);

                    return;
                }

                Logger.Info($"Command: \"{command.Name}\"\n\nUsage:\n{command.Usage}\n\nDescription:\n{command.Description}");
            }
            else if (e.HasFlag("l"))
            {
                if (e.Flags["l"] != null)
                {
                    IEnumerable<Command> commands;
                    try
                    {
                        commands = Server.CommandEngine.GetCommands(e.Flags["l"]);
                    }
                    catch (KeyNotFoundException)
                    {
                        Logger.Error("No plugins found with the name '" + e.Flags["l"] + "'");

                        return;
                    }

                    Logger.Info(commands.Count() == 0 ? "This plugin has no commands." : ("Available commands: " + string.Join(", ", commands.Select(c => c.Name).ToArray())));
                }
                else
                {
                    IEnumerable<Command> commands = Server.CommandEngine.GetCommands();

                    Logger.Info(commands.Count() == 0 ? "This server has no commands." : ("Available commands: " + string.Join(", ", commands.Select(c => c.Name).ToArray())));
                }
            }
            else
            {
                throw new CommandSyntaxException();
            }
        }
    }
}
