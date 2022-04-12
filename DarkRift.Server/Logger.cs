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
    ///     Encapsulates logging for a component.
    /// </summary>
    /// <see cref="ILogManager.GetLoggerFor(string)"/>
    // TODO DR3 these methods should be virtual to be at all useful when writing tests!
    public class Logger
    {
        /// <summary>
        ///     The name of the component the logger is logging for.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The log manager to send logs to.
        /// </summary>
        private readonly LogManager logManager;

        /// <summary>
        ///     Creates a new logger with the given name.
        /// </summary>
        /// <param name="name">The name of the component logging for.</param>
        /// <param name="logManager">The log manager to send logs to.</param>
        internal Logger(string name, LogManager logManager)
        {
            Name = name;
            this.logManager = logManager;
        }

        /// <summary>
        ///     Writes an event to the logs.
        /// </summary>
        /// <param name="message">The details of the event.</param>
        /// <param name="logType">The type of event that occurred.</param>
        /// <param name="exception">The exception that caused this log.</param>
        public void Log(string message, LogType logType, Exception exception = null)
        {
            logManager.WriteEvent(Name, message, logType, exception);
        }

        /// <summary>
        ///     Writes a trace event to the logs.
        /// </summary>
        /// <param name="message">The details of the event.</param>
        /// <param name="exception">The exception that caused this log.</param>
        public void Trace(string message, Exception exception = null)
        {
            logManager.WriteEvent(Name, message, LogType.Trace, exception);
        }

        /// <summary>
        ///     Writes an info event to the logs.
        /// </summary>
        /// <param name="message">The details of the event.</param>
        /// <param name="exception">The exception that caused this log.</param>
        public void Info(string message, Exception exception = null)
        {
            logManager.WriteEvent(Name, message, LogType.Info, exception);
        }

        /// <summary>
        ///     Writes a warning event to the logs.
        /// </summary>
        /// <param name="message">The details of the event.</param>
        /// <param name="exception">The exception that caused this log.</param>
        public void Warning(string message, Exception exception = null)
        {
            logManager.WriteEvent(Name, message, LogType.Warning, exception);
        }

        /// <summary>
        ///     Writes an error event to the logs.
        /// </summary>
        /// <param name="message">The details of the event.</param>
        /// <param name="exception">The exception that caused this log.</param>
        public void Error(string message, Exception exception = null)
        {
            logManager.WriteEvent(Name, message, LogType.Error, exception);
        }

        /// <summary>
        ///     Writes a fatal event to the logs.
        /// </summary>
        /// <param name="message">The details of the event.</param>
        /// <param name="exception">The exception that caused this log.</param>
        public void Fatal(string message, Exception exception = null)
        {
            logManager.WriteEvent(Name, message, LogType.Fatal, exception);
        }
    }
}
