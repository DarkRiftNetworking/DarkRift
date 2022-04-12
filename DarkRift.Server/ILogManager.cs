/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace DarkRift.Server
{
    /// <summary>
    ///     An object that manages the server's log writers.
    /// </summary>
    /// <remarks>
    ///     <c>Pro only.</c> 
    /// </remarks>
#if PRO
    public
#endif
    interface ILogManager
    {
        /// <summary>
        ///     Searches for a log writer by its name.
        /// </summary>
        /// <param name="name">The name of the log writer to find.</param>
        /// <returns>The log writer.</returns>
        LogWriter GetLogWriterByName(string name);

        /// <summary>
        ///     Searches for log writers by their type.
        /// </summary>
        /// <typeparam name="T">The type of the log writer to find.</typeparam>
        /// <returns>The log writer found.</returns>
        T GetLogWriterByType<T>() where T : LogWriter;

        /// <summary>
        ///     Searches for log writers by their type.
        /// </summary>
        /// <typeparam name="T">The type of the log writer to find.</typeparam>
        /// <returns>The log writers.</returns>
        T[] GetLogWritersByType<T>() where T : LogWriter;

        /// <summary>
        ///     Gets a log writer by its name.
        /// </summary>
        /// <param name="name">The name of the log writer to find.</param>
        /// <returns>The log writer.</returns>
        LogWriter this[string name] { get; }

        /// <summary>
        ///     Returns a logger for the given component.
        /// </summary>
        /// <param name="name">The name of the component to log for.</param>
        Logger GetLoggerFor(string name);
    }
}
