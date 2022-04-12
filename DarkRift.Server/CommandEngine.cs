/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace DarkRift.Server
{
    /// <summary>
    ///     Handles commands sent into the server.
    /// </summary>
    public sealed class CommandEngine
    {
        /// <summary>
        ///     The thread helper the command engine will use.
        /// </summary>
        private readonly DarkRiftThreadHelper threadHelper;

        /// <summary>
        ///     The plugin manager this command engine will use to find handlers.
        /// </summary>
        private readonly PluginManager pluginManager;

        /// <summary>
        ///     The logger the command engine will use.
        /// </summary>
        private readonly Logger logger;

        /// <summary>
        ///     Creates a new command engine.
        /// </summary>
        /// <param name="threadHelper">The thread helper the command engine will use.</param>
        /// <param name="pluginManager">The plugin manager this command engine will use to find handlers.</param>
        /// <param name="logger">The logger the command engine will use.</param>
        internal CommandEngine(DarkRiftThreadHelper threadHelper, PluginManager pluginManager, Logger logger)
        {
            this.threadHelper = threadHelper;
            this.pluginManager = pluginManager;
            this.logger = logger;
        }

        /// <summary>
        ///     Invokes a command on the specified plugin.
        /// </summary>
        /// <param name="command">The command to invoke.</param>
        internal void HandleCommand(string command)
        {
            if (command == null)
                return;

            command = command.Trim();

            if (string.IsNullOrEmpty(command))
                return;

            logger.Trace("Command entered: '" + command + "'");

            //Get command
            Command toInvoke;
            try
            {
                toInvoke = FindCommand(command);
            }
            catch (Exception e)
            {
                logger.Error("Unable to find an appropriate handler for the command. Cannot invoke it", e);

                return;
            }

            InvokeCommand(command, toInvoke);
        }

        /// <summary>
        ///     Invokes the given command.
        /// </summary>
        /// <param name="rawCommand">The command as entered into the console.</param>
        /// <param name="command">The command to run.</param>
        private void InvokeCommand(string rawCommand, Command command)
        {
            CommandEventArgs args = BuildCommandEventArgs(rawCommand, command);

            threadHelper.DispatchIfNeeded(() =>
            {
                try
                {
                    command.Handler.Invoke(this, args);
                }
                catch (CommandSyntaxException e)
                {
                    logger.Error($"Syntax Error: {e.Message}\nUsage: {command.Usage}");
                }
                catch (Exception e)
                {
                    logger.Error($"A plugin encountered an error whilst handling the command '{rawCommand}'. (See logs for exception)", e);
                }
            });
        }

        /// <summary>
        ///     Constructs a new CommandEventArgs object from the given command.
        /// </summary>
        /// <param name="rawCommand">The command as invoked on the console.</param>
        /// <param name="command">The command being executed.</param>
        /// <returns>The event args for the event.</returns>
        internal static CommandEventArgs BuildCommandEventArgs(string rawCommand, Command command)
        {
            string[] rawArguments = ParseArguments(GetArguments(rawCommand));
            string[] arguments = GetArguments(rawArguments);
            NameValueCollection flags = GetFlags(rawArguments);

            return new CommandEventArgs(command, rawCommand, rawArguments, arguments, flags);
        }

        /// <summary>
        ///     Searches all plugins for a given command.
        /// </summary>
        /// <param name="command">The command to search for.</param>
        /// <returns>The command.</returns>
        internal Command FindCommand(string command)
        {
            //Split the command up
            string pluginName = GetIntendedPlugin(command);
            string commandName = GetCommandName(command);

            Plugin plugin;
            if (pluginName == null)
            {
                plugin = FindPluginWithCommand(commandName);
            }
            else
            {
                try
                {
                    plugin = pluginManager[pluginName];
                }
                catch (KeyNotFoundException)
                {
                    throw new KeyNotFoundException($"Could not find plugin with the name '{pluginName}'.");
                }
            }

            string commandNameLower = commandName.ToLower();
            return plugin.Commands.Single((x) => x.Name.ToLower() == commandNameLower);
        }

        /// <summary>
        ///     Searches all plugins for the command with the specified name.
        /// </summary>
        /// <param name="commandName">The name of the command to find.</param>
        /// <returns>The plugin containing the command.</returns>
        private Plugin FindPluginWithCommand(string commandName)
        {
            string commandNameLower = commandName.ToLower();

            //Filter out to plugins containing that command
            var found =
                pluginManager.ActuallyGetAllPlugins()
                    .Where(
                        (x) =>
                            x.Commands.Any((y) => y.Name.ToLower() == commandNameLower)
                    );

            //Report on any errors
            int count = found.Count();
            if (count == 0)
                throw new InvalidOperationException($"Could not find any plugins able to handle the command {commandName}.");

            if (count > 1)
            {
                //Build error
                string list = "";
                foreach (Plugin p in found)
                    list += "\n\t" + p.Name;

                throw new InvalidOperationException($"Found {count} plugin(s) with that command, please specify which you mean: {list}");
            }

            return found.First();
        }

        /// <summary>
        ///     Returns all command available.
        /// </summary>
        /// <returns>The commands found.</returns>
        internal IEnumerable<Command> GetCommands()
        {
            return pluginManager.ActuallyGetAllPlugins().SelectMany(p => p.Commands);
        }
        
        /// <summary>
        ///     Returns all command available in the given plugin.
        /// </summary>
        /// <returns>The commands found.</returns>
        internal IEnumerable<Command> GetCommands(string pluginName)
        {
            return pluginManager.GetPluginByName(pluginName).Commands;
        }
        
        /// <summary>
        ///     Gets the plugin a command was intended for or null if none was specified.
        /// </summary>
        /// <param name="command">The command to parse</param>
        /// <returns>The name of the plugin the command is intended for.</returns>
        public static string GetIntendedPlugin(string command)
        {
            if (command.Contains('/'))
                return command.Split(new char[] { '/' }, 2, StringSplitOptions.None)[0];
            else
                return null;
        }

        /// <summary>
        ///     Returns the command part of a given input string.
        /// </summary>
        /// <param name="command">The command to parse.</param>
        /// <returns>The name and arguments of the command invoked.</returns>
        public static string GetCommandAndArgs(string command)
        {
            if (command.Contains('/'))
                return command.Split(new char[] { '/' }, 2, StringSplitOptions.None)[1];
            else
                return command;
        }

        /// <summary>
        ///     Returns the name of the command to be executed.
        /// </summary>
        /// <param name="command">The command to parse.</param>
        /// <returns>The name or the command invoked.</returns>
        public static string GetCommandName(string command)
        {
            return GetCommandAndArgs(command).Split(new char[] { ' ' }, 2)[0];
        }

        /// <summary>
        ///     Returns the arguments of the command to be executed.
        /// </summary>
        /// <param name="command">The command to parse.</param>
        /// <returns>The argument string of the command invoked</returns>
        public static string GetArguments(string command)
        {
            string[] parts = GetCommandAndArgs(command).Split(new char[] { ' ' }, 2, StringSplitOptions.None);
            return parts.Length == 2 ? parts[1] : "";
        }

        /// <summary>
        ///     Returns an array of raw arguments in the command invoked.
        /// </summary>
        /// <param name="arguments">The arguments part of the invocation.</param>
        /// <returns>The list of raw arguments for the invocation.</returns>
        public static string[] ParseArguments(string arguments)
        {
            List<string> parsed = new List<string>();

            //Get arguments
            string current = "";
            int i = 0;
            while (i < arguments.Length)
            {
                switch (arguments[i])
                {
                    case ' ':
                        if (current.Length > 0)
                        {
                            parsed.Add(current);
                            current = "";
                        }
                        break;

                    case '"':
                        //Add last argument
                        while (++i < arguments.Length)
                        {
                            //If escaped swap slash for "
                            if (arguments[i] == '"' && arguments[i - 1] == '\\')
                            {
                                current = current.Substring(0, current.Length - 1);
                                current += '"';
                            }

                            //If not escaped, exit
                            else if (arguments[i] == '"')
                            {
                                parsed.Add(current);
                                current = "";
                                break;
                            }

                            //Otherwise add
                            else
                            {
                                current += arguments[i];
                            }
                        }
                        break;

                    case '\'':
                        //Add last argument
                        while (++i < arguments.Length)
                        {
                            //If escaped swap slash for '
                            if (arguments[i] == '\'' && arguments[i - 1] == '\\')
                            {
                                current = current.Substring(0, current.Length - 1);
                                current += '\'';
                            }

                            //If not escaped, exit
                            else if (arguments[i] == '\'')
                            {
                                parsed.Add(current);
                                current = "";
                                break;
                            }

                            //Otherwise add
                            else
                            {
                                current += arguments[i];
                            }
                        }
                        break;

                    default:
                        current += arguments[i];
                        break;
                }

                i++;
            }

            //Add any left in current variable
            if (current.Length > 0)
            {
                parsed.Add(current);
            }

            return parsed.ToArray();
        }

        /// <summary>
        ///     Returns an array of arguments in the command invoked.
        /// </summary>
        /// <param name="rawArguments">The raw arguments parsed from the invocation.</param>
        /// <returns>The list of arguments for the invocation.</returns>
        public static string[] GetArguments(string[] rawArguments)
        {
            List<string> args = new List<string>(rawArguments.Length);

            foreach (string argument in rawArguments)
            {
                if (!argument.StartsWith("-"))
                    args.Add(argument);
            }

            return args.ToArray();
        }

        /// <summary>
        ///     Returns the flags from the command invoked.
        /// </summary>
        /// <param name="rawArguments">The raw arguments parsed from the invocation.</param>
        /// <returns>The flags for the invocation.</returns>
        public static NameValueCollection GetFlags(string[] rawArguments)
        {
            NameValueCollection flags = new NameValueCollection(rawArguments.Length);

            foreach (string argument in rawArguments)
            {
                if (argument.StartsWith("-"))
                {
                    string[] parts = argument.Substring(1).Split(new char[] { '=' }, 2);

                    if (parts.Length == 2)
                        flags.Add(parts[0], parts[1]);
                    else
                        flags.Add(parts[0], null);
                }
            }

            return flags;
        }
    }
}
