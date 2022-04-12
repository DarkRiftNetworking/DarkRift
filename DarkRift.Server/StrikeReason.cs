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
    ///     Reasons for strikes to be issued to clients.
    /// </summary>
    public enum StrikeReason
    {
        /// <summary>
        ///     Specifies the message wasn't long enough contain the message header.
        /// </summary>
        InvalidMessageLength,

        /// <summary>
        ///     Specifies the command the client sent was not accepted by the server.
        /// </summary>
        InvalidCommand,

        /// <summary>
        ///     Specifies a plugin requested the strike.
        /// </summary>
        PluginRequest,

        /// <summary>
        ///     Specifies the client's connection requested the strike.
        /// </summary>
        ConnectionRequest,

        /// <summary>
        ///     Specifies a client sent a ping acknowledgement for a nonexistant ping.
        /// </summary>
        UnidentifiedPing,

        /// <summary>
        ///     Specifies a client sent a ping acknowledgment with too long a total RTT.
        /// </summary>
        RttToLarge
    }
}
