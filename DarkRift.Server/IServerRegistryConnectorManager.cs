/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace DarkRift.Server
{
#if PRO
    /// <summary>
    ///     Manager for the server registry connectors.
    /// </summary>
    public interface IServerRegistryConnectorManager
    {
        /// <summary>
        ///     The server registry connector.
        /// </summary>
        ServerRegistryConnector ServerRegistryConnector { get; set; }
    }
#endif
}
