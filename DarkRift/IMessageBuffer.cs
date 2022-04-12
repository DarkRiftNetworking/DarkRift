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
    public interface IMessageBuffer : IDisposable
    {
        /// <summary>
        ///     The array containing the data.
        /// </summary>
        byte[] Buffer { get; }
        
        /// <summary>
        ///     The number of bytes of data in the array.
        /// </summary>
        int Count { get; set; }

        /// <summary>
        ///     The offset at which bytes of data start in the array.
        /// </summary>
        int Offset { get; set; }

        /// <summary>
        ///     Create a shallow copy of the <see cref="IMessageBuffer"/> that points to the same memory.
        /// </summary>
        /// <returns>A new <see cref="IMessageBuffer"/> with the same values as this.</returns>
        IMessageBuffer Clone();
    }
}
