/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DarkRift.Server.Plugins.LogWriters
{
    /// <summary>
    ///     Creates a new log writer that outputs to a file.
    /// </summary>
    public sealed class FileWriter : LogWriter
    {
        /// <inheritdoc/>
        public override Version Version => new Version(1, 0, 0);

        /// <summary>
        ///     The stream to the log file to write to.
        /// </summary>
        private StreamWriter LogFileStream { get; }

        /// <summary>
        ///     The directory we are writing to.
        /// </summary>
        public string LogFilePath { get; private set; }

        /// <summary>
        ///     Creates a new file writer with the given plugin load data.
        /// </summary>
        /// <param name="logWriterLoadData">The data for this plugin.</param>
        public FileWriter(LogWriterLoadData logWriterLoadData) : base(logWriterLoadData)
        {
            //Get the log directory concatenated with the date
            LogFilePath = string.Format(logWriterLoadData.Settings["file"], DateTime.Now);

            //Create the actual log directory (will do nothing if it already exists)
            Directory.CreateDirectory(Path.GetDirectoryName(LogFilePath));

            //Create the log file and stream for it
            LogFileStream = new StreamWriter(LogFilePath);
        }

        /// <inheritdoc/>
        public override void WriteEvent(WriteEventArgs args)
        {
            //Write to file
            lock (LogFileStream)
            {
                LogFileStream.WriteLine(args.FormattedMessage);

                LogFileStream.Flush();
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (LogFileStream)
                    LogFileStream.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
