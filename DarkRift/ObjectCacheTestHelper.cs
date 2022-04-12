/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DarkRift
{
#if DEBUG
    /// <summary>
    ///     Helper for tests to assert recycling is carried out correctly.
    /// </summary>
    public static class ObjectCacheTestHelper
    {
        /// <summary>
        ///     The number of <see cref="AutoRecyclingArray"/> objects that were retrieved from the cache.
        /// </summary>
        public static int RetrievedAutoRecyclingArrays => retrievedAutoRecyclingArrays;

        private static int retrievedAutoRecyclingArrays = 0;

        /// <summary>
        ///     The number of <see cref="DarkRiftReader"/> objects that were retrieved from the cache.
        /// </summary>
        public static int RetrievedDarkRiftReaders => retrievedDarkRiftReaders;

        private static int retrievedDarkRiftReaders = 0;

        /// <summary>
        ///     The number of <see cref="DarkRiftWriter"/> objects that were retrieved from the cache.
        /// </summary>
        public static int RetrievedDarkRiftWriters => retrievedDarkRiftWriters;

        private static int retrievedDarkRiftWriters = 0;

        /// <summary>
        ///     The number of <see cref="Message"/> objects that were retrieved from the cache.
        /// </summary>
        public static int RetrievedMessages => retrievedMessages;

        private static int retrievedMessages = 0;

        /// <summary>
        ///     The number of <see cref="MessageBuffer"/> objects that were retrieved from the cache.
        /// </summary>
        public static int RetrievedMessageBuffers => retrievedMessageBuffers;

        private static int retrievedMessageBuffers = 0;

        /// <summary>
        ///     The number of <see cref="SocketAsyncEventArgs"/> objects that were retrieved from the cache.
        /// </summary>
        public static int RetrievedSocketAsyncEventArgs => retrievedSocketAsyncEventArgs;

        private static int retrievedSocketAsyncEventArgs = 0;

        /// <summary>
        ///     The number of <see cref="Dispatching.ActionDispatcherTask"/> objects that were retrieved from the cache.
        /// </summary>
        public static int RetrievedActionDispatcherTasks => retrievedActionDispatcherTasks;

        private static int retrievedActionDispatcherTasks = 0;

        /// <summary>
        ///     The number of memory segments that were retrieved from the cache.
        /// </summary>
        public static int RetrievedMemory => retrievedMemory;

        private static int retrievedMemory = 0;

        /// <summary>
        ///     The number of <see cref="AutoRecyclingArray"/> objects that were returned to the cache.
        /// </summary>
        public static int ReturnedAutoRecyclingArrays => returnedAutoRecyclingArrays;

        private static int returnedAutoRecyclingArrays = 0;

        /// <summary>
        ///     The number of <see cref="DarkRiftReader"/> objects that were returned to the cache.
        /// </summary>
        public static int ReturnedDarkRiftReaders => returnedDarkRiftReaders;

        private static int returnedDarkRiftReaders = 0;

        /// <summary>
        ///     The number of <see cref="DarkRiftWriter"/> objects that were returned to the cache.
        /// </summary>
        public static int ReturnedDarkRiftWriters => returnedDarkRiftWriters;

        private static int returnedDarkRiftWriters = 0;

        /// <summary>
        ///     The number of <see cref="Message"/> objects that were returned to the cache.
        /// </summary>
        public static int ReturnedMessages => returnedMessages;

        private static int returnedMessages = 0;

        /// <summary>
        ///     The number of <see cref="MessageBuffer"/> objects that were returned to the cache.
        /// </summary>
        public static int ReturnedMessageBuffers => returnedMessageBuffers;

        private static int returnedMessageBuffers = 0;

        /// <summary>
        ///     The number of <see cref="SocketAsyncEventArgs"/> objects that were returned to the cache.
        /// </summary>
        public static int ReturnedSocketAsyncEventArgs => returnedSocketAsyncEventArgs;

        private static int returnedSocketAsyncEventArgs = 0;

        /// <summary>
        ///     The number of <see cref="Dispatching.ActionDispatcherTask"/> objects that were returned to the cache.
        /// </summary>
        public static int ReturnedActionDispatcherTasks => returnedActionDispatcherTasks;

        private static int returnedActionDispatcherTasks = 0;

        /// <summary>
        ///     The number of memory segments that were returned to the cache.
        /// </summary>
        public static int ReturnedMemory => returnedMemory;

        private static int returnedMemory = 0;

        /// <summary>
        ///     Indcates an <see cref="AutoRecyclingArray"/> was retrieved from the cache.
        /// </summary>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void AutoRecyclingArrayWasRetrieved()
        {
            Interlocked.Increment(ref retrievedAutoRecyclingArrays);
        }

        /// <summary>
        ///     Indcates an <see cref="DarkRiftReader"/> was retrieved from the cache.
        /// </summary>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void DarkRiftReaderWasRetrieved()
        {
            Interlocked.Increment(ref retrievedDarkRiftReaders);
        }

        /// <summary>
        ///     Indcates an <see cref="DarkRiftWriter"/> was retrieved from the cache.
        /// </summary>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void DarkRiftWriterWasRetrieved()
        {
            Interlocked.Increment(ref retrievedDarkRiftWriters);
        }

        /// <summary>
        ///     Indcates an <see cref="Message"/> was retrieved from the cache.
        /// </summary>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void MessageWasRetrieved()
        {
            Interlocked.Increment(ref retrievedMessages);
        }

        /// <summary>
        ///     Indcates an <see cref="MessageBuffer"/> was retrieved from the cache.
        /// </summary>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void MessageBufferWasRetrieved()
        {
            Interlocked.Increment(ref retrievedMessageBuffers);
        }

        /// <summary>
        ///     Indcates a <see cref="SocketAsyncEventArgs"/> object was retrieved from the cache.
        /// </summary>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void SocketAsyncEventArgsWasRetrieved()
        {
            Interlocked.Increment(ref retrievedSocketAsyncEventArgs);
        }

        /// <summary>
        ///     Indcates an <see cref="Dispatching.ActionDispatcherTask"/> was retrieved from the cache.
        /// </summary>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void ActionDispatcherTaskWasRetrieved()
        {
            Interlocked.Increment(ref retrievedActionDispatcherTasks);
        }

        /// <summary>
        ///     Indcates a memory segment was retrieved from the cache.
        /// </summary>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void MemoryWasRetrieved()
        {
            Interlocked.Increment(ref retrievedMemory);
        }

        /// <summary>
        ///     Indcates an <see cref="AutoRecyclingArray"/> was returned to the cache.
        /// </summary>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void AutoRecyclingArrayWasReturned()
        {
            Interlocked.Increment(ref returnedAutoRecyclingArrays);
        }

        /// <summary>
        ///     Indcates an <see cref="DarkRiftReader"/> was returned to the cache.
        /// </summary>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void DarkRiftReaderWasReturned()
        {
            Interlocked.Increment(ref returnedDarkRiftReaders);
        }

        /// <summary>
        ///     Indcates an <see cref="DarkRiftWriter"/> was returned to the cache.
        /// </summary>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void DarkRiftWriterWasReturned()
        {
            Interlocked.Increment(ref returnedDarkRiftWriters);
        }

        /// <summary>
        ///     Indcates an <see cref="Message"/> was returned to the cache.
        /// </summary>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void MessageWasReturned()
        {
            Interlocked.Increment(ref returnedMessages);
        }

        /// <summary>
        ///     Indcates an <see cref="MessageBuffer"/> was returned to the cache.
        /// </summary>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void MessageBufferWasReturned()
        {
            Interlocked.Increment(ref returnedMessageBuffers);
        }


        /// <summary>
        ///     Indcates an <see cref="MessageBuffer"/> was returned to the cache.
        /// </summary>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void SocketAsyncEventArgsWasReturned()
        {
            Interlocked.Increment(ref returnedSocketAsyncEventArgs);
        }

        /// <summary>
        ///     Indcates an <see cref="Dispatching.ActionDispatcherTask"/> was returned to the cache.
        /// </summary>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void ActionDispatcherTaskWasReturned()
        {
            Interlocked.Increment(ref returnedActionDispatcherTasks);
        }

        /// <summary>
        ///     Indcates a memory segment was returned to the cache.
        /// </summary>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void MemoryWasReturned()
        {
            Interlocked.Increment(ref returnedMemory);
        }

        /// <summary>
        ///     Resets all metrics recorded.
        /// </summary>
        public static void ResetCounters()
        {
            Interlocked.Exchange(ref retrievedAutoRecyclingArrays, 0);
            Interlocked.Exchange(ref retrievedDarkRiftWriters, 0);
            Interlocked.Exchange(ref retrievedDarkRiftReaders, 0);
            Interlocked.Exchange(ref retrievedMessages, 0);
            Interlocked.Exchange(ref retrievedMessageBuffers, 0);
            Interlocked.Exchange(ref retrievedSocketAsyncEventArgs, 0);
            Interlocked.Exchange(ref retrievedActionDispatcherTasks, 0);
            Interlocked.Exchange(ref retrievedMemory, 0);

            Interlocked.Exchange(ref returnedAutoRecyclingArrays, 0);
            Interlocked.Exchange(ref returnedDarkRiftWriters, 0);
            Interlocked.Exchange(ref returnedDarkRiftReaders, 0);
            Interlocked.Exchange(ref returnedMessages, 0);
            Interlocked.Exchange(ref returnedMessageBuffers, 0);
            Interlocked.Exchange(ref returnedSocketAsyncEventArgs, 0);
            Interlocked.Exchange(ref returnedActionDispatcherTasks, 0);
            Interlocked.Exchange(ref returnedMemory, 0);
        }
    }
#endif
}
