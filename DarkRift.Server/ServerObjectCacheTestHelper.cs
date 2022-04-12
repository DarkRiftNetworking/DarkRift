/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading;

namespace DarkRift.Server
{
#if DEBUG
    /// <summary>
    ///     Helper for tests to assert recycling is carried out correctly.
    /// </summary>
    public class ServerObjectCacheTestHelper
    {
        /// <summary>
        ///     The number of <see cref="MessageReceivedEventArgs"/> objects that were retrieved from the cache.
        /// </summary>
        public static int RetrievedMessageReceivedEventArgs => retrievedMessageReceivedEventArgs;

        private static int retrievedMessageReceivedEventArgs = 0;

        /// <summary>
        ///     The number of <see cref="MessageReceivedEventArgs"/> objects that were returned to the cache.
        /// </summary>
        public static int ReturnedMessageReceivedEventArgs => returnedMessageReceivedEventArgs;

        private static int returnedMessageReceivedEventArgs = 0;

#if PRO
        /// <summary>
        ///     The number of <see cref="ServerMessageReceivedEventArgs"/> objects that were retrieved from the cache.
        /// </summary>
        public static int RetrievedServerMessageReceivedEventArgs => retrievedServerMessageReceivedEventArgs;

        private static int retrievedServerMessageReceivedEventArgs = 0;

        /// <summary>
        ///     The number of <see cref="ServerMessageReceivedEventArgs"/> objects that were returned to the cache.
        /// </summary>
        public static int ReturnedServerMessageReceivedEventArgs => returnedServerMessageReceivedEventArgs;

        private static int returnedServerMessageReceivedEventArgs = 0;
#endif

        /// <summary>
        ///     Indcates an <see cref="MessageReceivedEventArgs"/> was retrieved from the cache.
        /// </summary>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void MessageReceivedEventArgsWasRetrieved()
        {
            Interlocked.Increment(ref retrievedMessageReceivedEventArgs);
        }

        /// <summary>
        ///     Indcates an <see cref="MessageReceivedEventArgs"/> was returned to the cache.
        /// </summary>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void MessageReceivedEventArgsWasReturned()
        {
            Interlocked.Increment(ref returnedMessageReceivedEventArgs);
        }

#if PRO
        /// <summary>
        ///     Indcates an <see cref="ServerMessageReceivedEventArgs"/> was retrieved from the cache.
        /// </summary>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void ServerMessageReceivedEventArgsWasRetrieved()
        {
            Interlocked.Increment(ref retrievedServerMessageReceivedEventArgs);
        }

        /// <summary>
        ///     Indcates an <see cref="ServerMessageReceivedEventArgs"/> was returned to the cache.
        /// </summary>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void ServerMessageReceivedEventArgsWasReturned()
        {
            Interlocked.Increment(ref returnedServerMessageReceivedEventArgs);
        }
#endif

        /// <summary>
        ///     Resets all metrics recorded.
        /// </summary>
        public static void ResetCounters() {
            Interlocked.Exchange(ref retrievedMessageReceivedEventArgs, 0);
            Interlocked.Exchange(ref returnedMessageReceivedEventArgs, 0);

#if PRO
            Interlocked.Exchange(ref retrievedServerMessageReceivedEventArgs, 0);
            Interlocked.Exchange(ref returnedServerMessageReceivedEventArgs, 0);
#endif
        }
    }
#endif
}
