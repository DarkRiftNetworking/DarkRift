/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;

namespace DarkRift.Server
{
    /// <summary>
    ///     Arguments passed to loggers when reporting an event.
    /// </summary>
    public class WriteEventArgs : EventArgs
    {
        /// <summary>
        ///     The component that sent the message.
        /// </summary>
        public string Sender { get; }

        /// <summary>
        ///     The message that was sent.
        /// </summary>
        public string Message { get; }

        /// <summary>
        ///     The type of log that was sent.
        /// </summary>
        public LogType LogType { get; }

        /// <summary>
        ///     The exception (if present) that caused the event.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        ///     A formatted version of the event.
        /// </summary>
        /// <remarks>
        ///     For efficiency you should log this whereever possible rather than re build your own formatted
        ///     string for the message as this will be precomputed once for all log writers and it well optimized.
        ///     It is also more consistent to those reading logs if tey are all the same format.
        /// </remarks>
        public string FormattedMessage { get; }

        /// <summary>
        ///     The time the log occured.
        /// </summary>
        public DateTime LogTime { get; }

        /// <summary>
        ///     Creates a new write event for log writers.
        /// </summary>
        /// <param name="sender">The object that logged the message.</param>
        /// <param name="message">The message logged.</param>
        /// <param name="logType">The log level of the message logged.</param>
        /// <param name="exception">The exception triggering the message, if one occured.</param>
        /// <param name="formattedMessage">A forrmatted string of the message details.</param>
        /// <param name="logTime">The time the message was logged.</param>
        public WriteEventArgs(string sender, string message, LogType logType, Exception exception, string formattedMessage, DateTime logTime)
        {
            this.Sender = sender;
            this.Message = message;
            this.LogType = logType;
            this.Exception = exception;
            this.FormattedMessage = formattedMessage;
            this.LogTime = logTime;
        }
    }
}
