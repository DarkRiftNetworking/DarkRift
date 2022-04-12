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
    ///     IP addressing modes.
    /// </summary>
    [Obsolete("Use IPAddress.Family instead.")]
    public enum IPVersion
    {
        /// <summary>
        ///     Indicates IPv4 is in use/should be used.
        /// </summary>
        IPv4,

        /// <summary>
        ///     Indicates IPv6 is in user/should be used.
        /// </summary>
        IPv6
    }
}
