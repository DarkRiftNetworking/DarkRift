/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace DarkRift.Client
{
    /// <summary>
    ///     Configuration for the <see cref="ClientObjectCache"/>.
    /// </summary>
    public class ClientObjectCacheSettings : ObjectCacheSettings
    {
        /// <summary>
        ///     Return settings so no objects are cached.
        /// </summary>
        public static new ClientObjectCacheSettings DontUseCache { get; } = new ClientObjectCacheSettings();

        /// <summary>
        ///     The maximum number of MessageReceivedEventArgs to cache per thread.
        /// </summary>
        public int MaxMessageReceivedEventArgs { get; set; }
    }
}
