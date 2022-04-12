/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DarkRift
{
    /// <summary>
    ///     Helper class for serializing values into  byte buffers.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Not thread safe as the write order is critical!
    ///     </para>
    ///     <para>
    ///         This class implements IDisposable as it is a recyclable object, if you call Dispose the class 
    ///         will be recycled and so it is not compulsory to call Dispose.
    ///     </para>
    /// </remarks>
    public class DarkRiftWriter : IDisposable
    {
        /// <summary>
        ///     The string encoding to use when writing characters.
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        ///     The position data is being written to in this writer.
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        ///     The number of bytes in this writer.
        /// </summary>
        public int Length => buffer.Count;

        /// <summary>
        ///     The current capacity of the base array for the writer.
        /// </summary>
        public int Capacity => buffer.Buffer.Length;

        /// <summary>
        ///     An array of a single char for caching.
        /// </summary>
        private readonly char[] singleCharArray = new char[1];

        /// <summary>
        ///     A singleton event that exists with this writer for serializing IDarkRiftSerializables.
        /// </summary>
        private readonly SerializeEvent serializeEventSingleton;

        /// <summary>
        ///     The backing array holding the data.
        /// </summary>
        private MessageBuffer buffer;

        /// <summary>
        ///     Whether this writer is currently in an object pool waiting or not.
        /// </summary>
        private volatile bool isCurrentlyLoungingInAPool;

        /// <summary>
        ///     Creates a new DarkRift writer with Unicode encoding.
        /// </summary>
        public static DarkRiftWriter Create()
        {
           return Create(16, Encoding.Unicode);     // TODO DR3 Default to UTF-8
        }

        /// <summary>
        ///     Creates a new DarkRift writer with the specified encoding.
        /// </summary>
        /// <param name="encoding">The encoding to serialize strings and characters using.</param>
        public static DarkRiftWriter Create(Encoding encoding)
        {
            return Create(16, encoding);
        }

        /// <summary>
        ///     Creates a new DarkRift writer with an initial capacity.
        /// </summary>
        /// <param name="initialCapacity">The initial capacity for the backing array.</param>
        public static DarkRiftWriter Create(int initialCapacity)
        {
            return Create(initialCapacity, Encoding.Unicode);
        }

        /// <summary>
        ///     Creates a new DarkRift writer with an initial capacity and specified encoding.
        /// </summary>
        /// <param name="initialCapacity">The initial capacity for the backing array.</param>
        /// <param name="encoding">The encoding to serialize strings and characters using.</param>
        public static DarkRiftWriter Create(int initialCapacity, Encoding encoding)
        {
            DarkRiftWriter writer = ObjectCache.GetWriter();

            writer.isCurrentlyLoungingInAPool = false;

            writer.buffer = MessageBuffer.Create(initialCapacity);

            writer.Position = 0;
            writer.Encoding = encoding;
            return writer;
        }

        /// <summary>
        ///     Creates a new DarkRiftWriter. For use from the ObjectCache.
        /// </summary>
        internal DarkRiftWriter()
        {
            serializeEventSingleton = new SerializeEvent(this);
        }
        
        /// <summary>
        ///     Writes a single byte to the writer.
        /// </summary>
        /// <param name="value">The byte to write.</param>
        public void Write(byte value)
        {
            buffer.EnsureLength(Position + 1);
            buffer.Buffer[Position++] = value;
            buffer.Count = Math.Max(Length, Position);
        }

        /// <summary>
        ///     Writes a single character to the writer.
        /// </summary>
        /// <param name="value">The character to write.</param>
        public void Write(char value)
        {
            singleCharArray[0] = value;
            Write(singleCharArray);        //Write as array so we have a length (sorts encoding issues)
        }

        /// <summary>
        ///     Writes a single boolean to the writer.
        /// </summary>
        /// <param name="value">The boolean to write.</param>
        public void Write(bool value)
        {
            buffer.EnsureLength(Position + 1);
            Write((byte)(value ? 1 : 0));
        }

        /// <summary>
        ///     Writes a single double to the writer.
        /// </summary>
        /// <param name="value">The double to write.</param>
        public void Write(double value)
        {
            buffer.EnsureLength(Position + 8);
            BigEndianHelper.WriteBytes(buffer.Buffer, Position, value);
            Position += 8;
            buffer.Count = Math.Max(Length, Position);
        }

        /// <summary>
        ///     Writes a single 16bit integer to the writer.
        /// </summary>
        /// <param name="value">The 16bit integer to write.</param>
        public void Write(short value)
        {
            buffer.EnsureLength(Position + 2);
            BigEndianHelper.WriteBytes(buffer.Buffer, Position, value);
            Position += 2;
            buffer.Count = Math.Max(Length, Position);
        }

        /// <summary>
        ///     Writes a single 32bit integer to the writer.
        /// </summary>
        /// <param name="value">The 32bit integer to write.</param>
        public void Write(int value)
        {
            buffer.EnsureLength(Position + 4);
            BigEndianHelper.WriteBytes(buffer.Buffer, Position, value);
            Position += 4;
            buffer.Count = Math.Max(Length, Position);
        }

        /// <summary>
        ///     Writes a single 64bit integer to the writer.
        /// </summary>
        /// <param name="value">The 64bit integer to write.</param>
        public void Write(long value)
        {
            buffer.EnsureLength(Position + 8);
            BigEndianHelper.WriteBytes(buffer.Buffer, Position, value);
            Position += 8;
            buffer.Count = Math.Max(Length, Position);
        }

        /// <summary>
        ///     Writes a single signed byte to the writer.
        /// </summary>
        /// <param name="value">The signed byte to write.</param>
        public void Write(sbyte value)
        {
            Write((byte)value);
        }

        /// <summary>
        ///     Writes a single single to the writer.
        /// </summary>
        /// <param name="value">The single to write.</param>
        public void Write(float value)
        {
            buffer.EnsureLength(Position + 4);
            BigEndianHelper.WriteBytes(buffer.Buffer, Position, value);
            Position += 4;
            buffer.Count = Math.Max(Length, Position);
        }

        /// <summary>
        ///     Writes a single unsigned 16bit integer to the writer.
        /// </summary>
        /// <param name="value">The unsigned 16bit integer to write.</param>
        public void Write(ushort value)
        {
            buffer.EnsureLength(Position + 2);
            BigEndianHelper.WriteBytes(buffer.Buffer, Position, value);
            Position += 2;
            buffer.Count = Math.Max(Length, Position);
        }

        /// <summary>
        ///     Writes a single unsigned 32bit integer to the writer.
        /// </summary>
        /// <param name="value">The unsigned 32bit integer to write.</param>
        public void Write(uint value)
        {
            buffer.EnsureLength(Position + 4);
            BigEndianHelper.WriteBytes(buffer.Buffer, Position, value);
            Position += 4;
            buffer.Count = Math.Max(Length, Position);
        }

        /// <summary>
        ///     Writes a single unsigned 64bit integer to the writer.
        /// </summary>
        /// <param name="value">The unsigned 64bit integer to write.</param>
        public void Write(ulong value)
        {
            buffer.EnsureLength(Position + 8);
            BigEndianHelper.WriteBytes(buffer.Buffer, Position, value);
            Position += 8;
            buffer.Count = Math.Max(Length, Position);
        }

        /// <summary>
        ///     Writes a single string to the writer using the writer's encoding.
        /// </summary>
        /// <param name="value">The string to write.</param>
        public void Write(string value)
        {
            Write(value, Encoding);
        }

        /// <summary>
        ///     Writes a single string to the writer using the given encoding.
        /// </summary>
        /// <param name="value">The string to write.</param>
        /// <param name="encoding">The encoding to deserialize the string using.</param>
        public void Write(string value, Encoding encoding)
        {
            //Legacy implementation means we need to send number of bytes not chars
            int length = encoding.GetByteCount(value);

            buffer.EnsureLength(Position + 4 + length);
            BigEndianHelper.WriteBytes(buffer.Buffer, Position, length);
            encoding.GetBytes(value, 0, value.Length, buffer.Buffer, Position + 4);
            Position += 4 + length;
            buffer.Count = Math.Max(Length, Position);
        }

        /// <summary>
        ///     Writes a single serializable object to the writer.
        /// </summary>
        /// <param name="serializable">The serializable object to write.</param>
        [Obsolete("Use Write<T>(T serializable) instead.")]
        public void Write(IDarkRiftSerializable serializable)
        {
            serializable.Serialize(serializeEventSingleton);
        }

        /// <summary>
        ///     Writes a single serializable object to the writer.
        /// </summary>
        /// <param name="serializable">The serializable object to write.</param>
        public void Write<T>(T serializable) where T : IDarkRiftSerializable
        {
            serializable.Serialize(serializeEventSingleton);
        }

        /// <summary>
        ///     Writes an array of bytes to the writer.
        /// </summary>
        /// <param name="value">The array of bytes to write.</param>
        public void Write(byte[] value)
        {
            buffer.EnsureLength(Position + 4 + value.Length);
            BigEndianHelper.WriteBytes(buffer.Buffer, Position, value.Length);
            System.Buffer.BlockCopy(value, 0, buffer.Buffer, Position + 4, value.Length);
            Position += 4 + value.Length;
            buffer.Count = Math.Max(Length, Position);
        }

        /// <summary>
        ///     Writes a array of characters to the writer.
        /// </summary>
        /// <param name="value">The array of characters to write.</param>
        public void Write(char[] value)
        {
            Write(value, Encoding);
        }

        /// <summary>
        ///     Writes an array of characters to the writer using the given encoding.
        /// </summary>
        /// <param name="encoding">The encoding to use during the deserialization.</param>
        /// <param name="value">The array of characters to write.</param>
        public void Write(char[] value, Encoding encoding)
        {
            //Legacy implementation means we need to send number of bytes not chars
            int length = encoding.GetByteCount(value);

            buffer.EnsureLength(Position + 4 + length);
            BigEndianHelper.WriteBytes(buffer.Buffer, Position, length);
            encoding.GetBytes(value, 0, value.Length, buffer.Buffer, Position + 4);
            Position += 4 + length;
            buffer.Count = Math.Max(Length, Position);
        }

        /// <summary>
        ///     Writes an array of booleans to the writer.
        /// </summary>
        /// <param name="value">The array of booleans to write.</param>
        public void Write(bool[] value)
        {
            int total = (int)Math.Ceiling(value.Length / 8.0);   //Total number of bytes that will be needed

            buffer.EnsureLength(Position + 4 + total);
            BigEndianHelper.WriteBytes(buffer.Buffer, Position, value.Length);

            int ptr = 0;                                    //Pointer to current boolean in value array
            
            //Repeat for each byte we will need
            for (int i = 0; i < total; i++)
            {
                byte b = 0;                                     //Temp holder of booleans being packed

                //Repeat for each bit in that byte
                for (int k = 7; k >= 0 && ptr < value.Length; k--)
                {
                    if (value[ptr])
                        b |= (byte)(1 << k);

                    ptr++;
                }

                buffer.Buffer[Position + 4 + i] = b;
            }

            Position += 4 + total;
            buffer.Count = Math.Max(Length, Position);
        }

        /// <summary>
        ///     Writes an array of doubles to the writer.
        /// </summary>
        /// <param name="value">The array of doubles to write.</param>
        public void Write(double[] value)
        {
            buffer.EnsureLength(Position + 4 + value.Length * 8);
            BigEndianHelper.WriteBytes(buffer.Buffer, Position, value.Length);

            for (int i = 0, j = Position + 4; i < value.Length; i++, j += 8)
            {
                byte[] b = BitConverter.GetBytes(value[i]);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(b);

                System.Buffer.BlockCopy(b, 0, buffer.Buffer, j, 8);
            }
            
            Position += 4 + value.Length * 8;
            buffer.Count = Math.Max(Length, Position);
        }

        /// <summary>
        ///     Writes an array of 16bit integers to the writer.
        /// </summary>
        /// <param name="value">The array of 16bit integers to write.</param>
        public void Write(short[] value)
        {
            buffer.EnsureLength(Position + 4 + value.Length * 2);
            BigEndianHelper.WriteBytes(buffer.Buffer, Position, value.Length);

            for (int i = 0, j = Position + 4; i < value.Length; i++, j += 2)
                BigEndianHelper.WriteBytes(buffer.Buffer, j, value[i]);

            Position += 4 + value.Length * 2;
            buffer.Count = Math.Max(Length, Position);
        }

        /// <summary>
        ///     Writes an array of 32bit integers to the writer.
        /// </summary>
        /// <param name="value">The array of 32bit integers to write.</param>
        public void Write(int[] value)
        {
            buffer.EnsureLength(Position + 4 + value.Length * 4);
            BigEndianHelper.WriteBytes(buffer.Buffer, Position, value.Length);

            for (int i = 0, j = Position + 4; i < value.Length; i++, j += 4)
                BigEndianHelper.WriteBytes(buffer.Buffer, j, value[i]);

            Position += 4 + value.Length * 4;
            buffer.Count = Math.Max(Length, Position);
        }

        /// <summary>
        ///     Writes an array of 64bit integers to the writer.
        /// </summary>
        /// <param name="value">The array of 64bit integers to write.</param>
        public void Write(long[] value)
        {
            buffer.EnsureLength(Position + 4 + value.Length * 8);
            BigEndianHelper.WriteBytes(buffer.Buffer, Position, value.Length);

            for (int i = 0, j = Position + 4; i < value.Length; i++, j += 8)
                BigEndianHelper.WriteBytes(buffer.Buffer, j, value[i]);

            Position += 4 + value.Length * 8;
            buffer.Count = Math.Max(Length, Position);
        }

        /// <summary>
        ///     Writes an array of signed bytes to the writer.
        /// </summary>
        /// <param name="value">The array of signed bytes to write.</param>
        public void Write(sbyte[] value)
        {
            buffer.EnsureLength(Position + 4 + value.Length);
            BigEndianHelper.WriteBytes(buffer.Buffer, Position, value.Length);
            System.Buffer.BlockCopy(value, 0, buffer.Buffer, Position + 4, value.Length);
            Position += 4 + value.Length;
            buffer.Count = Math.Max(Length, Position);
        }

        /// <summary>
        ///     Writes an array of singles to the writer.
        /// </summary>
        /// <param name="value">The array of singles to write.</param>
        public void Write(float[] value)
        {
            buffer.EnsureLength(Position + 4 + value.Length * 4);
            BigEndianHelper.WriteBytes(buffer.Buffer, Position, value.Length);

            for (int i = 0, j = Position + 4; i < value.Length; i++, j += 4)
            {
                byte[] b = BitConverter.GetBytes(value[i]);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(b);

                System.Buffer.BlockCopy(b, 0, buffer.Buffer, j, 4);
            }

            Position += 4 + value.Length * 4;
            buffer.Count = Math.Max(Length, Position);
        }

        /// <summary>
        ///     Writes an array of strings to the writer using the writer's encoding.
        /// </summary>
        /// <param name="value">The array of strings to write.</param>
        public void Write(string[] value)
        {
            buffer.EnsureLength(Position + 4);          //Encodings suck, just do this manually
            BigEndianHelper.WriteBytes(buffer.Buffer, Position, value.Length);
            Position += 4;
            buffer.Count = Math.Max(Length, Position);

            foreach (string b in value)
                Write(b);
        }

        /// <summary>
        ///     Writes an array unsigned 16bit integers to the writer.
        /// </summary>
        /// <param name="value">The array of unsigned 16bit integers to write.</param>
        public void Write(ushort[] value)
        {
            buffer.EnsureLength(Position + 4 + value.Length * 2);
            BigEndianHelper.WriteBytes(buffer.Buffer, Position, value.Length);

            for (int i = 0, j = Position + 4; i < value.Length; i++, j += 2)
                BigEndianHelper.WriteBytes(buffer.Buffer, j, value[i]);

            Position += 4 + value.Length * 2;
            buffer.Count = Math.Max(Length, Position);
        }

        /// <summary>
        ///     Writes an array unsigned 32bit integers to the writer.
        /// </summary>
        /// <param name="value">The array of unsigned 32bit integers to write.</param>
        public void Write(uint[] value)
        {
            buffer.EnsureLength(Position + 4 + value.Length * 4);
            BigEndianHelper.WriteBytes(buffer.Buffer, Position, value.Length);

            for (int i = 0, j = Position + 4; i < value.Length; i++, j += 4)
                BigEndianHelper.WriteBytes(buffer.Buffer, j, value[i]);

            Position += 4 + value.Length * 4;
            buffer.Count = Math.Max(Length, Position);
        }

        /// <summary>
        ///     Writes an array unsigned 64bit integers to the writer.
        /// </summary>
        /// <param name="value">The array of unsigned 64bit integers to write.</param>
        public void Write(ulong[] value)
        {
            buffer.EnsureLength(Position + 4 + value.Length * 8);
            BigEndianHelper.WriteBytes(buffer.Buffer, Position, value.Length);

            for (int i = 0, j = Position + 4; i < value.Length; i++, j += 8)
                BigEndianHelper.WriteBytes(buffer.Buffer, j, value[i]);

            Position += 4 + value.Length * 8;
            buffer.Count = Math.Max(Length, Position);
        }

        /// <summary>
        ///     Writes an array IDarkRiftSerializables to the writer.
        /// </summary>
        /// <param name="value">The array of serializable objects to write.</param>
        public void Write<T>(T[] value) where T : IDarkRiftSerializable
        {
            buffer.EnsureLength(Position + 4);
            BigEndianHelper.WriteBytes(buffer.Buffer, Position, value.Length);

            Position += 4;
            buffer.Count = Math.Max(Length, Position);

            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        /// <summary>
        ///     Writes an array of raw bytes to the writer.
        /// </summary>
        /// <param name="bytes">The array of bytes to write.</param>
        /// <param name="offset">The start point in the array to write.</param>
        /// <param name="length">The number of bytes to write.</param>
        public void WriteRaw(byte[] bytes, int offset, int length)
        {
            buffer.EnsureLength(Position + length);
            System.Buffer.BlockCopy(bytes, offset, buffer.Buffer, Position, length);

            Position += length;
            buffer.Count = Math.Max(Length, Position);
        }

        /// <summary>
        ///     Reserves blank space in the writer.
        /// </summary>
        /// <param name="size">The number of bytes to reserve.</param>
        /// <returns>The position of the space reserved.</returns>
        public int Reserve(int size)
        {
            buffer.EnsureLength(Position + size);

            int location = Position;

            Position += size;
            buffer.Count = Math.Max(Length, Position);

            return location;
        }

        /// <summary>
        ///     Writes the contents of the writer to an array.
        /// </summary>
        /// <returns>An array containg the writer's contents.</returns>
        public byte[] ToArray()
        {
            byte[] array = new byte[Length];
            CopyTo(array, 0);
            return array;
        }

        /// <summary>
        ///     Copies the contents of this writer to the given array.
        /// </summary>
        /// <param name="destination">The array to copy the contents into.</param>
        /// <param name="offset">The offset to start writing contents at in the array.</param>
        public void CopyTo(byte[] destination, int offset)
        {
            Buffer.BlockCopy(buffer.Buffer, buffer.Offset, destination, offset, Length);
        }

        /// <summary>
        ///     Converts this writer into a <see cref="MessageBuffer"/>.
        /// </summary>
        /// <returns>A message buffer of this writer.</returns>
        internal IMessageBuffer ToBuffer()
        {
            // Clone it so we maintain our own lifecyle for the underlying memory
            return buffer.Clone();
        }

        /// <summary>
        ///     Recycles this object back into the pool.
        /// </summary>
        public void Dispose()
        {
            buffer.Dispose();

            ObjectCache.ReturnWriter(this);
            isCurrentlyLoungingInAPool = true;
        }

        /// <summary>
        ///     Finalizer so we can inform the cache system we were not recycled correctly.
        /// </summary>
        ~DarkRiftWriter()
        {
            if (!isCurrentlyLoungingInAPool)
                ObjectCacheHelper.DarkRiftWriterWasFinalized();
        }
    }
}
