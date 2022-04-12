/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;

namespace DarkRift.Server
{
    /// <summary>
    ///     Manages the connection strings used by server plugins.
    /// </summary>
    [Obsolete("Use configuration settings under the plugin that requires the database connection string.")]
    public interface IDatabaseManager
    {
        /// <summary>
        ///     Gets a connection string defined in the configuration file. 
        /// </summary>
        /// <param name="providerName">The name of the connection string.</param>
        /// <returns>
        ///     The connection string.
        /// </returns>
        string this[string providerName] { get; }

        /// <summary>
        ///     Gets a connection string defined in the configuration file. 
        /// </summary>
        /// <param name="providerName">The name of the connection string.</param>
        /// <returns>
        ///     The connection string.
        /// </returns>
        string GetConnectionString(string providerName);
    }
}
