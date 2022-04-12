/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace DarkRift.Server.Plugins.Commands
{
    /// <summary>
    ///     Plugin management plugin.
    /// </summary>
    internal class PluginController : Plugin
    {
        public override Version Version => new Version(1, 0, 0);

        public override Command[] Commands => new Command[]
        {
            new Command("plugins", "Allows management of installed plugins.", "plugins uninstall <plugin-name>\nplugins loaded (-h)\n\tAdding option -h will include hidden plugins.\nplugins installed", CommandHandler)
        };

        public override bool ThreadSafe => true;

        internal override bool Hidden => true;
        
        public PluginController(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
        }

        private void CommandHandler(object sender, CommandEventArgs e)
        {
            if (e.Arguments.Length < 1)
                throw new CommandSyntaxException($"Expected 1 argument but found {e.Arguments.Length}.");

            if (e.Arguments[0] == "uninstall")
                Uninstall(e);
            else if (e.Arguments[0] == "loaded")
                ListLoaded(e);
            else if (e.Arguments[0] == "installed")
                ListInstalled(e);
            else
                throw new CommandSyntaxException($"Unknown operation to peform '{e.Arguments[0]}'.");
        }

        private void Uninstall(CommandEventArgs e)
        {
            if (e.Arguments.Length != 2)
                throw new CommandSyntaxException();

            try
            {
                Server.InternalPluginManager.Uninstall(e.Arguments[1]);

                Logger.Info("Uninstall successful");
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(ex.Message, ex);
            }
            catch (KeyNotFoundException)
            {
                Logger.Error("No plugins by that name could be found.");
            }
        }

        private void ListLoaded(CommandEventArgs e)
        {
            if (e.Arguments.Length != 1)
                throw new CommandSyntaxException();

            Plugin[] loaded;
            if (e.HasFlag("h"))
                loaded = Server.InternalPluginManager.ActuallyGetAllPlugins();
            else
                loaded = Server.InternalPluginManager.GetAllPlugins();

            StringBuilder sb = new StringBuilder();
            sb.Append("Found ");
            sb.Append(loaded.Length);
            sb.AppendLine(" plugins loaded.");
            for (int i = 0; i < loaded.Length; i++)
            {
                sb.Append((i+1).ToString().PadRight(4));
                sb.Append(loaded[i].Name.PadRight(24));
                sb.AppendLine(loaded[i].Hidden ? "(Hidden)" : "");
            }

            Logger.Info(sb.ToString());
        }

        private void ListInstalled(CommandEventArgs e)
        {
            if (e.Arguments.Length != 1)
                throw new CommandSyntaxException();

            StringBuilder sb = new StringBuilder();
                        
            int i = 0;
            foreach (PluginRecord plugin in Server.DataManager.ReadAllPluginRecords())
            {
                sb.Append((++i).ToString().PadRight(4));
                sb.Append(plugin.Name.PadRight(24));
                sb.Append(" ");
                sb.Append("V");
                sb.AppendLine(plugin.Version.ToString());
            }

            sb.Insert(0, "Found " + i + " plugins installed.\n");

            Logger.Info(sb.ToString());
        }
    }
}
