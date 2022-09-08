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
        private StreamWriter LogFileStream { get; set; }

        private readonly object streamLock = new object();

        /// <summary>
        ///     The directory we are writing to.
        /// </summary>
        public string LogFilePath { get; private set; }

        /// <summary>
        ///     The format of the path (used for rotation).
        /// </summary>
        public string LogFilePathFormat { get; private set; }

        /// <summary>
        ///     The time this file started (for rotation).
        /// </summary>
        public DateTime LogCreated { get; private set; }

        /// <summary>
        ///     Number of bytes written.
        /// </summary>
        public int LogBytesWritten { get; private set; }

        /// <summary>
        ///     Maximum number of bytes (before rotating). 0 to disable.
        /// </summary>
        public int LogMaxBytes { get; private set; }

        /// <summary>
        ///     Maximum number of seconds per log file (before rotating). 0 to disable.
        /// </summary>
        public int LogMaxTimeSeconds { get; private set; }

        /// <summary>
        ///     Creates a new file writer with the given plugin load data.
        /// </summary>
        /// <param name="logWriterLoadData">The data for this plugin.</param>
        public FileWriter(LogWriterLoadData logWriterLoadData) : base(logWriterLoadData)
        {

            // Sore the format for creating the filename
            LogFilePathFormat = logWriterLoadData.Settings["file"] ?? "Logs/{0:yyyy-MM-dd}/{0:HH-mm-ss}.txt";

            // Get the maximum size (in MB, convert to bytes)
            try
            {
                int logMaxSizeMB = int.Parse(logWriterLoadData.Settings["maxSize"] ?? "100");
                LogMaxBytes = Math.Max(0, logMaxSizeMB * 1000000);
            }
            catch (FormatException)
            {
                LogMaxBytes = 0;
            }

            // Get the maximum time before rotation (in seconds)
            try
            {
                LogMaxTimeSeconds = int.Parse(logWriterLoadData.Settings["maxTime"] ?? "86400");
            }
            catch (FormatException)
            {
                LogMaxTimeSeconds = 0;
            }

            CreateStream();
        }

        private void CreateStream()
        {
            // Only create a new stream if the file name is different from existing.
            string newFilePath = string.Format(LogFilePathFormat, DateTime.Now);
            if (LogFilePath is null || newFilePath != LogFilePath)
            {
                // Close the old stream, if open
                if (!(LogFilePath is null))
                {
                    LogFileStream.WriteLine($"Log File Continues in {newFilePath}");
                    CloseStream();
                }

                //Get the log directory concatenated with the date
                LogFilePath = string.Format(LogFilePathFormat, DateTime.Now);

                //Create the actual log directory (will do nothing if it already exists)
                Directory.CreateDirectory(Path.GetDirectoryName(LogFilePath));

                //Create the log file and stream for it
                LogFileStream = new StreamWriter(LogFilePath);

                // Write
                if (!(LogFilePath is null))
                {
                    LogFileStream.WriteLine($"Log File Continued from {LogFilePath}");
                    LogFileStream.Flush();
                }


                // Reset the counters
                LogFilePath = newFilePath;
                LogBytesWritten = 0;
                LogCreated = DateTime.Now;
            }
        }

        private void CloseStream()
        {
            LogFileStream.Flush();
            LogFileStream.Close();
            LogFileStream.Dispose();
        }

        /// <inheritdoc/>
        public override void WriteEvent(WriteEventArgs args)
        {
            //Write to file
            lock (streamLock)
            {
                LogFileStream.WriteLine(args.FormattedMessage);

                LogFileStream.Flush();

                // Remember number of btyes written
                LogBytesWritten += args.FormattedMessage.Length + 2;

                // If limits exceeded, then rotate while lock still in place
                // Will only rotate is log file older than a second (to avoid naming clashes)
                if ((LogMaxBytes > 0 && LogBytesWritten >= LogMaxBytes)
                    || (LogMaxTimeSeconds > 0 && (int)DateTime.Now.Subtract(LogCreated).TotalSeconds >= LogMaxTimeSeconds)
                ) {
                    CreateStream();
                }
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (streamLock)
                    CloseStream();
            }
            base.Dispose(disposing);
        }
    }
}
