/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace DarkRift.Server
{
    /// <summary>
    ///     Configuration for the <see cref="ServerObjectCache"/>.
    /// </summary>
    public class ServerObjectCacheSettings : ObjectCacheSettings
    {
        /// <summary>
        ///     Return settings so no objects are cached.
        /// </summary>
        public static new ServerObjectCacheSettings DontUseCache { get; } = new ServerObjectCacheSettings();

        /// <summary>
        ///     The maximum number of MessageReceivedEventArgs to cache per thread.
        /// </summary>
        public int MaxMessageReceivedEventArgs { get; set; }

#if PRO
        /// <summary>
        ///     The maximum number of ServerMessageReceivedEventArgs to cache per thread.
        /// </summary>
        public int MaxServerMessageReceivedEventArgs { get; set; }
#endif
    }
}
