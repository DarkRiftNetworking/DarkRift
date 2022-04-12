/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;

namespace DarkRift.Client
{
    internal class ClientObjectCache
    {
        /// <summary>
        ///     Whether this cache has been initialized yet.
        /// </summary>
        [ThreadStatic]
        private static bool initialized;

        /// <summary>
        ///     Object pool of <see cref="MessageReceivedEventArgs"/> objects.
        /// </summary>
        [ThreadStatic]
        private static ObjectPool<MessageReceivedEventArgs> messageReceivedEventArgsPool;

        /// <summary>
        ///     The settings for all object caches.
        /// </summary>
        private static ClientObjectCacheSettings settings;

        /// <summary>
        ///     The lock for the settings field.
        /// </summary>
        private static readonly object settingsLock = new object();

        /// <summary>
        ///     Sets up the ObjectCache with the given settings.
        /// </summary>
        /// <returns>True if the object cache was set with the sepcified settings, false if it is already initialized.</returns>
        public static bool Initialize(ClientObjectCacheSettings settings)
        {
            lock (settingsLock)
            {
                if (ClientObjectCache.settings == null)
                {
                    ClientObjectCache.settings = settings;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Initializes the object cache with the stored settings.
        /// </summary>
        private static void ThreadInitialize()
        {
            lock (settingsLock)
            {
                messageReceivedEventArgsPool = new ObjectPool<MessageReceivedEventArgs>(settings.MaxMessageReceivedEventArgs, () => new MessageReceivedEventArgs());
            }

            initialized = true;
        }

        /// <summary>
        ///     Returns a pooled <see cref="MessageReceivedEventArgs"/> or generates a new one if there are none available.
        /// </summary>
        /// <returns>A free <see cref="MessageReceivedEventArgs"/>.</returns>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static MessageReceivedEventArgs GetMessageReceivedEventArgs()
        {
            if (!initialized)
                ThreadInitialize();

#if DEBUG
            ClientObjectCacheTestHelper.MessageReceivedEventArgsWasRetrieved();
#endif

            return messageReceivedEventArgsPool.GetInstance();
        }

        /// <summary>
        ///     Returns a used <see cref="MessageReceivedEventArgs"/> to the pool.
        /// </summary>
        /// <param name="writer">The <see cref="MessageReceivedEventArgs"/> to return.</param>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ReturnMessageReceivedEventArgs(MessageReceivedEventArgs writer)
        {
            if (!initialized)
                ThreadInitialize();

#if DEBUG
            ClientObjectCacheTestHelper.MessageReceivedEventArgsWasReturned();
#endif
            messageReceivedEventArgsPool.ReturnInstance(writer);
        }
    }
}
