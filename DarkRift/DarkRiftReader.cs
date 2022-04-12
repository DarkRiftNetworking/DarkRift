/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DarkRift
{
    /// <summary>
    ///     Helper class for converting byte buffers into their original components during deserialization.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Not thread safe as the read order is critical!
    ///     </para>
    ///     <para>
    ///         This class implements IDisposable as it is a recyclable object, if you call Dispose the class 
    ///         will be recycled and so it is not compulsory to call Dispose.
    ///     </para>
    /// </remarks>
    public class DarkRiftReader : IDisposable
    {
        /// <summary>
        ///     The string encoding to use when reading characters.
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        ///     The buffer we are reading from.
        /// </summary>
        private IMessageBuffer buffer;

        /// <summary>
        ///     The number of bytes in this reader.
        /// </summary>
        public int Length => buffer.Count;

        /// <summary>
        ///     The number of bytes read so far.
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        ///     A singleton event that exists with this reader for deserializing IDarkRiftSerializables.
        /// </summary>
        private readonly DeserializeEvent deserializeEventSingleton;

        /// <summary>
        ///     An array of a single char for caching.
        /// </summary>
        private readonly char[] singleCharArray = new char[1];

        /// <summary>
        ///     Whether this reader is currently in an object pool waiting or not.
        /// </summary>
        private volatile bool isCurrentlyLoungingInAPool;

        internal static DarkRiftReader Create(IMessageBuffer buffer)
        {
            DarkRiftReader reader = ObjectCache.GetReader();

            reader.isCurrentlyLoungingInAPool = false;

            reader.buffer = buffer;
            reader.Encoding = Encoding.Unicode;     // TODO DR3 Default to UTF-8
            reader.Position = 0;

            return reader;
        }

        /// <summary>
        ///     Creates a reader that deserializes from a standard .NET byte array.
        /// </summary>
        /// <param name="array">The array to deserialize from.</param>
        /// <param name="offset">The position in the array to begin deserializing from.</param>
        /// <param name="count">The number of bytes to deserialize.</param>
        /// <returns>The reader created for reading from the given array.</returns>
        public static DarkRiftReader CreateFromArray(byte[] array, int offset, int count)
        {
            return Create(new UnmanagedMemoryBuffer(array, offset, count));
        }

        /// <summary>
        ///     Creates a new DarkRiftReader. For use from the ObjectCache.
        /// </summary>
        internal DarkRiftReader()
        {
            deserializeEventSingleton = new DeserializeEvent(this);
        }

        /// <summary>
        ///     Reads a single byte from the reader.
        /// </summary>
        /// <returns>The byte read.</returns>
        public byte ReadByte()
        {
            if (Position >= Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 1 byte but reader only has {Length - Position} bytes remaining.");

            return buffer.Buffer[buffer.Offset + Position++];
        }

        /// <summary>
        ///     Reads a single character from the reader.
        /// </summary>
        /// <returns>The character read.</returns>
        public char ReadChar()
        {
            ReadCharsInto(singleCharArray, 0);
            return singleCharArray[0];
        }

        /// <summary>
        ///     Reads a single boolean from the reader.
        /// </summary>
        /// <returns>The boolean read.</returns>
        public bool ReadBoolean()
        {
            return ReadByte() == 1;
        }

        /// <summary>
        ///     Reads a single double from the reader.
        /// </summary>
        /// <returns>The double read.</returns>
        public double ReadDouble()
        {
            if (Position + 8 > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected 8 bytes but reader only has {Length - Position} bytes remaining.");

            double v = BigEndianHelper.ReadDouble(buffer.Buffer, buffer.Offset + Position);
            Position += 8;

            return v;
        }

        /// <summary>
        ///     Reads a single 16bit integer from the reader.
        /// </summary>
        /// <returns>The 16bit integer read.</returns>
        public short ReadInt16()
        {
            if (Position + 2 > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected 2 bytes but reader only has {Length - Position} bytes remaining.");

            short v = BigEndianHelper.ReadInt16(buffer.Buffer, buffer.Offset + Position);
            Position += 2;

            return v;
        }

        /// <summary>
        ///     Reads a single 32bit integer from the reader.
        /// </summary>
        /// <returns>The 32bit integer read.</returns>
        public int ReadInt32()
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected 4 bytes but reader only has {Length - Position} bytes remaining.");

            int v = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);
            Position += 4;

            return v;
        }

        /// <summary>
        ///     Reads a single 64bit integer from the reader.
        /// </summary>
        /// <returns>The 64bit integer read.</returns>
        public long ReadInt64()
        {
            if (Position + 8 > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected 8 bytes but reader only has {Length - Position} bytes remaining.");

            long v = BigEndianHelper.ReadInt64(buffer.Buffer, buffer.Offset + Position);
            Position += 8;

            return v;
        }

        /// <summary>
        ///     Reads a single signed byte from the reader.
        /// </summary>
        /// <returns>The signed byte read.</returns>
        public sbyte ReadSByte()
        {
            return (sbyte)ReadByte();
        }

        /// <summary>
        ///     Reads a single single from the reader.
        /// </summary>
        /// <returns>The single read.</returns>
        public float ReadSingle()
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected 4 bytes but reader only has {Length - Position} bytes remaining.");

            float v = BigEndianHelper.ReadSingle(buffer.Buffer, buffer.Offset + Position);
            Position += 4;

            return v;
        }

        /// <summary>
        ///     Reads a single string from the reader using the reader's encoding.
        /// </summary>
        /// <returns>The string read.</returns>
        public string ReadString()
        {
            return ReadString(Encoding);
        }

        /// <summary>
        ///     Reads a single string from the reader using the given encoding.
        /// </summary>
        /// <param name="encoding">The encoding to deserialize the string using.</param>
        /// <returns>The string read.</returns>
        public string ReadString(Encoding encoding)
        {
            //Read number of bytes not chars
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            if (Position + 4 + length > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length} bytes but reader only has {Length - Position - 4} bytes remaining.");

            string v = encoding.GetString(buffer.Buffer, buffer.Offset + Position + 4, length);

            Position += 4 + length;

            return v;
        }

        /// <summary>
        ///     Reads a single unsigned 16bit integer from the reader.
        /// </summary>
        /// <returns>The unsigned 16bit integer read.</returns>
        public ushort ReadUInt16()
        {
            if (Position + 2 > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected 2 bytes but reader only has {Length - Position} bytes remaining.");

            ushort v = BigEndianHelper.ReadUInt16(buffer.Buffer, buffer.Offset + Position);
            Position += 2;

            return v;
        }

        /// <summary>
        ///     Reads a single unsigned 32bit integer from the reader.
        /// </summary>
        /// <returns>The unsigned 32bit integer read.</returns>
        public uint ReadUInt32()
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected 4 bytes but reader only has {Length - Position} bytes remaining.");

            uint v = BigEndianHelper.ReadUInt32(buffer.Buffer, buffer.Offset + Position);
            Position += 4;

            return v;
        }

        /// <summary>
        ///     Reads a single unsigned 64bit integer from the reader.
        /// </summary>
        /// <returns>The unsigned 64bit integer read.</returns>
        public ulong ReadUInt64()
        {
            if (Position + 8 > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected 8 bytes but reader only has {Length - Position} bytes remaining.");

            ulong v = BigEndianHelper.ReadUInt64(buffer.Buffer, buffer.Offset + Position);
            Position += 8;

            return v;
        }

        /// <summary>
        ///     Reads a single serializable object from the reader.
        /// </summary>
        /// <typeparam name="T">The type of the object to read.</typeparam>
        /// <returns>The serializable object read.</returns>
        public T ReadSerializable<T>() where T : IDarkRiftSerializable, new()
        {
            T t = new T();
            ReadSerializableInto(ref t);
            return t;
        }

        /// <summary>
        ///     Reads a single serializable object from the reader into the given object.
        /// </summary>
        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <param name="t">The object to deserialize the data into.</param>
        public void ReadSerializableInto<T>(ref T t) where T : IDarkRiftSerializable
        {
            t.Deserialize(deserializeEventSingleton);
        }

        /// <summary>
        ///     Reads an array of bytes from the reader.
        /// </summary>
        /// <returns>The array of bytes read.</returns>
        public byte[] ReadBytes()
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            if (Position + 4 + length > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length} bytes but reader only has {Length - Position - 4} bytes remaining.");

            byte[] array = new byte[length];
            Buffer.BlockCopy(buffer.Buffer, buffer.Offset + Position + 4, array, 0, length);

            Position += 4 + length;

            return array;
        }

        /// <summary>
        ///     Reads an array of bytes from the reader.
        /// </summary>
        /// <param name="destination">The array to read bytes into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadBytesInto(byte[] destination, int offset)
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            if (Position + 4 + length > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length} bytes but reader only has {Length - Position - 4} bytes remaining.");

            Buffer.BlockCopy(buffer.Buffer, buffer.Offset + Position + 4, destination, 0, length);

            Position += 4 + length;
        }

        /// <summary>
        ///     Reads a array of characters from the reader.
        /// </summary>
        /// <returns>The array of characters read.</returns>
        public char[] ReadChars()
        {
            return ReadChars(Encoding);
        }

        /// <summary>
        ///     Reads a array of characters from the reader.
        /// </summary>
        /// <param name="destination">The array to read characters into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadCharsInto(char[] destination, int offset)
        {
            ReadCharsInto(destination, offset, Encoding);
        }

        /// <summary>
        ///     Reads an array of characters from the reader using the given encoding.
        /// </summary>
        /// <param name="encoding">The encoding to use during the deserialization.</param>
        /// <returns>The array of characters read.</returns>
        public char[] ReadChars(Encoding encoding)
        {
            //Read number of bytes not chars
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            if (Position + 4 + length > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length} bytes but reader only has {Length - Position - 4} bytes remaining.");

            char[] array = encoding.GetChars(buffer.Buffer, buffer.Offset + Position + 4, length);
            
            Position += 4 + length;

            return array;
        }

        /// <summary>
        ///     Reads an array of characters from the reader using the given encoding.
        /// </summary>
        /// <param name="destination">The array to read characters into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        /// <param name="encoding">The encoding to use during the deserialization.</param>
        public void ReadCharsInto(char[] destination, int offset, Encoding encoding)
        {
            //Read number of bytes not chars
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            if (Position + 4 + length > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length} bytes but reader only has {Length - Position - 4} bytes remaining.");

            encoding.GetChars(buffer.Buffer, buffer.Offset + Position + 4, length, destination, offset);

            Position += 4 + length;
        }

        /// <summary>
        ///     Reads an array of booleans from the reader.
        /// </summary>
        /// <returns>The array of booleans read.</returns>
        public bool[] ReadBooleans()
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            int total = (int)Math.Ceiling(length / 8.0);    //Number of bytes the booleans are stored in

            if (Position + 4 + total > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {total} bytes but reader only has {Length - Position - 4} bytes remaining.");

            bool[] array = new bool[length];
            int ptr = 0;                                    //The index of the array we're writing to.

            //Repeat for each byte we will need
            for (int i = 0; i < total; i++)
            {
                byte b = buffer.Buffer[buffer.Offset + Position + 4 + i];

                //Repeat for each bit in that byte
                for (int k = 7; k >= 0 && ptr < length; k--)
                    array[ptr++] = (b & (1 << k)) != 0;
            }

            Position += 4 + total;

            return array;
        }

        /// <summary>
        ///     Reads an array of booleans from the reader.
        /// </summary>
        /// <param name="destination">The array to read booleans into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadBooleansInto(bool[] destination, int offset)
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            int total = (int)Math.Ceiling(length / 8.0);    //Number of bytes the booleans are stored in

            if (Position + 4 + total > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {total} bytes but reader only has {Length - Position - 4} bytes remaining.");

            int ptr = offset;                                    //The index of the array we're writing to.

            //Repeat for each byte we will need
            for (int i = 0; i < total; i++)
            {
                byte b = buffer.Buffer[buffer.Offset + Position + 4 + i];

                //Repeat for each bit in that byte
                for (int k = 7; k >= 0 && ptr < length; k--)
                    destination[ptr++] = (b & (1 << k)) != 0;
            }

            Position += 4 + total;
        }

        /// <summary>
        ///     Reads an array of doubles from the reader.
        /// </summary>
        /// <returns>The array of doubles read.</returns>
        public double[] ReadDoubles()
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            if (Position + 4 + length * 8 > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length * 8} bytes but reader only has {Length - Position - 4} bytes remaining.");

            double[] array = new double[length];
            for (int i = 0, j = buffer.Offset + Position + 4; i < length; i++, j += 8)
                array[i] = BigEndianHelper.ReadDouble(buffer.Buffer, j);

            Position += 4 + length * 8;

            return array;
        }

        /// <summary>
        ///     Reads an array of doubles from the reader.
        /// </summary>
        /// <returns>The array of doubles read.</returns>
        /// <param name="destination">The array to read doubles into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadDoublesInto(double[] destination, int offset)
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            if (Position + 4 + length * 8 > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length * 8} bytes but reader only has {Length - Position - 4} bytes remaining.");

            for (int i = 0, j = buffer.Offset + Position + 4; i < length; i++, j += 8)
                destination[i + offset] = BigEndianHelper.ReadDouble(buffer.Buffer, j);

            Position += 4 + length * 8;
        }

        /// <summary>
        ///     Reads an array of 16bit integers from the reader.
        /// </summary>
        /// <returns>The array of 16bit integers read.</returns>
        public short[] ReadInt16s()
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            if (Position + 4 + length * 2 > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length * 2} bytes but reader only has {Length - Position - 4} bytes remaining.");

            short[] array = new short[length];
            for (int i = 0, j = buffer.Offset + Position + 4; i < length; i++, j += 2)
                array[i] = BigEndianHelper.ReadInt16(buffer.Buffer, j);

            Position += 4 + length *2;

            return array;
        }

        /// <summary>
        ///     Reads an array of 16bit integers from the reader.
        /// </summary>
        /// <param name="destination">The array to read int16s into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadInt16sInto(short[] destination, int offset)
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            if (Position + 4 + length * 2 > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length * 2} bytes but reader only has {Length - Position - 4} bytes remaining.");

            for (int i = 0, j = buffer.Offset + Position + 4; i < length; i++, j += 2)
                destination[i + offset] = BigEndianHelper.ReadInt16(buffer.Buffer, j);

            Position += 4 + length * 2;
        }

        /// <summary>
        ///     Reads an array of 32bit integers from the reader.
        /// </summary>
        /// <returns>The array of 32bit integers read.</returns>
        public int[] ReadInt32s()
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            if (Position + 4 + length * 4 > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length * 4} bytes but reader only has {Length - Position - 4} bytes remaining.");

            int[] array = new int[length];
            for (int i = 0, j = buffer.Offset + Position + 4; i < length; i++, j += 4)
                array[i] = BigEndianHelper.ReadInt32(buffer.Buffer, j);

            Position += 4 + length * 4;

            return array;
        }

        /// <summary>
        ///     Reads an array of 32bit integers from the reader.
        /// </summary>
        /// <param name="destination">The array to read int32s into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadInt32sInto(int[] destination, int offset)
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            if (Position + 4 + length * 4 > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length * 4} bytes but reader only has {Length - Position - 4} bytes remaining.");

            for (int i = 0, j = buffer.Offset + Position + 4; i < length; i++, j += 4)
                destination[i + offset] = BigEndianHelper.ReadInt32(buffer.Buffer, j);

            Position += 4 + length * 4;
        }

        /// <summary>
        ///     Reads an array of 64bit integers from the reader.
        /// </summary>
        /// <returns>The array of 64bit integers read.</returns>
        public long[] ReadInt64s()
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            if (Position + 4 + length * 8 > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length * 8} bytes but reader only has {Length - Position - 4} bytes remaining.");

            long[] array = new long[length];
            for (int i = 0, j = buffer.Offset + Position + 4; i < length; i++, j += 8)
                array[i] = BigEndianHelper.ReadInt64(buffer.Buffer, j);

            Position += 4 + length * 8;

            return array;
        }

        /// <summary>
        ///     Reads an array of 64bit integers from the reader.
        /// </summary>
        /// <param name="destination">The array to read int64s into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadInt64sInto(long[] destination, int offset)
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            if (Position + 4 + length * 8 > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length * 8} bytes but reader only has {Length - Position - 4} bytes remaining.");
            
            for (int i = 0, j = buffer.Offset + Position + 4; i < length; i++, j += 8)
                destination[i + offset] = BigEndianHelper.ReadInt64(buffer.Buffer, j);

            Position += 4 + length * 8;
        }

        /// <summary>
        ///     Reads an array of signed bytes from the reader.
        /// </summary>
        /// <returns>The array of signed bytes read.</returns>
        public sbyte[] ReadSBytes()
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            if (Position + 4 + length > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length} bytes but reader only has {Length - Position - 4} bytes remaining.");

            sbyte[] array = new sbyte[length];
            Buffer.BlockCopy(buffer.Buffer, buffer.Offset + Position + 4, array, 0, length);

            Position += 4 + length;

            return array;
        }

        /// <summary>
        ///     Reads an array of signed bytes from the reader.
        /// </summary>
        /// <param name="destination">The array to read sbytes into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadSBytesInto(sbyte[] destination, int offset)
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            if (Position + 4 + length > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length} bytes but reader only has {Length - Position - 4} bytes remaining.");

            Buffer.BlockCopy(buffer.Buffer, buffer.Offset + Position + 4, destination, offset, length);

            Position += 4 + length;
        }

        /// <summary>
        ///     Reads an array of singles from the reader.
        /// </summary>
        /// <returns>The array of singles read.</returns>
        public float[] ReadSingles()
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            if (Position + 4 + length * 4 > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length * 4} bytes but reader only has {Length - Position - 4} bytes remaining.");

            float[] array = new float[length];
            for (int i = 0, j = buffer.Offset + Position + 4; i < length; i++, j += 4)
                array[i] = BigEndianHelper.ReadSingle(buffer.Buffer, j);

            Position += 4 + length * 4;

            return array;
        }

        /// <summary>
        ///     Reads an array of singles from the reader.
        /// </summary>
        /// <param name="destination">The array to read singles into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadSinglesInto(float[] destination, int offset)
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            if (Position + 4 + length * 4 > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length * 4} bytes but reader only has {Length - Position - 4} bytes remaining.");

            for (int i = 0, j = buffer.Offset + Position + 4; i < length; i++, j += 4)
                destination[i + offset] = BigEndianHelper.ReadSingle(buffer.Buffer, j);

            Position += 4 + length * 4;
        }

        /// <summary>
        ///     Reads an array of strings from the reader using the reader's encoding.
        /// </summary>
        /// <returns>The array of strings read.</returns>
        public string[] ReadStrings()
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);
            Position += 4;

            string[] array = new string[length];
            for (int i = 0; i < length; i++)
                array[i] = ReadString();

            return array;
        }

        /// <summary>
        ///     Reads an array of strings from the reader using the reader's encoding.
        /// </summary>
        /// <param name="destination">The array to read strings into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadStringsInto(string[] destination, int offset)
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);
            Position += 4;

            for (int i = 0; i < length; i++)
                destination[i + offset] = ReadString();
        }
        
        /// <summary>
        ///     Reads an array unsigned 16bit integers from the reader.
        /// </summary>
        /// <returns>The array of unsigned 16bit integers read.</returns>
        public ushort[] ReadUInt16s()
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            if (Position + 4 + length * 2 > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length * 2} bytes but reader only has {Length - Position - 4} bytes remaining.");

            ushort[] array = new ushort[length];
            for (int i = 0, j = buffer.Offset + Position + 4; i < length; i++, j += 2)
                array[i] = BigEndianHelper.ReadUInt16(buffer.Buffer, j);

            Position += 4 + length * 2;

            return array;
        }

        /// <summary>
        ///     Reads an array unsigned 16bit integers from the reader.
        /// </summary>
        /// <param name="destination">The array to read strings into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadUInt16sInto(ushort[] destination, int offset)
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            if (Position + 4 + length * 2 > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length * 2} bytes but reader only has {Length - Position - 4} bytes remaining.");

            for (int i = 0, j = buffer.Offset + Position + 4; i < length; i++, j += 2)
                destination[i + offset] = BigEndianHelper.ReadUInt16(buffer.Buffer, j);

            Position += 4 + length * 2;
        }

        /// <summary>
        ///     Reads an array unsigned 32bit integers from the reader.
        /// </summary>
        /// <returns>The array of unsigned 32bit integers read.</returns>
        public uint[] ReadUInt32s()
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            if (Position + 4 + length * 4 > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length * 4} bytes but reader only has {Length - Position - 4} bytes remaining.");

            uint[] array = new uint[length];
            for (int i = 0, j = buffer.Offset + Position + 4; i < length; i++, j += 4)
                array[i] = BigEndianHelper.ReadUInt32(buffer.Buffer, j);

            Position += 4 + length * 4;

            return array;
        }

        /// <summary>
        ///     Reads an array unsigned 32bit integers from the reader.
        /// </summary>
        /// <param name="destination">The array to read strings into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadUInt32sInto(uint[] destination, int offset)
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            if (Position + 4 + length * 4 > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length * 4} bytes but reader only has {Length - Position - 4} bytes remaining.");

            for (int i = 0, j = buffer.Offset + Position + 4; i < length; i++, j += 4)
                destination[i + offset] = BigEndianHelper.ReadUInt32(buffer.Buffer, j);

            Position += 4 + length * 4;
        }

        /// <summary>
        ///     Reads an array unsigned 64bit integers from the reader.
        /// </summary>
        /// <returns>The array of unsigned 64bit integers read.</returns>
        public ulong[] ReadUInt64s()
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            if (Position + 4 + length * 8 > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length * 8} bytes but reader only has {Length - Position - 4} bytes remaining.");

            ulong[] array = new ulong[length];
            for (int i = 0, j = buffer.Offset + Position + 4; i < length; i++, j += 8)
                array[i] = BigEndianHelper.ReadUInt64(buffer.Buffer, j);

            Position += 4 + length * 8;

            return array;
        }

        /// <summary>
        ///     Reads an array unsigned 64bit integers from the reader.
        /// </summary>
        /// <param name="destination">The array to read strings into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadUInt64sInto(ulong[] destination, int offset)
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            if (Position + 4 + length * 8 > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length * 8} bytes but reader only has {Length - Position - 4} bytes remaining.");

            for (int i = 0, j = buffer.Offset + Position + 4; i < length; i++, j += 8)
                destination[i + offset] = BigEndianHelper.ReadUInt64(buffer.Buffer, j);

            Position += 4 + length * 8;
        }

        /// <summary>
        ///     Reads an array of a serializable object from the reader.
        /// </summary>
        /// <typeparam name="T">The type of the object to read.</typeparam>
        /// <returns>The serializable objects read.</returns>
        public T[] ReadSerializables<T>() where T : IDarkRiftSerializable, new()
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            Position += 4;

            T[] array = new T[length];
            for (int i = 0; i < length; i++)
                array[i] = ReadSerializable<T>();
            
            return array;
        }

        /// <summary>
        ///     Reads an array of a serializable object from the reader.
        /// </summary>
        /// <typeparam name="T">The type of the object to read.</typeparam>
        /// <returns>The serializable objects read.</returns>
        public void ReadSerializablesInto<T>(T[] destination, int offset) where T : IDarkRiftSerializable, new()
        {
            if (Position + 4 > Length)
                throw new EndOfStreamException($"Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {Length - Position} bytes remaining.");

            int length = BigEndianHelper.ReadInt32(buffer.Buffer, buffer.Offset + Position);

            Position += 4;
            
            for (int i = 0; i < length; i++)
                destination[i + offset] = ReadSerializable<T>();
        }

        /// <summary>
        ///     Reads an array of raw bytes from the reader.
        /// </summary>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>The array of bytes read.</returns>
        public byte[] ReadRaw(int length)
        {
            if (Position + length > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length} bytes but reader only has {Length - Position} bytes remaining.");

            byte[] raw = new byte[length];

            ReadRawInto(raw, 0, length);

            return raw;
        }

        /// <summary>
        ///     Reads an array of raw bytes from the reader into the given array.
        /// </summary>
        /// <param name="buffer">The buffer to read into.</param>
        /// <param name="offset">The offset to start writing into the buffer at.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>The array of bytes read.</returns>
        public void ReadRawInto(byte[] buffer, int offset, int length)
        {
            if (Position + length > Length)
                throw new EndOfStreamException($"Failed to read data from reader as the reader does not have enough data remaining. Expected {length} bytes but reader only has {Length - Position} bytes remaining.");

            Buffer.BlockCopy(this.buffer.Buffer, this.buffer.Offset + Position, buffer, offset, length);
            Position += length;
        }

        /// <summary>
        ///     Recycles this object back into the pool.
        /// </summary>
        public void Dispose()
        {
            buffer.Dispose();

            ObjectCache.ReturnReader(this);
            isCurrentlyLoungingInAPool = true;
        }

        /// <summary>
        ///     Finalizer so we can inform the cache system we were not recycled correctly.
        /// </summary>
        ~DarkRiftReader()
        {
            if (!isCurrentlyLoungingInAPool)
                ObjectCacheHelper.DarkRiftReaderWasFinalized();
        }
    }
}
