/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace DarkRift.Server
{
    /// <summary>
    ///     Event arguments for <see cref="Command"/> callbacks.
    /// </summary>
    public class CommandEventArgs : EventArgs
    {
        /// <summary>
        ///     The command being executed.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        ///     The command as typed in by the user.
        /// </summary>
        public string OriginalCommand { get; }

        /// <summary>
        ///     The arguments the command was called with.
        /// </summary>
        public string[] RawArguments { get; }
        
        /// <summary>
        ///     The arguments passed with the command that weren't flags.
        /// </summary>
        public string[] Arguments { get; }

        /// <summary>
        ///     The flags that were passed with the command.
        /// </summary>
        public NameValueCollection Flags { get; }

        /// <summary>
        ///     Creates a new CommandEventArgs object.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="originalCommand">The command as typed in by the user.</param>
        /// <param name="rawArguments">The arguments the command was called with.</param>
        /// <param name="arguments">The arguments passed with the command that weren't flags.</param>
        /// <param name="flags">The flags that were passed with the command.</param>
        internal CommandEventArgs(Command command, string originalCommand, string[] rawArguments, string[] arguments, NameValueCollection flags)
        {
            this.Command = command;
            this.OriginalCommand = originalCommand;
            this.RawArguments = rawArguments;
            this.Arguments = arguments;
            this.Flags = flags;
        }
        
        /// <summary>
        ///     Returns whether the arguments contain the specified flag.
        /// </summary>
        /// <param name="name">The name of the flag.</param>
        /// <returns>Whether the flag is present.</returns>
        public bool HasFlag(string name)
        {
            return Flags.AllKeys.Contains(name);
        }
    }
}
