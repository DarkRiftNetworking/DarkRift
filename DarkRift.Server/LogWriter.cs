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
    ///     Base class for all log writers.
    /// </summary>
    public abstract class LogWriter : PluginBase
    {
        /// <summary>
        ///     Creates a new LogWriter.
        /// </summary>
        /// <param name="pluginLoadData">The data to start the log writer with.</param>
        /// <remarks>
        ///     This constructor is now obsolete and should not be used where possible as it carries
        ///     additional, irrelevant server components. Instead, now log writers should define a 
        ///     constructor that uses the LogWriterLoadData which should be a drop in replacement and 
        ///     should also provide for better unit testing.
        ///     
        ///     <code>
        ///         <![CDATA[public MyLogWriter(LogWriterLoadData logWriterLoadData)
        ///     : base(logWriterLoadData)
        /// {
        ///     
        /// }]]>
        ///     </code>
        /// </remarks>
        [Obsolete("Use LogWriter(LogWriterLoadData) constructor instead for better unit testing.")]
        public LogWriter(PluginLoadData pluginLoadData)
            : base(pluginLoadData)
        {
        }

        /// <summary>
        ///     Creates a new LogWriter.
        /// </summary>
        /// <param name="logWriterLoadData">The data to start the log writer with.</param>
        public LogWriter(LogWriterLoadData logWriterLoadData)
            : base(logWriterLoadData)
        {

        }

        /// <summary>
        ///     Writes an event to this log writer.
        /// </summary>
        /// <param name="args">The message to log.</param>
        public abstract void WriteEvent(WriteEventArgs args);
    }
}
