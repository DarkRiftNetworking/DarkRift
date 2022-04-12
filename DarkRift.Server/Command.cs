/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkRift.Server
{
    /// <summary>
    ///     A command that can be issued on the server.
    /// </summary>
    public class Command
    {
        /// <summary>
        ///     The name of the command.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     A description of the command.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        ///     A description of the command's usage.
        /// </summary>
        public string Usage { get; private set; }

        /// <summary>
        ///     The action to be executed when the command is invoked.
        /// </summary>
        /// <remarks>
        ///     If the syntax of a command is incorrect a <see cref="CommandSyntaxException"/> should be 
        ///     thrown to indicate this.
        /// </remarks>
        public EventHandler<CommandEventArgs> Handler { get; private set; }

        /// <summary>
        ///     Creates a new command object.
        /// </summary>
        /// <param name="name">The name of the command that will be typed in at the console.</param>
        /// <param name="description">The description of the command for the command manual.</param>
        /// <param name="usage">How the command should be invoked for the command manual.</param>
        /// <param name="handler">The event handler that should be used if the command is invoked.</param>
        public Command (string name, string description, string usage, EventHandler<CommandEventArgs> handler)
        {
            this.Name = name;
            this.Description = description;
            this.Usage = usage;
            this.Handler = handler;
        }
    }
}
