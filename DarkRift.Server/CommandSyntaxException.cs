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
    ///     Exception indicating a syntax error in a command invocation.
    /// </summary>
    public class CommandSyntaxException : Exception
    {
        /// <summary>
        ///     Creates a new syntax error.
        /// </summary>
        public CommandSyntaxException() : base() { }

        /// <summary>
        ///     Creates a new syntax error with a given message.
        /// </summary>
        public CommandSyntaxException(string message) : base(message) { }

        /// <summary>
        ///     Creates a new syntax error with a given message and inner exception.
        /// </summary>
        public CommandSyntaxException(string message, Exception innerException) : base(message, innerException) { }
    }
}
