/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;

namespace DarkRift
{
    /// <summary>
    ///     Holds raw data related to a message.
    /// </summary>
    /// <remarks>
    ///     On dispose <see cref="Buffer" /> may be recycled, therefore you should only dispose once you have completely 
    ///     finished with the array, else you may find the data is changed seemingly randomly.
    /// </remarks>
    public class MessageBuffer : IMessageBuffer
    {
        /// <summary>
        ///     The array containing the data.
        /// </summary>
        public byte[] Buffer => backingBuffer.Buffer;
        
        /// <summary>
        ///     The offset at which bytes of data start in the array.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        ///     The number of bytes of data in the array.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        ///     Backing array that can automatically dispose correctly.
        /// </summary>
        private AutoRecyclingArray backingBuffer;

        /// <summary>
        ///     Whether this message buffer is currently in an object pool waiting or not.
        /// </summary>
        private volatile bool isCurrentlyLoungingInAPool;

        /// <summary>
        ///     Creates a new message buffer.
        /// </summary>
        internal MessageBuffer()
        {

        }

        /// <summary>
        ///     Creates a new message buffer with a given minimum capacity in the backing buffer.
        /// </summary>
        /// <param name="minCapacity">The minimum number of bytes in the buffer.</param>
        /// <returns>The new message buffer.</returns>
        public static MessageBuffer Create(int minCapacity)
        {
            AutoRecyclingArray array = AutoRecyclingArray.Create(minCapacity);

            MessageBuffer buffer = ObjectCache.GetMessageBuffer();

            buffer.isCurrentlyLoungingInAPool = false;

            buffer.backingBuffer = array;
            buffer.Offset = 0;
            buffer.Count = 0;
            
            return buffer;
        }

        /// <summary>
        ///     Create a shallow copy of the <see cref="MessageBuffer"/> that points to the same memory.
        /// </summary>
        /// <returns>A new <see cref="MessageBuffer"/> with the same values as this.</returns>
        public IMessageBuffer Clone()
        {
            MessageBuffer buffer = ObjectCache.GetMessageBuffer();

            buffer.isCurrentlyLoungingInAPool = false;

            //We're creating a reference to a reference counted object so we need to increment the count
            backingBuffer.IncrementReference();
            buffer.backingBuffer = backingBuffer;

            buffer.Offset = Offset;
            buffer.Count = Count;
            return buffer;
        }
        
        /// <summary>
        ///     Ensures the buffer is greater than or equal to the specified length.
        /// </summary>
        /// <param name="newLength">The desired length.</param>
        internal void EnsureLength(int newLength)
        {
            if (newLength > backingBuffer.Buffer.Length)
            {
                //Get a new buffer and copy over data
                AutoRecyclingArray newBackingBuffer = AutoRecyclingArray.Create(newLength);
                Array.Copy(backingBuffer.Buffer, newBackingBuffer.Buffer, backingBuffer.Buffer.Length);

                // Swap out the buffer
                //We're removing a reference to a reference counted object so we need to decrement the count
                backingBuffer.DecrementReference();
                backingBuffer = newBackingBuffer;
            }
        }

        /// <summary>
        ///     Recycles the backing array behind the message buffer. 
        /// </summary>
        #region IDisposable Support
        public void Dispose()
        {
            //AutoRecyclingArray is reference counted, mark that we're no longer using it
            backingBuffer.DecrementReference();

            //Return to the pool of objects
            ObjectCache.ReturnMessageBuffer(this);
            isCurrentlyLoungingInAPool = true;
        }
        #endregion

        /// <summary>
        ///     Finalizer so we can inform the cache system we were not recycled correctly.
        /// </summary>
        ~MessageBuffer()
        {
            if (!isCurrentlyLoungingInAPool)
                ObjectCacheHelper.MessageBufferWasFinalized();
        }
    }
}
