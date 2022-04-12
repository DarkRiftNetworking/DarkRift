/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace DarkRift.Server
{
#if PRO
    /// <summary>
    ///     The direction of a connection to another server.
    /// </summary>
    public enum ServerConnectionDirection
    {
        /// <summary>
        ///     Indicates we connect to the remote server.
        /// </summary>
        Upstream,

        /// <summary>
        ///     Indicates the remote server connects to us.
        /// </summary>
        Downstream
    }
#endif
}
