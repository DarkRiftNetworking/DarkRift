/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.Serialization;

namespace DarkRift.Dispatching
{
    /// <summary>
    ///     A wrapper for unhandled exceptions thrown within dispatcher tasks so the stacktrace is preserved.
    /// </summary>
    [Serializable]
    public sealed class DispatcherException : Exception
    {
        /// <summary>
        ///     Creates a new exception with no parameters.
        /// </summary>
        public DispatcherException()
        {
        }

        /// <summary>
        ///     Creates a new exception with a message.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        public DispatcherException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Creates a new exception with a message and inner exceptions.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        /// <param name="innerException">The exception raised in the task.</param>
        public DispatcherException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}