/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace DarkRift
{
    /// <summary>
    ///     Reference counted array type that automatically recycles of itself when it has no references.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    // TODO tie ARA in 1-1 relationship with memory so we can count finalizes and can't accidently mess up references
    internal class AutoRecyclingArray
    {
        /// <summary>
        ///     The current backing array.
        /// </summary>
        public byte[] Buffer { get; private set; }

        /// <summary>
        ///     The number of references currently held to this array.
        /// </summary>
        private int referenceCount;

        /// <summary>
        ///     Whether this array is currently in an object pool waiting or not.
        /// </summary>
        private volatile bool isCurrentlyLoungingInAPool;

        /// <summary>
        ///     Gets an instance of an <see cref="AutoRecyclingArray"/>.
        /// </summary>
        /// <param name="minLength">The minimumn number of bytes to start with.</param>
        /// <returns>The array.</returns>
        // TODO DR3 use Caller Information Attributes to improve debugging
        internal static AutoRecyclingArray Create(int minLength)
        {
            AutoRecyclingArray array = ObjectCache.GetAutoRecyclingArray();

            array.isCurrentlyLoungingInAPool = false;
            array.Buffer = ObjectCache.GetMemory(minLength);
            Interlocked.Exchange(ref array.referenceCount, 1);

            return array;
        }

        /// <summary>
        ///     Creates an empty auto recycling array.
        /// </summary>
        internal AutoRecyclingArray()
        {

        }

        /// <summary>
        ///     Creates an auto recycling array around the given buffer.
        /// </summary>
        /// <param name="buffer">The nuffer to wrap</param>
        internal AutoRecyclingArray(byte[] buffer)
        {
            Buffer = buffer;
            referenceCount = 1;
        }

        /// <summary>
        ///     Marks that a new reference to this array has been created.
        /// </summary>
        public void IncrementReference()
        {
#if DEBUG
            // Extra guard to make debugging leaks easier
            if (isCurrentlyLoungingInAPool)
                throw new InvalidOperationException();
#endif

            Interlocked.Increment(ref referenceCount);
        }

        /// <summary>
        ///     Marks that a reference to this array has been removed and disposes if there are no more references.
        /// </summary>
        public void DecrementReference()
        {
#if DEBUG
            // Extra guard to make debugging leaks easier
            if (isCurrentlyLoungingInAPool)
                throw new InvalidOperationException();
#endif

            int newRefCount = Interlocked.Decrement(ref referenceCount);

            if (newRefCount == 0)
            {
                // When we recycle the memory set it to null so we can't accidently reuse it next time!
                ObjectCache.ReturnMemory(Buffer);
                Buffer = null;

                ObjectCache.ReturnAutoRecyclingArray(this);
                isCurrentlyLoungingInAPool = true;
            }
        }

        /// <summary>
        ///     Finalizer so we can inform the cache system we were not recycled correctly.
        /// </summary>
        ~AutoRecyclingArray()
        {
            if (!isCurrentlyLoungingInAPool)
                ObjectCacheHelper.AutoRecyclingArrayWasFinalized();
        }
    }
}
