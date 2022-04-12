/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;

namespace DarkRift
{
    /// <summary>
    ///     A pool for byte arrays of different lengths for recycling.
    /// </summary>
    /// <remarks>
    ///     This object is not thread safe as it is intended to be used with the ThreadStatic attribute.
    /// </remarks>
    internal class MemoryPool
    {
        /// <summary>
        ///     Pool of extra large byte arrays.
        /// </summary>
        private ObjectPool<byte[]> extraLargePool;

        /// <summary>
        ///     The minimum number of bytes in an extra large pool.
        /// </summary>
        public int ExtraLargeSize { get; }

        /// <summary>
        ///     Pool of large byte arrays.
        /// </summary>
        private ObjectPool<byte[]> largePool;
        
        /// <summary>
        ///     The minimum number of bytes in a large pool.
        /// </summary>
        public int LargeSize { get; }

        /// <summary>
        ///     Pool of medium byte arrays.
        /// </summary>
        private ObjectPool<byte[]> mediumPool;

        /// <summary>
        ///     The minimum number of bytes in a medium pool.
        /// </summary>
        public int MediumSize { get; }

        /// <summary>
        ///     Pool of small byte arrays.
        /// </summary>
        private ObjectPool<byte[]> smallPool;

        /// <summary>
        ///     The minimum number of bytes in a small pool.
        /// </summary>
        public int SmallSize { get; }

        /// <summary>
        ///     Pool of extra small byte arrays.
        /// </summary>
        private ObjectPool<byte[]> extraSmallPool;

        /// <summary>
        ///     The minimum number of bytes in an extra small pool.
        /// </summary>
        public int ExtraSmallSize { get; }

        public MemoryPool(int extraSmallSize, int maxExtraSmall, int smallSize, int maxSmall, int mediumSize, int maxMedium, int largeSize, int maxLarge, int extraLargeSize, int maxExtraLarge)
        {
            this.ExtraSmallSize = extraSmallSize;
            this.SmallSize = smallSize;
            this.MediumSize = mediumSize;
            this.LargeSize = largeSize;
            this.ExtraLargeSize = extraLargeSize;

            this.extraSmallPool = new ObjectPool<byte[]>(maxExtraSmall, () => new byte[ExtraSmallSize]);
            this.smallPool = new ObjectPool<byte[]>(maxSmall, () => new byte[SmallSize]);
            this.mediumPool = new ObjectPool<byte[]>(maxMedium, () => new byte[MediumSize]);
            this.largePool = new ObjectPool<byte[]>(maxLarge, () => new byte[LargeSize]);
            this.extraLargePool = new ObjectPool<byte[]>(maxExtraLarge, () => new byte[ExtraLargeSize]);
        }

        /// <summary>
        ///     Provides a byte array greater than or equal to the specified size.
        /// </summary>
        /// <param name="minSize">The minimum size of the byte array.</param>
        /// <returns>The new byte array.</returns>
        public byte[] GetInstance(int minSize)
        {
            if (minSize <= ExtraSmallSize)
                return extraSmallPool.GetInstance();
            else if (minSize <= SmallSize)
                return smallPool.GetInstance();
            else if (minSize <= MediumSize)
                return mediumPool.GetInstance();
            else if (minSize <= LargeSize)
                return largePool.GetInstance();
            else if (minSize <= ExtraLargeSize)
                return extraLargePool.GetInstance();
            else
                return new byte[minSize];
        }

        /// <summary>
        ///     Returns a byte array back to the pool.
        /// </summary>
        /// <param name="buffer">The byte array to return.</param>
        public void ReturnInstance(byte[] buffer)
        {
            if (buffer.Length >= ExtraLargeSize)
                extraLargePool.ReturnInstance(buffer);
            else if (buffer.Length >= LargeSize)
                largePool.ReturnInstance(buffer);
            else if (buffer.Length >= MediumSize)
                mediumPool.ReturnInstance(buffer);
            else if (buffer.Length >= SmallSize)
                smallPool.ReturnInstance(buffer);
            else if (buffer.Length >= ExtraSmallSize)
                extraSmallPool.ReturnInstance(buffer);
            
            //Else ignore and let the GC deal with it
        }
    }
}
