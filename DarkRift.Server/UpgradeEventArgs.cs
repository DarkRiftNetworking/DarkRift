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
    ///     Event arguments for plugin upgrades.
    /// </summary>
#if PRO
    public
#else
    internal
#endif
        class UpgradeEventArgs
    {
        /// <summary>
        ///     The previous version of the plugin installed.
        /// </summary>
        public Version PreviousVersion { get; set; }

        /// <summary>
        ///     Creates a new UpgradeEventArgs object.
        /// </summary>
        /// <param name="previousVersion">The previous version of the plugin installed.</param>
        public UpgradeEventArgs(Version previousVersion)
        {
            PreviousVersion = previousVersion;
        }
    }
}
