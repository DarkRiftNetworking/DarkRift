/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace DarkRift.Server
{
    //TODO implement PluginManagerBase
    internal sealed class LogManager : IDisposable, ILogManager
    {
        private const int TYPE_PAD = 10;
        private const int NAME_PAD = 22;

        /// <summary>
        ///     The log writers to use for messages.
        /// </summary>
        private LogWriter[] logWriters;

        /// <summary>
        ///     The matrix of writers for logging at the right level.
        /// </summary>
        private LogWriter[][] writerMatrix;

        /// <summary>
        ///     The server this belongs to.
        /// </summary>
        private readonly DarkRiftServer server;

        /// <summary>
        ///     Default writer for logs before log wrtiers have been setup.
        /// </summary>
        private class DefaultWriter : LogWriter
        {
            public override Version Version => throw new NotImplementedException();

            public DefaultWriter(LogWriterLoadData loadData) : base(loadData)
            {

            }

            public override void WriteEvent(WriteEventArgs args)
            {
                Console.WriteLine(args.FormattedMessage);
            }
        }

        /// <summary>
        ///     Creates a new log manager.
        /// </summary>
        /// <param name="server">The server we belong to.</param>
        /// <param name="settings">The settings to load writers from.</param>
        internal LogManager(DarkRiftServer server, ServerSpawnData.LoggingSettings settings)
        {
            this.server = server;

            //Load default writer
            LogWriter writer = new DefaultWriter(new LogWriterLoadData("Initial", server, new NameValueCollection(), GetLoggerFor(nameof(DefaultWriter))));

            //Set this writer to be used for everything but trace
            logWriters = new LogWriter[] { writer };
            if (settings.StartupLogLevels != null)
            {
                writerMatrix = new LogWriter[][]
                {
                    settings.StartupLogLevels.Contains(LogType.Trace) ? logWriters : new LogWriter[0],
                    settings.StartupLogLevels.Contains(LogType.Info) ? logWriters : new LogWriter[0],
                    settings.StartupLogLevels.Contains(LogType.Warning) ? logWriters : new LogWriter[0],
                    settings.StartupLogLevels.Contains(LogType.Error) ? logWriters : new LogWriter[0],
                    settings.StartupLogLevels.Contains(LogType.Fatal) ? logWriters : new LogWriter[0]
                };
            }
            else
            {
                writerMatrix = new LogWriter[][]
                {
                    new LogWriter[0],
                    logWriters,
                    logWriters,
                    logWriters,
                    logWriters
                };
            }
        }

        /// <summary>
        ///     Loads the writers found by the plugin factory.
        /// </summary>
        /// <param name="settings">The settings to load writers from.</param>
        /// <param name="pluginFactory">The server's plugin factory.</param>
        internal void LoadWriters(ServerSpawnData.LoggingSettings settings, PluginFactory pluginFactory)
        {
            if (logWriters != null)
                throw new InvalidOperationException("Cannot load writers if writers are already present. This suggests that writers have already been loaded into the server.\n\nThis is likely an internal DR issue, please consider creating an issue here: https://github.com/DarkRiftNetworking/DarkRift/issues");

            List<LogWriter> traceWriters = new List<LogWriter>();
            List<LogWriter> infoWriters = new List<LogWriter>();
            List<LogWriter> warningWriters = new List<LogWriter>();
            List<LogWriter> errorWriters = new List<LogWriter>();
            List<LogWriter> fatalWriters = new List<LogWriter>();

            logWriters = new LogWriter[settings.LogWriters.Count];

            for (int i = 0; i < settings.LogWriters.Count; i++)
            {
                ServerSpawnData.LoggingSettings.LogWriterSettings s = settings.LogWriters[i];
                
                //Create a load data object and backup
                LogWriterLoadData loadData = new LogWriterLoadData(s.Name, server, s.Settings, GetLoggerFor(nameof(s.Name)));
                PluginLoadData backupLoadData = new PluginLoadData(s.Name, server, s.Settings, GetLoggerFor(nameof(s.Name)),
#if PRO
                    null,
#endif
                    null);

                LogWriter writer = pluginFactory.Create<LogWriter>(s.Type, loadData, backupLoadData);

                logWriters[i] = writer;

                if (s.LogLevels.Contains(LogType.Trace))
                    traceWriters.Add(writer);
                if (s.LogLevels.Contains(LogType.Info))
                    infoWriters.Add(writer);
                if (s.LogLevels.Contains(LogType.Warning))
                    warningWriters.Add(writer);
                if (s.LogLevels.Contains(LogType.Error))
                    errorWriters.Add(writer);
                if (s.LogLevels.Contains(LogType.Fatal))
                    fatalWriters.Add(writer);
            }

            writerMatrix = new LogWriter[][]
            {
                traceWriters.ToArray(),
                infoWriters.ToArray(),
                warningWriters.ToArray(),
                errorWriters.ToArray(),
                fatalWriters.ToArray()
            };
        }
        
        /// <summary>
        ///     Clears all writers.
        /// </summary>
        internal void Clear()
        {
            foreach (LogWriter writer in logWriters)
                writer.Dispose();

            logWriters = null;
            writerMatrix = null;
        }

        /// <summary>
        ///     Writes an event to the logs.
        /// </summary>
        /// <param name="sender">The object that's reporting this event.</param>
        /// <param name="message">The details of the event.</param>
        /// <param name="logType">The type of event that occurred.</param>
        /// <param name="exception">The exception that caused this log.</param>
        internal void WriteEvent(string sender, string message, LogType logType, Exception exception = null)
        {
            if (writerMatrix == null || writerMatrix[(int)logType].Length == 0)
                return;

            string exceptionString = exception?.ToString();

            StringBuilder builder = new StringBuilder(TYPE_PAD + NAME_PAD + 1 + message.Length + (exceptionString?.Length * 2) ?? 0);   //TODO 1 pool object
            builder.Append("[");
            builder.Append(logType.ToString());
            builder.Append("]");
            builder.Append(' ', TYPE_PAD - logType.ToString().Length - 2);
            builder.Append(sender);
            builder.Append(' ', Math.Max(NAME_PAD - sender.Length, 1));

            string[] lines = message.Split('\n');
            builder.Append(lines[0]);
            for (int i = 1; i < lines.Length; i++)
            {
                builder.Append('\n');
                builder.Append(' ', TYPE_PAD + NAME_PAD);
                builder.Append(lines[i]);
            }

            if (exceptionString != null)
            {
                lines = exceptionString.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    builder.Append('\n');
                    builder.Append(' ', TYPE_PAD + NAME_PAD + 1);
                    builder.Append(lines[i]);
                }
            }

            WriteEventArgs args = new WriteEventArgs(sender, message, logType, exception, builder.ToString(), DateTime.Now);    //TODO 1 pool object

            foreach (LogWriter logWriter in writerMatrix[(int)logType])
                logWriter.WriteEvent(args);
        }

        /// <inheritdoc/>
        public LogWriter GetLogWriterByName(string name)
        {
            foreach (LogWriter[] writerLevel in writerMatrix)
            {
                foreach (LogWriter writer in writerLevel)
                {
                    if (writer.Name == name)
                        return writer;
                }
            }

            throw new KeyNotFoundException();
        }

        /// <inheritdoc/>
        public T GetLogWriterByType<T>() where T : LogWriter
        {
            return (T)writerMatrix.SelectMany(w => w)
                        .First(x => x is T);
        }

        /// <inheritdoc/>
        public T[] GetLogWritersByType<T>() where T : LogWriter
        {
            return logWriters.Where(x => x is T).Cast<T>().ToArray();
        }

        /// <inheritdoc/>
        public LogWriter this[string name] => GetLogWriterByName(name);

        /// <inheitdoc />
        public Logger GetLoggerFor(string name)
        {
            return new Logger(name, this);
        }
#region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (LogWriter writer in logWriters)
                        writer.Dispose();
                }
                
                disposedValue = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
        }
#endregion
    }
}
