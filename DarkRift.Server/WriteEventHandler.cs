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
    ///     A handler for writing log events.
    /// </summary>
    /// <param name="message">The message being logged.</param>
    /// <param name="logType">The type of event being logged</param>
    /// <param name="exception">The exception (if present) being logged.</param>
    public delegate void WriteEventHandler(string message, LogType logType, Exception exception);
}
