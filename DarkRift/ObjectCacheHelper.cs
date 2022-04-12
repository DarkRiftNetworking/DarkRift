/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading;

namespace DarkRift
{
    /// <summary>
    ///     Helper class for the object cache.
    /// </summary>
    //DR3 Make static
    public class ObjectCacheHelper
    {
        /// <summary>
        ///     Initializes the object cache.
        /// </summary>
        /// <remarks>
        ///     Normally, initializing the object cache is handled for you when you create a server or client
        ///     however there are times when it is necessary to initialize it without creating a server or client
        ///     such as during testing. This method can be used to initialize the cache in those circumstances.
        ///
        ///     If the cache is already initialized this method will do nothing.
        /// </remarks>
        /// <param name="settings"></param>
        //DR3 Make static
        public void InitializeObjectCache(ObjectCacheSettings settings)
        {
            ObjectCache.Initialize(settings);
        }

        /// <summary>
        ///     The number of <see cref="AutoRecyclingArray"/> objects that were not recycled properly.
        /// </summary>
        public static int FinalizedAutoRecyclingArrays => finalizedAutoRecyclingArrays;

        private static int finalizedAutoRecyclingArrays = 0;

        /// <summary>
        ///     The number of <see cref="DarkRiftReader"/> objects that were not recycled properly.
        /// </summary>
        public static int FinalizedDarkRiftReaders => finalizedDarkRiftReaders;

        private static int finalizedDarkRiftReaders = 0;

        /// <summary>
        ///     The number of <see cref="DarkRiftWriter"/> objects that were not recycled properly.
        /// </summary>
        public static int FinalizedDarkRiftWriters => finalizedDarkRiftWriters;

        private static int finalizedDarkRiftWriters = 0;

        /// <summary>
        ///     The number of <see cref="Message"/> objects that were not recycled properly.
        /// </summary>
        public static int FinalizedMessages => finalizedMessages;

        private static int finalizedMessages = 0;

        /// <summary>
        ///     The number of <see cref="MessageBuffer"/> objects that were not recycled properly.
        /// </summary>
        public static int FinalizedMessageBuffers => finalizedMessageBuffers;

        private static int finalizedMessageBuffers = 0;

        /// <summary>
        ///     The number of <see cref="Dispatching.ActionDispatcherTask"/> objects that were not recycled properly.
        /// </summary>
        public static int FinalizedActionDispatcherTasks => finalizedActionDispatcherTasks;

        private static int finalizedActionDispatcherTasks = 0;

        /// <summary>
        ///     Indcates an <see cref="AutoRecyclingArray"/> did not get recycled properly.
        /// </summary>
        internal static void AutoRecyclingArrayWasFinalized()
        {
            Interlocked.Increment(ref finalizedAutoRecyclingArrays);
        }

        /// <summary>
        ///     Indcates an <see cref="DarkRiftReader"/> did not get recycled properly.
        /// </summary>
        internal static void DarkRiftReaderWasFinalized()
        {
            Interlocked.Increment(ref finalizedDarkRiftReaders);
        }

        /// <summary>
        ///     Indcates an <see cref="DarkRiftWriter"/> did not get recycled properly.
        /// </summary>
        internal static void DarkRiftWriterWasFinalized()
        {
            Interlocked.Increment(ref finalizedDarkRiftWriters);
        }
        /// <summary>
        ///     Indcates an <see cref="Message"/> did not get recycled properly.
        /// </summary>
        internal static void MessageWasFinalized()
        {
            Interlocked.Increment(ref finalizedMessages);
        }
        /// <summary>
        ///     Indcates an <see cref="MessageBuffer"/> did not get recycled properly.
        /// </summary>
        internal static void MessageBufferWasFinalized()
        {
            Interlocked.Increment(ref finalizedMessageBuffers);
        }
        /// <summary>
        ///     Indcates an <see cref="Dispatching.ActionDispatcherTask"/> did not get recycled properly.
        /// </summary>
        internal static void ActionDispatcherTaskWasFinalized()
        {
            Interlocked.Increment(ref finalizedActionDispatcherTasks);
        }

        /// <summary>
        ///     Resets all counters to 0.
        /// </summary>
        public static void ResetCounters()
        {
            Interlocked.Exchange(ref finalizedAutoRecyclingArrays, 0);
            Interlocked.Exchange(ref finalizedDarkRiftReaders, 0);
            Interlocked.Exchange(ref finalizedDarkRiftWriters, 0);
            Interlocked.Exchange(ref finalizedMessages, 0);
            Interlocked.Exchange(ref finalizedMessageBuffers, 0);
            Interlocked.Exchange(ref finalizedActionDispatcherTasks, 0);
        }
    }
}
