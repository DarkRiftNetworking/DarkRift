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
    ///     Message class for all messages sent through DarkRift.
    /// </summary>
    /// <remarks>
    ///     Since each message is handled by single, separate threads this class is not thread safe.
    /// </remarks>
    public sealed class Message : IDisposable
    {
        /// <summary>
        ///     Bitmask for the command message flag.
        /// </summary>
        private const byte COMMAND_FLAG_MASK = 0b10000000;

        /// <summary>
        ///     Bitmask for the ping message flag.
        /// </summary>
        private const byte IS_PING_FLAG_MASK = 0b01000000;

        /// <summary>
        ///     Bitmask for the type of ping message flag.
        /// </summary>
        private const byte PING_TYPE_FLAG_MASK = 0b00100000;

        /// <summary>
        ///     The buffer behind the message.
        /// </summary>
        private IMessageBuffer buffer;

        /// <summary>
        ///     The number of bytes of data in this message.
        /// </summary>
        public int DataLength => buffer.Count;

        /// <summary>
        ///     Are setters on this object disabled?
        /// </summary>
        // TODO Readonly isn't really needed now that we return copied instances from event args
        public bool IsReadOnly { get; private set; }

        /// <summary>
        ///     Indicates whether this message is a command message or not.
        /// </summary>
        /// <exception cref="AccessViolationException">If the message is readonly.</exception>
        internal bool IsCommandMessage
        {
            get => (flags & COMMAND_FLAG_MASK) != 0;

            set
            {
                if (IsReadOnly)
                {
                    throw new AccessViolationException("Message is read-only. This property can only be set when IsReadOnly is false. You may want to create a writable instance of this Message using Message.Clone().");
                }
                else
                {
                    if (value)
                        flags |= COMMAND_FLAG_MASK;
                    else
                        flags &= byte.MaxValue ^ COMMAND_FLAG_MASK;        //XOR over simple bitwise NOT to avoid entering negative values and ints!
                }
            }
        }

        /// <summary>
        ///     Indicates whether this message is a ping message or not.
        /// </summary>
        public bool IsPingMessage
        {
            get => (flags & IS_PING_FLAG_MASK) != 0 && (flags & PING_TYPE_FLAG_MASK) == 0;

            internal set
            {
                if (value)
                {
                    flags |= IS_PING_FLAG_MASK;
                    flags &= byte.MaxValue ^ PING_TYPE_FLAG_MASK;       //XOR over simple bitwise NOT to avoid entering negative values and ints!
                }
                else
                {
                    flags &= byte.MaxValue ^ IS_PING_FLAG_MASK ^ PING_TYPE_FLAG_MASK;       //XOR over simple bitwise NOT to avoid entering negative values and ints!
                }
            }
        }

        /// <summary>
        ///     Indicates whether this message is a ping acknowledegment message or not.
        /// </summary>
        public bool IsPingAcknowledgementMessage
        {
            get => (flags & IS_PING_FLAG_MASK) != 0 && (flags & PING_TYPE_FLAG_MASK) != 0;

            private set
            {
                if (value)
                {
                    flags |= IS_PING_FLAG_MASK | PING_TYPE_FLAG_MASK;
                }
                else
                {
                    flags &= byte.MaxValue ^ IS_PING_FLAG_MASK ^ PING_TYPE_FLAG_MASK;       //XOR over simple bitwise NOT to avoid entering negative values and ints!
                }
            }
        }

        /// <summary>
        ///     The flags attached to this message.
        /// </summary>
        /// <remarks>
        ///     8th bit - Is Command
        ///     7th bit - Is Ping Attached
        ///     6th bit - Ping (0)/Ping Acknowledgement (1)
        ///     5th bit - Not used
        ///     4th bit - Not used
        ///     3rd bit - Not used
        ///     2nd bit - Not used
        ///     1st bit - Not used
        /// </remarks>
        private byte flags;

        /// <summary>
        ///     The tag of the message.
        /// </summary>
        /// <exception cref="AccessViolationException">If the message is readonly.</exception>
        public ushort Tag
        {
            get => tag;

            set
            {
                if (IsReadOnly)
                    throw new AccessViolationException("Message is read-only. This property can only be set when IsReadOnly is false. You may want to create a writable instance of this Message using Message.Clone().");
                else
                    tag = value;
            }
        }

        private ushort tag;

        /// <summary>
        ///     Code to identify pings and acknowledgements.
        /// </summary>
        internal ushort PingCode { get; private set; }

        /// <summary>
        ///     Random number generator for each thread.
        /// </summary>
        [ThreadStatic]
        private static Random random;

        /// <summary>
        ///     Whether this message is currently in an object pool waiting or not.
        /// </summary>
        private volatile bool isCurrentlyLoungingInAPool;

        /// <summary>
        ///     Creates a new message with the given tag and an empty payload.
        /// </summary>
        /// <param name="tag">The tag the message has.</param>
        public static Message CreateEmpty(ushort tag)
        {
            Message message = ObjectCache.GetMessage();

            message.isCurrentlyLoungingInAPool = false;

            message.IsReadOnly = false;
            message.buffer = MessageBuffer.Create(0);
            message.tag = tag;
            message.flags = 0;
            message.PingCode = 0;
            return message;
        }

        /// <summary>
        ///     Creates a new message with the given tag and writer.
        /// </summary>
        /// <param name="tag">The tag the message has.</param>
        /// <param name="writer">The initial data in the message.</param>
        public static Message Create(ushort tag, DarkRiftWriter writer)
        {
            Message message = ObjectCache.GetMessage();

            message.isCurrentlyLoungingInAPool = false;

            message.IsReadOnly = false;
            message.buffer = writer.ToBuffer();
            message.tag = tag;
            message.flags = 0;
            message.PingCode = 0;
            return message;
        }

        /// <summary>
        ///     Creates a new message with the given tag and serializable object.
        /// </summary>
        /// <param name="tag">The tag the message has.</param>
        /// <param name="obj">The initial object in the message data.</param>
        public static Message Create<T>(ushort tag, T obj) where T : IDarkRiftSerializable
        {
            Message message = ObjectCache.GetMessage();

            message.isCurrentlyLoungingInAPool = false;

            message.IsReadOnly = false;

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(obj);

                message.buffer = writer.ToBuffer();
            }

            message.tag = tag;
            message.flags = 0;
            message.PingCode = 0;
            return message;
        }

        /// <summary>
        ///     Creates a new message with the given tag and serializable object.
        /// </summary>
        /// <param name="tag">The tag the message has.</param>
        /// <param name="obj">The initial object in the message data.</param>
        [Obsolete("Use Create<T>(ushort tag, T serializable) instead.")]
        public static Message Create(ushort tag, IDarkRiftSerializable obj)
        {
            Message message = ObjectCache.GetMessage();

            message.isCurrentlyLoungingInAPool = false;

            message.IsReadOnly = false;

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(obj);

                message.buffer = writer.ToBuffer();
            }

            message.tag = tag;
            message.flags = 0;
            message.PingCode = 0;
            return message;
        }

        /// <summary>
        ///     Creates a new message from the given buffer.
        /// </summary>
        /// <param name="buffer">The buffer containing the message.</param>
        /// <param name="isReadOnly">Whether the message should be created read only or not.</param>
        internal static Message Create(IMessageBuffer buffer, bool isReadOnly)
        {
            Message message = ObjectCache.GetMessage();

            message.isCurrentlyLoungingInAPool = false;

            // We clone the message buffer so we can modify it's properties safely
            message.buffer = buffer.Clone();
            
            //Get flags first so we can query it
            message.flags = buffer.Buffer[buffer.Offset];

            //Ping messages have an extra 2 byte header
            int headerLength = message.IsPingMessage || message.IsPingAcknowledgementMessage ? 5 : 3;
            message.buffer.Offset = buffer.Offset + headerLength;
            message.buffer.Count = buffer.Count - headerLength;

            message.IsReadOnly = isReadOnly;

            message.tag = BigEndianHelper.ReadUInt16(buffer.Buffer, buffer.Offset + 1);
            message.PingCode = (ushort)(message.IsPingMessage || message.IsPingAcknowledgementMessage ? BigEndianHelper.ReadUInt16(buffer.Buffer, buffer.Offset + 3) : 0);
            return message;
        }

        /// <summary>
        ///     Creates a new Message. For use from the ObjectCache.
        /// </summary>
        internal Message()
        {
            
        }

        /// <summary>
        ///     Clears the data in this message.
        /// </summary>
        public void Empty()
        {
            if (IsReadOnly)
                throw new AccessViolationException("Message is read-only. This property can only be set when IsReadOnly is false. You may want to create a writable instance of this Message using Message.Clone().");

            // To avoid corrupting the shared memory just get rid of the buffer and create a new one
            buffer.Dispose();
            buffer = MessageBuffer.Create(0);
        }

        /// <summary>
        ///     Creates a DarkRiftReader to read the data in the message.
        /// </summary>
        /// <returns>A DarkRiftReader for the message.</returns>
        public DarkRiftReader GetReader()
        {
            // Clone the buffer so the reader has it's own lifecycle for the underlying memory
            return DarkRiftReader.Create(buffer.Clone());
        }

        /// <summary>
        ///     Serializes a <see cref="DarkRiftWriter"/> into the data of this message.
        /// </summary>
        /// <param name="writer">The writer to serialize.</param>
        /// <exception cref="AccessViolationException">If the message is readonly.</exception>
        public void Serialize(DarkRiftWriter writer)
        {
            if (IsReadOnly)
                throw new AccessViolationException("Message is read-only. This property can only be set when IsReadOnly is false. You may want to create a writable instance of this Message using Message.Clone().");

            buffer.Dispose();
            buffer = writer.ToBuffer();
        }

        /// <summary>
        ///     Deserializes the data to the given object type.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize to.</typeparam>
        /// <returns>The deserialized object.</returns>
        public T Deserialize<T>() where T : IDarkRiftSerializable, new()
        {
            using (DarkRiftReader reader = GetReader())
                return reader.ReadSerializable<T>();
        }

        /// <summary>
        ///     Deserializes the data to the given object.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize to.</typeparam>
        /// <param name="t">The object to deserialize the data into.</param>
        public void DeserializeInto<T>(ref T t) where T : IDarkRiftSerializable
        {
            using (DarkRiftReader reader = GetReader())
                reader.ReadSerializableInto<T>(ref t);
        }

        /// <summary>
        ///     Serializes an object into the data of this message.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <exception cref="AccessViolationException">If the message is readonly.</exception>
        [Obsolete("Use Serialize<T>(T obj) instead.")]
        public void Serialize(IDarkRiftSerializable obj)
        {
            if (IsReadOnly)
                throw new AccessViolationException("Message is read-only. This property can only be set when IsReadOnly is false. You may want to create a writable instance of this Message using Message.Clone().");

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(obj);

                Serialize(writer);
            }
        }

        /// <summary>
        ///     Serializes an object into the data of this message.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <exception cref="AccessViolationException">If the message is readonly.</exception>
        public void Serialize<T>(T obj) where T : IDarkRiftSerializable
        {
            if (IsReadOnly)
                throw new AccessViolationException("Message is read-only. This property can only be set when IsReadOnly is false. You may want to create a writable instance of this Message using Message.Clone().");

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(obj);

                Serialize(writer);
            }
        }

        /// <summary>
        ///     Makes this a ping message and generates it a random ping identification.
        /// </summary>
        public void MakePingMessage()
        {
            IsPingMessage = true;

            if (random == null)
                random = new Random();
            PingCode = (ushort)random.Next();
        }

        /// <summary>
        ///     Makes this a ping acknowledgement message for the given ping message.
        /// </summary>
        /// <exception cref="ArgumentException">If the message passed is not a ping message.</exception>
        public void MakePingAcknowledgementMessage(Message acknowledging)
        {
            if (!acknowledging.IsPingMessage)
                throw new ArgumentException("Message to acknowledge is not a ping message so cannot be used here. You can check if a message is a ping message using the Message.IsPingMessage property.");

            IsPingAcknowledgementMessage = true;
            PingCode = acknowledging.PingCode;
        }

        /// <summary>
        ///     Converts this message into a buffer.
        /// </summary>
        /// <returns>The buffer.</returns>
        //TODO DR3 Make this return an IMessageBuffer
        internal MessageBuffer ToBuffer()
        {
            int headerLength = IsPingMessage || IsPingAcknowledgementMessage ? 5 : 3;
            int totalLength = headerLength + DataLength;

            MessageBuffer buffer = MessageBuffer.Create(totalLength);
            buffer.Count = totalLength;

            buffer.Buffer[buffer.Offset] = flags;
            BigEndianHelper.WriteBytes(buffer.Buffer, buffer.Offset + 1, tag);

            if (IsPingMessage || IsPingAcknowledgementMessage)
                BigEndianHelper.WriteBytes(buffer.Buffer, buffer.Offset + 3, PingCode);

            //Due to poor design, here's un unavoidable memory copy! Hooray!
            Buffer.BlockCopy(this.buffer.Buffer, this.buffer.Offset, buffer.Buffer, buffer.Offset + headerLength, this.buffer.Count);

            return buffer;
        }

        /// <summary>
        ///     Performs a shallow copy of the message.
        /// </summary>
        /// <returns>A new instance of the message.</returns>
        public Message Clone()
        {
            Message message = ObjectCache.GetMessage();

            //We don't want to give a reference to our buffer so we need to clone it
            message.buffer = buffer.Clone();
            
            message.flags = flags;
            message.tag = tag;
            message.PingCode = PingCode;
            return message;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Message with tag '{Tag}' and {DataLength} bytes of data.";
        }

        /// <summary>
        ///     Recycles this object back into the pool.
        /// </summary>
        public void Dispose()
        {
            buffer.Dispose();

            ObjectCache.ReturnMessage(this);
            isCurrentlyLoungingInAPool = true;
        }

        /// <summary>
        ///     Finalizer so we can inform the cache system we were not recycled correctly.
        /// </summary>
        ~Message()
        {
            if (!isCurrentlyLoungingInAPool)
                ObjectCacheHelper.MessageWasFinalized();
        }
    }
}
