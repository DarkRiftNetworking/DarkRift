/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkRift
{
    /// <summary>
    ///     Helper class for writing primitives to arrays in big endian format.
    /// </summary>
    public static class BigEndianHelper
    {
        /// <summary>
        ///     Writes the bytes from the short to the destination array at offset.
        /// </summary>
        /// <param name="destination">The array to write to.</param>
        /// <param name="offset">The position of the array to begin writing.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteBytes(byte[] destination, int offset, short value)
        {
            destination[offset]     = (byte)(value >> 8);
            destination[offset + 1] = (byte)value;
        }

        /// <summary>
        ///     Writes the bytes from the unsigned short to the destination array at offset.
        /// </summary>
        /// <param name="destination">The array to write to.</param>
        /// <param name="offset">The position of the array to begin writing.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteBytes(byte[] destination, int offset, ushort value)
        {
            destination[offset]     = (byte)(value >> 8);
            destination[offset + 1] = (byte)value;
        }

        /// <summary>
        ///     Writes the bytes from the int to the destination array at offset.
        /// </summary>
        /// <param name="destination">The array to write to.</param>
        /// <param name="offset">The position of the array to begin writing.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteBytes(byte[] destination, int offset, int value)
        {
            destination[offset]     = (byte)(value >> 24);
            destination[offset + 1] = (byte)(value >> 16);
            destination[offset + 2] = (byte)(value >> 8);
            destination[offset + 3] = (byte)value;
        }

        /// <summary>
        ///     Writes the bytes from the unsigned int to the destination array at offset.
        /// </summary>
        /// <param name="destination">The array to write to.</param>
        /// <param name="offset">The position of the array to begin writing.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteBytes(byte[] destination, int offset, uint value)
        {
            destination[offset]     = (byte)(value >> 24);
            destination[offset + 1] = (byte)(value >> 16);
            destination[offset + 2] = (byte)(value >> 8);
            destination[offset + 3] = (byte)value;
        }

        /// <summary>
        ///     Writes the bytes from the long to the destination array at offset.
        /// </summary>
        /// <param name="destination">The array to write to.</param>
        /// <param name="offset">The position of the array to begin writing.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteBytes(byte[] destination, int offset, long value)
        {
            destination[offset]     = (byte)(value >> 56);
            destination[offset + 1] = (byte)(value >> 48);
            destination[offset + 2] = (byte)(value >> 40);
            destination[offset + 3] = (byte)(value >> 32);
            destination[offset + 4] = (byte)(value >> 24);
            destination[offset + 5] = (byte)(value >> 16);
            destination[offset + 6] = (byte)(value >> 8);
            destination[offset + 7] = (byte)value;
        }

        /// <summary>
        ///     Writes the bytes from the unsigned long to the destination array at offset.
        /// </summary>
        /// <param name="destination">The array to write to.</param>
        /// <param name="offset">The position of the array to begin writing.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteBytes(byte[] destination, int offset, ulong value)
        {
            destination[offset]     = (byte)(value >> 56);
            destination[offset + 1] = (byte)(value >> 48);
            destination[offset + 2] = (byte)(value >> 40);
            destination[offset + 3] = (byte)(value >> 32);
            destination[offset + 4] = (byte)(value >> 24);
            destination[offset + 5] = (byte)(value >> 16);
            destination[offset + 6] = (byte)(value >> 8);
            destination[offset + 7] = (byte)value;
        }

        /// <summary>
        ///     Writes the bytes from the float to the destination array at offset.
        /// </summary>
        /// <param name="destination">The array to write to.</param>
        /// <param name="offset">The position of the array to begin writing.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteBytes(byte[] destination, int offset, float value)
        {
            uint ivalue;
            unsafe
            {
                float* ptr = &value;
                ivalue = *(uint*)ptr;
            }

            //Endianess handled here
            WriteBytes(destination, offset, ivalue);
        }


        /// <summary>
        ///     Writes the bytes from the double to the destination array at offset.
        /// </summary>
        /// <param name="destination">The array to write to.</param>
        /// <param name="offset">The position of the array to begin writing.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteBytes(byte[] destination, int offset, double value)
        {
            ulong lvalue;
            unsafe
            {
                double* ptr = &value;
                lvalue = *(ulong*)ptr;
            }

            //Endianess handled here
            WriteBytes(destination, offset, lvalue);
        }

        /// <summary>
        ///     Reads an short from the array at offset.
        /// </summary>
        /// <param name="source">The array to read from.</param>
        /// <param name="offset">The position to begin reading from.</param>
        /// <returns>The short read.</returns>
        public static short ReadInt16(byte[] source, int offset)
        {
            return (short)((source[offset] << 8) | source[offset + 1]);
        }

        /// <summary>
        ///     Reads an unsigned short from the array at offset.
        /// </summary>
        /// <param name="source">The array to read from.</param>
        /// <param name="offset">The position to begin reading from.</param>
        /// <returns>The unsigned short read.</returns>
        public static ushort ReadUInt16(byte[] source, int offset)
        {
            return (ushort)((source[offset] << 8) | source[offset + 1]);
        }

        /// <summary>
        ///     Reads an integer from the array at offset.
        /// </summary>
        /// <param name="source">The array to read from.</param>
        /// <param name="offset">The position to begin reading from.</param>
        /// <returns>The integer read.</returns>
        public static int ReadInt32(byte[] source, int offset)
        {
            return (int)((source[offset] << 24) | (source[offset + 1] << 16) | (source[offset + 2] << 8) | source[offset + 3]);
        }

        /// <summary>
        ///     Reads an unsigned integer from the array at offset.
        /// </summary>
        /// <param name="source">The array to read from.</param>
        /// <param name="offset">The position to begin reading from.</param>
        /// <returns>The unsigned integer read.</returns>
        public static uint ReadUInt32(byte[] source, int offset)
        {
            return (uint)((source[offset] << 24) | (source[offset + 1] << 16) | (source[offset + 2] << 8) | source[offset + 3]);
        }

        /// <summary>
        ///     Reads a long from the array at offset.
        /// </summary>
        /// <param name="source">The array to read from.</param>
        /// <param name="offset">The position to begin reading from.</param>
        /// <returns>The long read.</returns>
        public static long ReadInt64(byte[] source, int offset)
        {
            return ((long)source[offset] << 56) | ((long)source[offset + 1] << 48) | ((long)source[offset + 2] << 40) | ((long)source[offset + 3] << 32) | ((long)source[offset + 4] << 24) | ((long)source[offset + 5] << 16) | ((long)source[offset + 6] << 8) | source[offset + 7];
        }

        /// <summary>
        ///     Reads an unsigned long from the array at offset.
        /// </summary>
        /// <param name="source">The array to read from.</param>
        /// <param name="offset">The position to begin reading from.</param>
        /// <returns>The unsigned long read.</returns>
        public static ulong ReadUInt64(byte[] source, int offset)
        {
            return ((ulong)source[offset] << 56) | ((ulong)source[offset + 1] << 48) | ((ulong)source[offset + 2] << 40) | ((ulong)source[offset + 3] << 32) | ((ulong)source[offset + 4] << 24) | ((ulong)source[offset + 5] << 16) | ((ulong)source[offset + 6] << 8) | source[offset + 7];
        }

        /// <summary>
        ///     Reads a single from the array at offset.
        /// </summary>
        /// <param name="source">The array to read from.</param>
        /// <param name="offset">The position to begin reading from.</param>
        /// <returns>The single read.</returns>
        public static float ReadSingle(byte[] source, int offset)
        {
            //Endianess handled here
            uint ivalue = ReadUInt32(source, offset);
            
            unsafe
            {
                uint* ptr = &ivalue;
                return *(float*)ptr;
            }
        }

        /// <summary>
        ///     Reads a double from the array at offset.
        /// </summary>
        /// <param name="source">The array to read from.</param>
        /// <param name="offset">The position to begin reading from.</param>
        /// <returns>The double read.</returns>
        public static double ReadDouble(byte[] source, int offset)
        {
            //Endianess handled here
            ulong lvalue = ReadUInt64(source, offset);
            
            unsafe
            {
                ulong* ptr = &lvalue;
                return *(double*)ptr;
            }
        }

        /// <summary>
        ///     Swaps the byte order of a ushort.
        /// </summary>
        /// <param name="value">The bytes to swap.</param>
        /// <returns>The reversed bytes.</returns>
        public static ushort SwapBytes(ushort value)
        {
            return (ushort)(((value & 0x00FF) << 8) |
                   ((value & 0xFF00) >> 8));
        }

        /// <summary>
        ///     Swaps the byte order of a uint.
        /// </summary>
        /// <param name="value">The bytes to swap.</param>
        /// <returns>The reversed bytes.</returns>
        public static uint SwapBytes(uint value)
        {
            return ((value & 0x000000FF) << 24) |
                   ((value & 0x0000FF00) << 08) |
                   ((value & 0x00FF0000) >> 08) |
                   ((value & 0xFF000000) >> 24);
        }
        
        /// <summary>
        ///     Swaps the byte order of a ulong.
        /// </summary>
        /// <param name="value">The bytes to swap.</param>
        /// <returns>The reversed bytes.</returns>
        public static ulong SwapBytes(ulong value)
        {
            return ((value & 0x00000000000000FF) << 56) |
                   ((value & 0x000000000000FF00) << 40) |
                   ((value & 0x0000000000FF0000) << 24) |
                   ((value & 0x00000000FF000000) << 08) |
                   ((value & 0x000000FF00000000) >> 08) |
                   ((value & 0x0000FF0000000000) >> 24) |
                   ((value & 0x00FF000000000000) >> 40) |
                   ((value & 0xFF00000000000000) >> 56);
        }
    }
}
