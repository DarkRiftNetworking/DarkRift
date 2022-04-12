/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkRift.Server.Plugins.LogWriters
{
    /// <summary>
    ///     A log writer that writes all output to debug.
    /// </summary>
    public sealed class DebugWriter : LogWriter
    {
        /// <inheritdoc />
        public override Version Version => new Version(1, 0, 0);
        
        /// <summary>
        ///     Creates a new debug log writer using the given plugin load data.
        /// </summary>
        /// <param name="logWriterLoadData">The data for this log writer.</param>
        public DebugWriter(LogWriterLoadData logWriterLoadData) : base(logWriterLoadData)
        {
        }

        /// <inheritdoc/>
        public override void WriteEvent(WriteEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine(args.FormattedMessage);
        }
    }
}
