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
    ///     Utility for testing plugins.
    /// </summary>
    // TODO DR3 could this be better static?
    public class PluginTestUtil
    {
        /// <summary>
        ///     Runs a command on the given plugin.
        /// </summary>
        /// <param name="command">The command to invoke. Plugin names will be ignored</param>
        /// <param name="plugin">The plugin to invoke the command on.</param>
        public void RunCommandOn(string command, ExtendedPluginBase plugin)
        {
            string commandName = CommandEngine.GetCommandName(command).ToLower();
            Command commandObj = plugin.Commands.Single((x) => x.Name.ToLower() == commandName);

            commandObj.Handler.Invoke(this, CommandEngine.BuildCommandEventArgs(command, commandObj));
        }

        // TODO builder for plugin spawn data?
    }
}
