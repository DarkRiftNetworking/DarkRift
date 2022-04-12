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
    ///     The level of logging that is associated with a log entry.
    /// </summary>
    public enum LogType
    {
        /// <summary>
        ///     The quietest log level. Indicates the information is not of immediate importance but it worth note.
        /// </summary>
        Trace = 0,

        /// <summary>
        ///     Indicates the information is of general use.
        /// </summary>
        Info = 1,

        /// <summary>
        ///     Indicates the information is warning the user.
        /// </summary>
        Warning = 2,

        /// <summary>
        ///     Indicates the information is the result of an error.
        /// </summary>
        Error = 3,

        /// <summary>
        ///     Indicates the information is the result of a fatal error that cannot be recovered from.
        /// </summary>
        Fatal = 4
    }
}
