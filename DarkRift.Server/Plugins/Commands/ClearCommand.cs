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
    ///     Command to clear the console window.
    /// </summary>
    internal class ClearCommand : Plugin
    {
        public override bool ThreadSafe => true;

        public override Version Version => new Version(1, 0, 0);

        public override Command[] Commands => new Command[] {
            new Command("clear", "Clears the console window.", "clear", (sender, args) => Console.Clear())
        };

        internal override bool Hidden => true;

        public ClearCommand(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
        }
    }
}
