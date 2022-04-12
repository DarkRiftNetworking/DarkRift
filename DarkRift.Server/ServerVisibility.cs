/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace DarkRift.Server
{
#if PRO
    /// <summary>
    ///     The visibility of the server.
    /// </summary>
    public enum ServerVisibility
    {
        /// <summary>
        ///     Indicates this server can only be connected to by clients.
        /// </summary>
        External,

        /// <summary>
        ///     Indicates this server can only be connected to by other servers.
        /// </summary>
        Internal
    }
#endif
}
