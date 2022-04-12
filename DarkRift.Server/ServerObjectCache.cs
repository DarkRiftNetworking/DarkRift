/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;

namespace DarkRift.Server
{
    internal class ServerObjectCache
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

#if PRO
        /// <summary>
        ///     Object pool of <see cref="ServerMessageReceivedEventArgs"/> objects.
        /// </summary>
        [ThreadStatic]
        private static ObjectPool<ServerMessageReceivedEventArgs> serverMessageReceivedEventArgsPool;
#endif

        /// <summary>
        ///     The settings for all object caches.
        /// </summary>
        private static ServerObjectCacheSettings settings;

        /// <summary>
        ///     The lock for the settings field.
        /// </summary>
        private static readonly object settingsLock = new object();

        /// <summary>
        ///     Sets up the ObjectCache with the given settings.
        /// </summary>
        /// <returns>True if the object cache was set with the sepcified settings, false if it is already initialized.</returns>
        public static bool Initialize(ServerObjectCacheSettings settings)
        {
            lock (settingsLock)
            {
                if (ServerObjectCache.settings == null)
                {
                    ServerObjectCache.settings = settings;
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
#if PRO
                serverMessageReceivedEventArgsPool = new ObjectPool<ServerMessageReceivedEventArgs>(settings.MaxServerMessageReceivedEventArgs, () => new ServerMessageReceivedEventArgs());
#endif
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
            ServerObjectCacheTestHelper.MessageReceivedEventArgsWasRetrieved();
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
            ServerObjectCacheTestHelper.MessageReceivedEventArgsWasReturned();
#endif
            messageReceivedEventArgsPool.ReturnInstance(writer);
        }

#if PRO
        /// <summary>
        ///     Returns a pooled <see cref="ServerMessageReceivedEventArgs"/> or generates a new one if there are none available.
        /// </summary>
        /// <returns>A free <see cref="ServerMessageReceivedEventArgs"/>.</returns>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ServerMessageReceivedEventArgs GetServerMessageReceivedEventArgs()
        {
            if (!initialized)
                ThreadInitialize();

#if DEBUG
            ServerObjectCacheTestHelper.ServerMessageReceivedEventArgsWasRetrieved();
#endif

            return serverMessageReceivedEventArgsPool.GetInstance();
        }

        /// <summary>
        ///     Returns a used <see cref="ServerMessageReceivedEventArgs"/> to the pool.
        /// </summary>
        /// <param name="writer">The <see cref="ServerMessageReceivedEventArgs"/> to return.</param>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ReturnServerMessageReceivedEventArgs(ServerMessageReceivedEventArgs writer)
        {
            if (!initialized)
                ThreadInitialize();

#if DEBUG
            ServerObjectCacheTestHelper.ServerMessageReceivedEventArgsWasReturned();
#endif
            serverMessageReceivedEventArgsPool.ReturnInstance(writer);
        }
#endif
    }
}
