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
    ///     The send mode to govern how messages are sent.
    /// </summary>
    public enum SendMode
    {
        /// <summary>
        ///     Sends the message unreliably.
        /// </summary>
        /// <remarks>
        ///     This will not fragment large messages, use Reliable if this is needed.
        /// </remarks>
        Unreliable,

        /// <summary>
        ///     Sends the message and ensures it will arive at it's destination.
        /// </summary>
        /// <remarks>
        ///     THis will also fragment large messages unlike Unreliable.
        /// </remarks>
        Reliable
    }
}
