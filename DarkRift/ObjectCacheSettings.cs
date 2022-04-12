/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkRift
{
    /// <summary>
    ///     Configuration for the <see cref="ObjectCache"/>.
    /// </summary>
    //TODO DR3 make abstract
    public class ObjectCacheSettings
    {
        /// <summary>
        ///     The maximum number of DarkRiftWriters to cache per thread.
        /// </summary>
        public int MaxWriters { get; set; }

        /// <summary>
        ///     The maximum number of DarkRiftReaders to cache per thread.
        /// </summary>
        public int MaxReaders { get; set; }

        /// <summary>
        ///     The maximum number of Messages to cache per thread.
        /// </summary>
        public int MaxMessages { get; set; }

        /// <summary>
        ///     The maximum number of MessageBuffers to cache per thread.
        /// </summary>
        public int MaxMessageBuffers { get; set; }

        /// <summary>
        ///     The maximum number of SocketAsyncEventArgs to cache per thread.
        /// </summary>
        public int MaxSocketAsyncEventArgs { get; set; }

        /// <summary>
        ///     The maximum number of ActionDisapatcherTasks to cache per thread.
        /// </summary>
        public int MaxActionDispatcherTasks { get; set; }

        /// <summary>
        ///     The maximum number of <see cref="AutoRecyclingArray"/> instances stored per thread.
        /// </summary>
        public int MaxAutoRecyclingArrays { get; set; }

        /// <summary>
        ///     The number of bytes in the extra small memory bocks cached.
        /// </summary>
        public int ExtraSmallMemoryBlockSize { get; set; }

        /// <summary>
        ///     The maximum number of extra small memory blocks stored per thread.
        /// </summary>
        public int MaxExtraSmallMemoryBlocks { get; set; }

        /// <summary>
        ///     The number of bytes in the small memory bocks cached.
        /// </summary>
        public int SmallMemoryBlockSize { get; set; }

        /// <summary>
        ///     The maximum number of small memory blocks stored per thread.
        /// </summary>
        public int MaxSmallMemoryBlocks { get; set; }

        /// <summary>
        ///     The number of bytes in the medium memory bocks cached.
        /// </summary>
        public int MediumMemoryBlockSize { get; set; }

        /// <summary>
        ///     The maximum number of extra small memory blocks stored per thread.
        /// </summary>
        public int MaxMediumMemoryBlocks { get; set; }

        /// <summary>
        ///     The number of bytes in the large memory bocks cached.
        /// </summary>
        public int LargeMemoryBlockSize { get; set; }

        /// <summary>
        ///     The maximum number of large memory blocks stored per thread.
        /// </summary>
        public int MaxLargeMemoryBlocks { get; set; }

        /// <summary>
        ///     The number of bytes in the extra large memory bocks cached.
        /// </summary>
        public int ExtraLargeMemoryBlockSize { get; set; }

        /// <summary>
        ///     The maximum number of extra large memory blocks stored per thread.
        /// </summary>
        public int MaxExtraLargeMemoryBlocks { get; set; }

        /// <summary>
        ///     Return settings so no objects are cached.
        /// </summary>
        [Obsolete("Use DontUseCache property on ClientObjectCacheSettings or ServerObjectCacheSettings instead.")]
        public static readonly ObjectCacheSettings DontUseCache = new ObjectCacheSettings();
    }
}
