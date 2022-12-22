/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using NUnit.Framework;

namespace DarkRift.Tests
{
    // TODO test exceptions and exception messages

    public class DarkRiftReaderTests
    {
        private MockMessageBuffer messageBuffer;
        private DarkRiftReader reader;

        [SetUp]
        public void SetUp()
        {
            messageBuffer = new MockMessageBuffer();

            // GIVEN the object cache is disabled
#pragma warning disable CS0618      // We don't care about using Server/Client specific cache settings
            ObjectCache.Initialize(ObjectCacheSettings.DontUseCache);
#pragma warning restore CS0618

            // AND a DarkRiftReader under test
            reader = DarkRiftReader.Create(messageBuffer);
        }

        [Test]
        public void ReadByte()
        {
            // GIVEN a buffer of serialized data
            messageBuffer.Buffer = new byte[] { 5 };
            messageBuffer.Offset = 0;
            messageBuffer.Count = 1;

            // WHEN I read a byte from the reader
            byte result = reader.ReadByte();

            // THEN the value is as expected
            Assert.AreEqual((byte)5, result);
        }

        [Test]
        public void ReadChar()
        {
            // GIVEN a buffer of serialized data
            messageBuffer.Buffer = new byte[] { 0, 0, 0, 2, 65, 0 };
            messageBuffer.Offset = 0;
            messageBuffer.Count = 6;

            // WHEN I read a char from the reader
            char result = reader.ReadChar();

            // THEN the value is as expected
            Assert.AreEqual('A', result);
        }

        [Test]
        public void ReadBoolean()
        {
            // GIVEN a buffer of serialized data
            messageBuffer.Buffer = new byte[] { 1 };
            messageBuffer.Offset = 0;
            messageBuffer.Count = 1;

            // WHEN I read a boolean from the reader
            bool result = reader.ReadBoolean();

            // THEN the value is as expected
            Assert.AreEqual(true, result);
        }

        [Test]
        public void ReadDouble()
        {
            // GIVEN a buffer of serialized data
            messageBuffer.Buffer = new byte[] { 0x3f, 0xE8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            messageBuffer.Offset = 0;
            messageBuffer.Count = 10;

            // WHEN I read a double from the reader
            double result = reader.ReadDouble();

            // THEN the value is as expected
            Assert.AreEqual(0.75d, result);
        }

        [Test]
        public void ReadInt16()
        {
            // GIVEN a buffer of serialized data
            messageBuffer.Buffer = new byte[] { 0xE8, 0xA2 };
            messageBuffer.Offset = 0;
            messageBuffer.Count = 2;

            // WHEN I read a short from the reader
            short result = reader.ReadInt16();

            // THEN the value is as expected
            Assert.AreEqual((short)-5982, result);
        }

        [Test]
        public void ReadInt32()
        {
            // GIVEN a buffer of serialized data
            messageBuffer.Buffer = new byte[] { 0x23, 0x24, 0x30, 0x5C };
            messageBuffer.Offset = 0;
            messageBuffer.Count = 4;

            // WHEN I read an int from the reader
            int result = reader.ReadInt32();

            // THEN the value is as expected
            Assert.AreEqual(589574236, result);
        }

        [Test]
        public void ReadInt64()
        {
            // GIVEN a buffer of serialized data
            messageBuffer.Buffer = new byte[] { 0x51, 0xD1, 0xE2, 0x71, 0xCA, 0x29, 0x58, 0x08 };
            messageBuffer.Offset = 0;
            messageBuffer.Count = 8;

            // WHEN I read a long from the reader
            long result = reader.ReadInt64();

            // THEN the value is as expected
            Assert.AreEqual(5895742365555578888L, result);
        }

        [Test]
        public void ReadSByte()
        {
            // GIVEN a buffer of serialized data
            messageBuffer.Buffer = new byte[] { 0xD3 };
            messageBuffer.Offset = 0;
            messageBuffer.Count = 1;

            // WHEN I read an sbyte from the reader
            sbyte result = reader.ReadSByte();

            // THEN the value is as expected
            Assert.AreEqual((sbyte)-45, result);
        }

        [Test]
        public void ReadSingle()
        {
            // GIVEN a buffer of serialized data
            messageBuffer.Buffer = new byte[] { 0x3f, 0x40, 0x00, 0x00 };
            messageBuffer.Offset = 0;
            messageBuffer.Count = 4;

            // WHEN I read a float from the reader
            float result = reader.ReadSingle();

            // THEN the value is as expected
            Assert.AreEqual(0.75f, result);
        }

        [Test]
        public void ReadUInt16()
        {
            // GIVEN a buffer of serialized data
            messageBuffer.Buffer = new byte[] { 0xE8, 0xA2 };
            messageBuffer.Offset = 0;
            messageBuffer.Count = 2;

            // WHEN I read a ushort from the reader
            ushort result = reader.ReadUInt16();

            // THEN the value is as expected
            Assert.AreEqual((ushort)59554, result);
        }

        [Test]
        public void ReadUInt32()
        {
            // GIVEN a buffer of serialized data
            messageBuffer.Buffer = new byte[] { 0x23, 0x24, 0x30, 0x5C };
            messageBuffer.Offset = 0;
            messageBuffer.Count = 4;

            // WHEN I read a uint from the reader
            uint result = reader.ReadUInt32();

            // THEN the value is as expected
            Assert.AreEqual((uint)589574236, result);
        }

        [Test]
        public void ReadUInt64()
        {
            // GIVEN a buffer of serialized data
            messageBuffer.Buffer = new byte[] { 0x51, 0xD1, 0xE2, 0x71, 0xCA, 0x29, 0x58, 0x08 };
            messageBuffer.Offset = 0;
            messageBuffer.Count = 8;

            // WHEN I read a ulong from the reader
            ulong result = reader.ReadUInt64();

            // THEN the value is as expected
            Assert.AreEqual((ulong)5895742365555578888, result);
        }

        [Test]
        public void ReadString()
        {
            // GIVEN a buffer of serialized data
            messageBuffer.Buffer = new byte[] { 0, 0, 0, 6, 65, 0, 66, 0, 67, 0 };
            messageBuffer.Offset = 0;
            messageBuffer.Count = 10;

            // WHEN I read a string from the reader
            string result = reader.ReadString();

            // THEN the value is as expected
            Assert.AreEqual("ABC", result);
        }

        [Test]
        public void ReadBooleans()
        {
            // GIVEN a buffer of serialized data
            messageBuffer.Buffer = new byte[] { 0, 0, 0, 9, 0b11001010, 0b10000000 };
            messageBuffer.Offset = 0;
            messageBuffer.Count = 6;

            // WHEN I read a boolean array from the reader
            bool[] result = reader.ReadBooleans();

            // THEN the value is as expected
            AssertExtensions.AreEqualAndSameLength(new bool[] { true, true, false, false, true, false, true, false, true }, result);
        }
    }
}
