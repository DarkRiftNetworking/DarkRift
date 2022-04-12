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
    internal class DebugCommand : Plugin
    {
        public override bool ThreadSafe => true;

        public override Version Version => new Version(1, 0, 0);

        public override Command[] Commands => new Command[]
        {
            new Command("debug-set", "Sets a property in the server.", "debug-set -<property-name>=<value>|-<property-name>...", DebugSet),
            new Command("debug-get", "Gets a property in the server.", "debug-get -<property-name>...", DebugGet),
            new Command("debug-rtt", "Shows the current round trip time to each client.", "debug-rtt", DebugLatency),
            new Command("debug-logs", "Prints a number of log lines at different levels.", "debug-logs (-l)", DebugLogs)
        };

        internal override bool Hidden => true;

        public DebugCommand(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
        }

        private void DebugSet(object sender, CommandEventArgs e)
        {
            foreach (string property in e.Flags.Keys)
            {
                switch (property)
                {
                    case "events-from-dispatcher":
                        if (e.Flags[property] == "true")
                            ThreadHelper.EventsFromDispatcher = true;
                        else if (e.Flags[property] == "false")
                            ThreadHelper.EventsFromDispatcher = false;
                        else
                            throw new CommandSyntaxException("Cannot set events-from-dispatcher to anything but 'true' or 'false'.");
                        break;

                    default:
                        throw new CommandSyntaxException("Unknown property to set value of.");
                }

                if (e.Flags[property] != null)
                    Logger.Info(property + " set to " + e.Flags[property] + ".");
                else
                    Logger.Info(property + " set.");
            }
        }

        private void DebugGet(object sender, CommandEventArgs e)
        {
            foreach (string property in e.Flags.Keys)
            {
                switch (property)
                {
                    case "events-from-dispatcher":
                        Logger.Info("events-from-dispatcher = " + ThreadHelper.EventsFromDispatcher);
                        break;

                    default:
                        throw new CommandSyntaxException("Unknown property to get value of.");
                }
            }
        }

        private void DebugLatency(object sender, CommandEventArgs e)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("| Client | EndPoint             | Latest RTT       | Latest Smoothed  |");
            builder.AppendLine("|--------|----------------------|------------------|------------------|");

            foreach (IClient client in ClientManager.GetAllClients())
            {
                builder.Append("| ");
                builder.Append(client.ID.ToString().PadRight(6));
                builder.Append(" | ");
                builder.Append(client.RemoteEndPoints.First().ToString().PadRight(20));
                builder.Append(" | ");
                builder.Append((client.RoundTripTime.LatestRtt.ToString() + "ms").PadRight(16));
                builder.Append(" | ");
                builder.Append((client.RoundTripTime.SmoothedRtt.ToString() + "ms").PadRight(16));
                builder.Append(" |");
            }

            Logger.Info(builder.ToString());
        }

        private void DebugLogs(object sender, CommandEventArgs e)
        {
            Logger.Trace("This is a test message at Trace level.", new Exception("This is a test exception at Trace level."));
            Logger.Info("This is a test message at Info level.", new Exception("This is a test exception at Info level."));
            Logger.Warning("This is a test message at Warning level.", new Exception("This is a test exception at Warning level."));
            Logger.Error("This is a test message at Error level.", new Exception("This is a test exception at Error level."));
            Logger.Fatal("This is a test message at Fatal level.", new Exception("This is a test exception at Fatal level."));

            if (e.HasFlag("l"))
            {
#pragma warning disable CS0618 // Type or member is obsolete
                WriteEvent("This is a test message at Trace level.", LogType.Trace, new Exception("This is a test exception at Trace level."));
                WriteEvent("This is a test message at Info level.", LogType.Info, new Exception("This is a test exception at Info level."));
                WriteEvent("This is a test message at Warning level.", LogType.Warning, new Exception("This is a test exception at Warning level."));
                WriteEvent("This is a test message at Error level.", LogType.Error, new Exception("This is a test exception at Error level."));
                WriteEvent("This is a test message at Fatal level.", LogType.Fatal, new Exception("This is a test exception at Fatal level."));
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
    }
}
