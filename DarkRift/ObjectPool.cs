/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.CompilerServices;


namespace DarkRift
{
    /// <summary>
    ///     Generic pool of objects for implementing recycling.
    /// </summary>
    /// <remarks>
    ///     This object is not thread safe as it is intended to be used with the ThreadStatic attribute.
    /// </remarks>
    internal class ObjectPool<T>
    {
        /// <summary>
        ///     The maximum number of objects allowed in this pool per thread.
        /// </summary>
        public int MaxObjects { get; }

        /// <summary>
        ///     The number of objects currently pooled in this thread's pool.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        ///     The function to generate a new isntance of T.
        /// </summary>
        private readonly Func<T> generate;

        /// <summary>
        ///     The pool of pooled objects.
        /// </summary>
        private readonly T[] pool;

        /// <summary>
        ///     Creates a new object pool.
        /// </summary>
        /// <param name="maxObjects">The maximum number of elements to store per thread.</param>
        /// <param name="generate">The function that will be invoked to generate a new instance. This must be thread safe.</param>
        public ObjectPool(int maxObjects, Func<T> generate)
        {
            this.MaxObjects = maxObjects;
            this.generate = generate;

            pool = new T[maxObjects];
        }

        /// <summary>
        ///     Provides an instance of the class.
        /// </summary>
        /// <returns></returns>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public T GetInstance()
        {
            if (Count > 0)
                return pool[--Count];
            else
                return generate();
        }

        /// <summary>
        ///     Returns an instance of the class to the pool.
        /// </summary>
        /// <param name="t">The instance to return.</param>
        /// <returns>Whether the instance was returned <c>true</c> or should be disposed of <c>false</c>.</returns>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool ReturnInstance(T t)
        {
            if (Count < MaxObjects)
            {
                pool[Count++] = t;

                return true;
            }

            return false;
        }
    }
}
