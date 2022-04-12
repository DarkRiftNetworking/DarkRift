/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading;

namespace DarkRift.Client
{
#if DEBUG
    /// <summary>
    ///     Helper for tests to assert recycling is carried out correctly.
    /// </summary>
    public class ClientObjectCacheTestHelper
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

        /// <summary>
        ///     Resets all metrics recorded.
        /// </summary>
        public static void ResetCounters() {
            Interlocked.Exchange(ref retrievedMessageReceivedEventArgs, 0);

            Interlocked.Exchange(ref returnedMessageReceivedEventArgs, 0);
        }
    }
#endif
}
