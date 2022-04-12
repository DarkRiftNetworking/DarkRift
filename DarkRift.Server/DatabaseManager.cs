/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;

namespace DarkRift.Server
{
    /// <summary>
    ///     Manages the connection strings used by server plugins.
    /// </summary>
    [Obsolete("Use configuration settings under the plugin that requires the database connection string.")]
    internal class DatabaseManager : IDatabaseManager
    {
        /// <summary>
        ///     The connection strings we are aware of.
        /// </summary>
        private readonly Dictionary<string, string> connectionStrings;

        internal DatabaseManager(ServerSpawnData.DatabaseSettings spawnData)
        {
            connectionStrings = new Dictionary<string, string>();

            if (spawnData != null && spawnData.Databases != null)
            {
                //Load provider factories
                lock (connectionStrings)
                {
                    foreach (ServerSpawnData.DatabaseSettings.DatabaseConnectionData database in spawnData.Databases)
                        connectionStrings.Add(database.Name, database.ConnectionString);
                }
            }
        }

        /// <inheritdoc/>
        public string GetConnectionString(string providerName)
        {
            lock (connectionStrings)
                return connectionStrings[providerName];
        }

        /// <inheritdoc/>
        public string this[string providerName]
        {
            get
            {
                lock (connectionStrings)
                    return GetConnectionString(providerName);
            }
        }
    }
}
