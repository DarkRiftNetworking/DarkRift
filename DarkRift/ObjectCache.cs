/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace DarkRift
{
    /// <summary>
    ///     A cache of DarkRift objects for recycling.
    /// </summary>
    /// <remarks>
    ///     Must be initialized on the thread using DarkRiftServer.InitializeObjectCache() or will throw errors.
    /// </remarks>
    internal static class ObjectCache
    {
        /// <summary>
        ///     Whether this cache has been initialized yet.
        /// </summary>
        [ThreadStatic]
        private static bool initialized;

        /// <summary>
        ///     Object pool of <see cref="DarkRiftWriter"/> objects.
        /// </summary>
        [ThreadStatic]
        private static ObjectPool<DarkRiftWriter> writerPool;

        /// <summary>
        ///     Object pool of <see cref="DarkRiftReader"/> objects.
        /// </summary>
        [ThreadStatic]
        private static ObjectPool<DarkRiftReader> readerPool;

        /// <summary>
        ///     Object pool of <see cref="Message"/> objects.
        /// </summary>
        [ThreadStatic]
        private static ObjectPool<Message> messagePool;

        /// <summary>
        ///     Object pool of <see cref="MessageBuffer"/> objects.
        /// </summary>
        [ThreadStatic]
        private static ObjectPool<MessageBuffer> messageBufferPool;

        /// <summary>
        ///     Object pool of <see cref="SocketAsyncEventArgs"/> objects.
        /// </summary>
        [ThreadStatic]
        private static ObjectPool<SocketAsyncEventArgs> socketAsyncEventArgsPool;

        /// <summary>
        ///     Object pool of <see cref="ActionDispatcherTask"/> objects.
        /// </summary>
        [ThreadStatic]
        private static ObjectPool<ActionDispatcherTask> actionDispatcherTaskPool;

        /// <summary>
        ///     Object pool of <see cref="AutoRecyclingArray"/> objects.
        /// </summary>
        [ThreadStatic]
        private static ObjectPool<AutoRecyclingArray> autoRecyclingArrayPool;

        /// <summary>
        ///     Pool of byte arrays.
        /// </summary>
        [ThreadStatic]
        private static MemoryPool memoryPool;

        /// <summary>
        ///     The settings for all object caches.
        /// </summary>
        private static ObjectCacheSettings settings;

        /// <summary>
        ///     The lock for the settings field.
        /// </summary>
        private static readonly object settingsLock = new object();

        /// <summary>
        ///     Sets up the ObjectCache with the given settings.
        /// </summary>
        /// <returns>True if the object cache was set with the sepcified settings, false if it is already initialized.</returns>
        public static bool Initialize(ObjectCacheSettings settings)
        {
            lock (settingsLock)
            {
                if (ObjectCache.settings == null)
                {
                    ObjectCache.settings = settings;
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
                writerPool = new ObjectPool<DarkRiftWriter>(settings.MaxWriters, () => new DarkRiftWriter());
                readerPool = new ObjectPool<DarkRiftReader>(settings.MaxReaders, () => new DarkRiftReader());
                messagePool = new ObjectPool<Message>(settings.MaxMessages, () => new Message());
                messageBufferPool = new ObjectPool<MessageBuffer>(settings.MaxMessageBuffers, () => new MessageBuffer());
                socketAsyncEventArgsPool = new ObjectPool<SocketAsyncEventArgs>(settings.MaxSocketAsyncEventArgs, () => new SocketAsyncEventArgs());
                actionDispatcherTaskPool = new ObjectPool<ActionDispatcherTask>(settings.MaxActionDispatcherTasks, () => new ActionDispatcherTask());
                autoRecyclingArrayPool = new ObjectPool<AutoRecyclingArray>(settings.MaxAutoRecyclingArrays, () => new AutoRecyclingArray());
                memoryPool = new MemoryPool(
                    settings.ExtraSmallMemoryBlockSize,
                    settings.MaxExtraSmallMemoryBlocks,
                    settings.SmallMemoryBlockSize,
                    settings.MaxSmallMemoryBlocks,
                    settings.MediumMemoryBlockSize,
                    settings.MaxMediumMemoryBlocks,
                    settings.LargeMemoryBlockSize,
                    settings.MaxLargeMemoryBlocks,
                    settings.ExtraLargeMemoryBlockSize,
                    settings.MaxExtraLargeMemoryBlocks
                );
            }

            initialized = true;
        }

        /// <summary>
        ///     Returns a pooled <see cref="DarkRiftWriter"/> or generates a new one if there are none available.
        /// </summary>
        /// <returns>A free <see cref="DarkRiftWriter"/>.</returns>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static DarkRiftWriter GetWriter()
        {
            if (!initialized)
                ThreadInitialize();

#if DEBUG
            ObjectCacheTestHelper.DarkRiftWriterWasRetrieved();
#endif

            return writerPool.GetInstance();
        }

        /// <summary>
        ///     Returns a used <see cref="DarkRiftWriter"/> to the pool.
        /// </summary>
        /// <param name="writer">The <see cref="DarkRiftWriter"/> to return.</param>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ReturnWriter(DarkRiftWriter writer)
        {
            if (!initialized)
                ThreadInitialize();

#if DEBUG
            ObjectCacheTestHelper.DarkRiftWriterWasReturned();
#endif
            writerPool.ReturnInstance(writer);
        }

        /// <summary>
        ///     Returns a pooled <see cref="DarkRiftReader"/> or generates a new one if there are none available.
        /// </summary>
        /// <returns>A free <see cref="DarkRiftReader"/>.</returns>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static DarkRiftReader GetReader()
        {
            if (!initialized)
                ThreadInitialize();

#if DEBUG
            ObjectCacheTestHelper.DarkRiftReaderWasRetrieved();
#endif
            return readerPool.GetInstance();
        }

        /// <summary>
        ///     Returns a used <see cref="DarkRiftReader"/> to the pool.
        /// </summary>
        /// <param name="reader">The <see cref="DarkRiftReader"/> to return.</param>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ReturnReader(DarkRiftReader reader)
        {
            if (!initialized)
                ThreadInitialize();

#if DEBUG
            ObjectCacheTestHelper.DarkRiftReaderWasReturned();
#endif

            readerPool.ReturnInstance(reader);
        }

        /// <summary>
        ///     Returns a pooled <see cref="Message"/> or generates a new one if there are none available.
        /// </summary>
        /// <returns>A free <see cref="Message"/>.</returns>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Message GetMessage()
        {
            if (!initialized)
                ThreadInitialize();

#if DEBUG
            ObjectCacheTestHelper.MessageWasRetrieved();
#endif

            return messagePool.GetInstance();
        }

        /// <summary>
        ///     Returns a used <see cref="Message"/> to the pool.
        /// </summary>
        /// <param name="message">The <see cref="Message"/> to return.</param>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ReturnMessage(Message message)
        {
            if (!initialized)
                ThreadInitialize();

#if DEBUG
            ObjectCacheTestHelper.MessageWasReturned();
#endif

            messagePool.ReturnInstance(message);
        }

        /// <summary>
        ///     Returns a pooled <see cref="MessageBuffer"/> or generates a new one if there are none available.
        /// </summary>
        /// <returns>A free <see cref="MessageBuffer"/>.</returns>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static MessageBuffer GetMessageBuffer()
        {
            if (!initialized)
                ThreadInitialize();

#if DEBUG
            ObjectCacheTestHelper.MessageBufferWasRetrieved();
#endif

            return messageBufferPool.GetInstance();
        }

        /// <summary>
        ///     Returns a used <see cref="MessageBuffer"/> to the pool.
        /// </summary>
        /// <param name="messageBuffer">The <see cref="MessageBuffer"/> to return.</param>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ReturnMessageBuffer(MessageBuffer messageBuffer)
        {
            if (!initialized)
                ThreadInitialize();

#if DEBUG
            ObjectCacheTestHelper.MessageBufferWasReturned();
#endif

            messageBufferPool.ReturnInstance(messageBuffer);
        }

        /// <summary>
        ///     Returns a pooled <see cref="SocketAsyncEventArgs"/> object or generates a new one if there are none available.
        /// </summary>
        /// <returns>A free <see cref="SocketAsyncEventArgs"/> object.</returns>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static SocketAsyncEventArgs GetSocketAsyncEventArgs()
        {
            if (!initialized)
                ThreadInitialize();

#if DEBUG
            ObjectCacheTestHelper.SocketAsyncEventArgsWasRetrieved();
#endif

            return socketAsyncEventArgsPool.GetInstance();
        }

        /// <summary>
        ///     Returns a used <see cref="SocketAsyncEventArgs"/> object to the pool or disposes of it if there are already enough.
        /// </summary>
        /// <param name="socketAsyncEventArgs">The <see cref="SocketAsyncEventArgs"/> object to return.</param>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ReturnSocketAsyncEventArgs(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            if (!initialized)
                ThreadInitialize();

#if DEBUG
            ObjectCacheTestHelper.SocketAsyncEventArgsWasReturned();
#endif

            if (!socketAsyncEventArgsPool.ReturnInstance(socketAsyncEventArgs))
                socketAsyncEventArgs.Dispose();
        }

        /// <summary>
        ///     Returns a pooled <see cref="ActionDispatcherTask"/> or generates a new one if there are none available.
        /// </summary>
        /// <returns>A free <see cref="ActionDispatcherTask"/>.</returns>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ActionDispatcherTask GetActionDispatcherTask()
        {
            if (!initialized)
                ThreadInitialize();

#if DEBUG
            ObjectCacheTestHelper.ActionDispatcherTaskWasRetrieved();
#endif
            return actionDispatcherTaskPool.GetInstance();
        }

        /// <summary>
        ///     Returns a used <see cref="ActionDispatcherTask"/> to the pool.
        /// </summary>
        /// <param name="task">The <see cref="ActionDispatcherTask"/> to return.</param>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ReturnActionDispatcherTask(ActionDispatcherTask task)
        {
            if (!initialized)
                ThreadInitialize();

#if DEBUG
            ObjectCacheTestHelper.ActionDispatcherTaskWasReturned();
#endif

            if (!actionDispatcherTaskPool.ReturnInstance(task))
                task.ActuallyDispose();
        }

        /// <summary>
        ///     Returns a pooled <see cref="AutoRecyclingArray"/> object or generates a new one if there are none available.
        /// </summary>
        /// <returns>A free <see cref="AutoRecyclingArray"/> object.</returns>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static AutoRecyclingArray GetAutoRecyclingArray()
        {
            if (!initialized)
                ThreadInitialize();

#if DEBUG
            ObjectCacheTestHelper.AutoRecyclingArrayWasRetrieved();
#endif

            return autoRecyclingArrayPool.GetInstance();
        }

        /// <summary>
        ///     Returns a used <see cref="AutoRecyclingArray"/> object to the pool or disposes of it if there are already enough.
        /// </summary>
        /// <param name="autoRecyclingArray">The <see cref="AutoRecyclingArray"/> object to return.</param>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ReturnAutoRecyclingArray(AutoRecyclingArray autoRecyclingArray)
        {
            if (!initialized)
                ThreadInitialize();

#if DEBUG
            ObjectCacheTestHelper.AutoRecyclingArrayWasReturned();
#endif

            autoRecyclingArrayPool.ReturnInstance(autoRecyclingArray);
        }

        /// <summary>
        ///     Returns a pooled byte array or allocates a new one if there are none available.
        /// </summary>
        /// <param name="minLength">The minimum length of memory to allocate.</param>
        /// <returns>A free byte array of sufficient size.</returns>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetMemory(int minLength)
        {
            if (!initialized)
                ThreadInitialize();

#if DEBUG
            ObjectCacheTestHelper.MemoryWasRetrieved();
#endif

            return memoryPool.GetInstance(minLength);
        }

        /// <summary>
        ///     Returns a used byte array to the pool or disposes of it if there are already enough.
        /// </summary>
        /// <param name="memory">The byte array to return.</param>
#if INLINE_CACHE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ReturnMemory(byte[] memory)
        {
            if (!initialized)
                ThreadInitialize();

#if DEBUG
            ObjectCacheTestHelper.MemoryWasReturned();
#endif

            memoryPool.ReturnInstance(memory);
        }
    }
}
