/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DarkRift.Server
{
    /// <summary>
    ///     Helper class for the server's object cache.
    /// </summary>
    //DR3 Make static
    public class ServerObjectCacheHelper
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
        ///
        ///     This method will also initialize the <see cref="ObjectCache"/>.
        /// </remarks>
        /// <param name="settings"></param>
        //DR3 Make static
        public void InitializeObjectCache(ServerObjectCacheSettings settings)
        {
            ObjectCache.Initialize(settings);
            ServerObjectCache.Initialize(settings);
        }

        /// <summary>
        ///     The number of <see cref="MessageReceivedEventArgs"/> objects that were not recycled properly.
        /// </summary>
        public static int FinalizedMessageReceivedEventArgs => finalizedMessageReceivedEventArgs;

        private static int finalizedMessageReceivedEventArgs = 0;

#if PRO
        /// <summary>
        ///     The number of <see cref="ServerMessageReceivedEventArgs"/> objects that were not recycled properly.
        /// </summary>
        public static int FinalizedServerMessageReceivedEventArgs => finalizedServerMessageReceivedEventArgs;

        private static int finalizedServerMessageReceivedEventArgs = 0;
#endif

        /// <summary>
        ///     Indcates an <see cref="MessageReceivedEventArgs"/> did not get recycled properly.
        /// </summary>
        internal static void MessageReceivedEventArgsWasFinalized()
        {
            Interlocked.Increment(ref finalizedMessageReceivedEventArgs);
        }

#if PRO
        /// <summary>
        ///     Indcates an <see cref="ServerMessageReceivedEventArgs"/> did not get recycled properly.
        /// </summary>
        internal static void ServerMessageReceivedEventArgsWasFinalized()
        {
            Interlocked.Increment(ref finalizedServerMessageReceivedEventArgs);
        }
#endif

        /// <summary>
        ///     Resets all counters to 0.
        /// </summary>
        public static void ResetCounters()
        {
            Interlocked.Exchange(ref finalizedMessageReceivedEventArgs, 0);
#if PRO
            Interlocked.Exchange(ref finalizedServerMessageReceivedEventArgs, 0);
#endif
        }
    }
}
