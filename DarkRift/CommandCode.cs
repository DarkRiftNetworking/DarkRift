/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkRift
{
    /// <summary>
    ///     Command codes for control sequences to clients/servers.
    /// </summary>
    internal enum CommandCode : ushort
    {
        /// <summary>
        ///     A identification packet presenting a new ID to a client.
        /// </summary>
        Configure = 0,              //                  [ID, ID]

        /// <summary>
        ///     An identification packet presenting a preknown ID to server.
        /// </summary>
        Identify = 1,              //                  [ID, ID]
    }
}
