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
    ///     A record for persistent storage of plugin data.
    /// </summary>
    internal sealed class PluginRecord
    {
        /// <summary>
        ///     The ID of the record.
        /// </summary>
        public uint ID { get; }

        /// <summary>
        ///     The name of the plugin.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The version of the plugin currently installed.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        ///     Creates a new plugin record.
        /// </summary>
        /// <param name="id">The ID of the record.</param>
        /// <param name="name">The name of the plugin.</param>
        /// <param name="version">The version of the plugin currently installed.</param>
        public PluginRecord(uint id, string name, Version version)
        {
            this.ID = id;
            this.Name = name;
            this.Version = version;
        }
    }
}
