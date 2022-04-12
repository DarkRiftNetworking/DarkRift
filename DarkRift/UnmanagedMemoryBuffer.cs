/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace DarkRift
{
    /// <summary>
    ///     Provides an implementation of <see cref="IMessageBuffer"/> for arrays not managed by DarkRift.
    /// </summary>
    internal sealed class UnmanagedMemoryBuffer : IMessageBuffer
    {
        /// <inheritdoc/>
        public byte[] Buffer { get; }

        /// <inheritdoc/>
        public int Count { get; set; }

        /// <inheritdoc/>
        public int Offset { get; set;  }

        /// <summary>
        ///     Creates an <see cref="IMessageBuffer"/> for arrays not managed by DarkRift.
        /// </summary>
        /// <param name="buffer">The buffer to wrap.</param>
        /// <param name="offset">The offset to wrap.</param>
        /// <param name="count">The count to wrap.</param>
        public UnmanagedMemoryBuffer(byte[] buffer, int offset, int count)
        {
            this.Buffer = buffer;
            this.Offset = offset;
            this.Count = count;
        }

        /// <inheritdoc/>
        public IMessageBuffer Clone()
        {
            // For an unmanaged arrray we don't need to actually clone this or change reference counts. We're read only so can just return ourselves.
            return this;
        }

        #region IDisposable Support

        // Nothing to dispose of here!
        
        public void Dispose()
        {

        }

        #endregion
    }
}
