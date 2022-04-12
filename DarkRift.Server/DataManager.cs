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
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace DarkRift.Server
{
    /// <summary>
    ///     Handles the persistent data for a DarkRift instance.
    /// </summary>
    // TODO DR3 don't store any state between sessions, cattle not pets!
    internal sealed class DataManager : IDisposable
    {
        /// <summary>
        ///     The name of the plugins DB file.
        /// </summary>
        private readonly string pluginsFileName;

        /// <summary>
        ///     The name of the plugins DB mutex.
        /// </summary>
        private readonly Mutex pluginsFileMutex;

        /// <summary>
        ///     The directory for storing data in.
        /// </summary>
        private readonly string dataDirectory;

        /// <summary>
        /// Logger to use for logging.
        /// </summary>
        private readonly Logger logger;

        /// <summary>
        ///     Class encapsulating the legacy locking functionality
        /// </summary>
        private sealed class LockedFile : IDisposable
        {
            /// <summary>
            ///     The file stream to the file.
            /// </summary>
            private readonly FileStream stream;

            /// <summary>
            ///     The legacy mutex for the file locking.
            /// </summary>
            //TODO DR3 remove mutex usage
            private readonly Mutex mutex;

            /// <summary>
            ///     Returns the reader for the stream.
            /// </summary>
            private readonly TextReader reader;

            /// <summary>
            ///     Returns the writer for the stream.
            /// </summary>
            private readonly TextWriter writer;

            /// <summary>
            ///     Returns whether the file is empty or not.
            /// </summary>
            public bool IsEmpty => stream.Length == 0;

            /// <summary>
            ///     Creates a new wrapper for the file specified and aquires the lock.
            /// </summary>
            /// <param name="filename">The file to aquire.</param>
            /// <param name="fileMutex">The legacy mutex for the file locking.</param>
            public LockedFile(string filename, Mutex fileMutex)
            {
                // For legacy compatibility, lock on the mutex if present
                this.mutex = fileMutex;
                fileMutex?.WaitOne();       // Not supported by some platforms Android

                // Then aquire the file with no sharing for newer compatibility
                this.stream = null;
                do
                {
                    try
                    {
                        this.stream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);        // Not supported by some platforms: Linux

                        try
                        {
                            // Not supported by some platforms: Mac
                            this.stream.Lock(0, 0);     // Third option for locking, see https://stackoverflow.com/questions/35444470/how-to-lock-unlock-a-file-across-process
                        }
                        catch (PlatformNotSupportedException)
                        {
                            // Probably a Mac, guess we have to hope one of the other methods worked!
                        }
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(0);
                    }
                }
                while (this.stream == null);

                this.reader = new StreamReader(stream);
                this.writer = new StreamWriter(stream);
            }

            /// <summary>
            ///     Returns the file as an XDocument.
            /// </summary>
            /// <returns>The XDocument for the file.</returns>
            public XDocument Load()
            {
                stream.Seek(0, SeekOrigin.Begin);
                return XDocument.Load(reader);
            }

            /// <summary>
            ///     Writes the XDocument to the file.
            /// </summary>
            /// <param name="document">The XDocument to save.</param>
            public void Save(XDocument document)
            {
                // Truncate file then write in contents
                stream.Seek(0, SeekOrigin.Begin);
                stream.SetLength(0);
                document.Save(writer);
            }

            /// <summary>
            ///     Releases the locks on the file.
            /// </summary>
            public void Dispose()
            {
                stream.Close();
                reader.Close();
                mutex?.ReleaseMutex();
            }
        }

        internal DataManager(ServerSpawnData.DataSettings settings, Logger logger)
        {
            this.dataDirectory = settings.Directory;
            this.logger = logger;

            DirectoryInfo directory = Directory.CreateDirectory(dataDirectory);
            directory.Attributes |= FileAttributes.Hidden;

            pluginsFileName = Path.Combine(dataDirectory, "Plugins.xml");

            // Try to create a legacy mutex for locking the file, if not don't worry
            try
            {
                pluginsFileMutex = new Mutex(false, pluginsFileName.GetHashCode() + " lock");
            }
            catch (NotSupportedException)
            {
                pluginsFileMutex = null;
            }

            CreatePluginsTable();
        }

        #region Resouce Folders

        /// <summary>
        ///     Gets the location of a plugins resources directory.
        /// </summary>
        /// <param name="pluginName">The name of the plugin.</param>
        /// <returns>The path to the resource directory.</returns>
        internal string GetResourceDirectory(string pluginName) {
            // TODO DR3 store plugin data not in root of the data directory
            return Path.Combine(dataDirectory, pluginName);
        }

        /// <summary>
        ///     Creates the resource directory for a plugin if it doesn't exist.
        /// </summary>
        /// <param name="pluginName">The name of the plugin.</param>
        internal void CreateResourceDirectory(string pluginName)
        {
            Directory.CreateDirectory(GetResourceDirectory(pluginName));
        }

        /// <summary>
        ///     Deletes the resource directory for a plugin.
        /// </summary>
        /// <param name="pluginName">The name of the plugin.</param>
        internal void DeleteResourceDirectory(string pluginName)
        {
            Directory.Delete(GetResourceDirectory(pluginName), true);
        }

        #endregion

        #region Plugin Records

        /// <summary>
        ///     Atomically reads a plugin record and updates the fields as specified or
        ///     creates a new record if not present.
        /// </summary>
        /// <param name="name">The plugin to read and set.</param>
        /// <param name="version">The version to update the record to.</param>
        internal PluginRecord ReadAndSetPluginRecord(string name, Version version)
        {
            try
            {
                using (LockedFile file = new LockedFile(pluginsFileName, pluginsFileMutex))
                {
                    XDocument doc = file.Load();

                    XElement element = doc.Root
                        .Elements()
                        .FirstOrDefault(x => x.Attribute("name").Value == name);

                    //If not there, build it
                    PluginRecord record;
                    if (element == null)
                    {
                        element = new XElement("plugin");

                        var idAttribute = doc.Root.Attribute("nextID");
                        uint id = uint.Parse(idAttribute.Value);
                        idAttribute.SetValue(id + 1);

                        element.SetAttributeValue("id", id);
                        element.SetAttributeValue("name", name);

                        doc.Root.Add(element);

                        record = null;
                    }
                    else
                    {
                        record = new PluginRecord(
                            uint.Parse(element.Attribute("id").Value),
                            name,
                            new Version(element.Attribute("version").Value)
                        );
                    }

                    element.SetAttributeValue("version", version);

                    file.Save(doc);

                    return record;
                }
            }
            catch (XmlException)
            {
                logger.Error($"The plugins index file ({pluginsFileName}) was corrupt and could not be loaded. It may be possible to fix this manually by inspecting the file; otherwise, it is likely that you will need to delete the file to force DarkRift to regenerate it, however doing so will cause all plugins to reinstall.");
                throw;
            }
        }

        /// <summary>
        ///     Reads a record from the plugin metadata.
        /// </summary>
        /// <param name="name">The name fo the plugin.</param>
        /// <returns>The plugin record.</returns>
        internal PluginRecord ReadPluginRecord(string name)
        {
            try
            {
                XDocument doc;
                using (LockedFile file = new LockedFile(pluginsFileName, pluginsFileMutex))
                    doc = file.Load();

                var element = doc.Root
                        .Elements()
                        .FirstOrDefault(x => x.Attribute("name").Value == name);

                if (element == null)
                    return null;

                return new PluginRecord(
                    uint.Parse(element.Attribute("id").Value),
                    name,
                    new Version(element.Attribute("version").Value)
                );
            }
            catch (XmlException)
            {
                logger.Error($"The plugins index file ({pluginsFileName}) was corrupt and could not be loaded. It may be possible to fix this manually by inspecting the file; otherwise, it is likely that you will need to delete the file to force DarkRift to regenerate it, however doing so will cause all plugins to reinstall.");
                throw;
            }
        }

        /// <summary>
        ///     Returns all records in the plugins table.
        /// </summary>
        /// <returns>The records stored.</returns>
        internal IEnumerable<PluginRecord> ReadAllPluginRecords()
        {
            try
            {
                XDocument doc;
                using (LockedFile file = new LockedFile(pluginsFileName, pluginsFileMutex))
                    doc = file.Load();

                return doc.Root
                    .Elements()
                    .Select(
                        (e) =>
                            new PluginRecord(
                                uint.Parse(e.Attribute("id").Value),
                                e.Attribute("name").Value,
                                new Version(e.Attribute("version").Value)
                            )
                    );
            }
            catch (XmlException)
            {
                logger.Error($"The plugins index file ({pluginsFileName}) was corrupt and could not be loaded. It may be possible to fix this manually by inspecting the file; otherwise, it is likely that you will need to delete the file to force DarkRift to regenerate it, however doing so will cause all plugins to reinstall.");
                throw;
            }
        }

        /// <summary>
        ///     Deletes a record from the plugin table.
        /// </summary>
        /// <param name="name">The plugin to delete.</param>
        internal void DeletePluginRecord(string name)
        {
            try
            {
                using (LockedFile file = new LockedFile(pluginsFileName, pluginsFileMutex))
                {
                    XDocument doc = file.Load();

                    doc.Root
                        .Elements()
                        .Single(e => e.Attribute("name").Value == name)
                        .Remove();

                    file.Save(doc);
                }
            }
            catch (XmlException)
            {
                logger.Error($"The plugins index file ({pluginsFileName}) was corrupt and could not be loaded. It may be possible to fix this manually by inspecting the file; otherwise, it is likely that you will need to delete the file to force DarkRift to regenerate it, however doing so will cause all plugins to reinstall.");
                throw;
            }
        }

        /// <summary>
        ///     Creates a new table for storing plugin metadata.
        /// </summary>
        private void CreatePluginsTable()
        {
            try
            {
                using (LockedFile file = new LockedFile(pluginsFileName, pluginsFileMutex))
                {
                    // If the file contains content, it doesn't need initialising
                    if (!file.IsEmpty)
                        return;

                    XDocument doc = new XDocument();
                    if (doc.Root == null)
                        doc.Add(new XElement("plugins", new XAttribute("nextID", 0)));

                    file.Save(doc);
                }
            }
            catch (XmlException)
            {
                logger.Error($"The plugins index file ({pluginsFileName}) was corrupt and could not be loaded. It may be possible to fix this manually by inspecting the file; otherwise, it is likely that you will need to delete the file to force DarkRift to regenerate it, however doing so will cause all plugins to reinstall.");
                throw;
            }
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (pluginsFileMutex != null)
                        pluginsFileMutex.Close();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
