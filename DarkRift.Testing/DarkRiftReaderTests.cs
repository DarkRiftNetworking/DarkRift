/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRift.Testing
{
    // TODO test exceptions and exception messages

    [TestClass]
    public class DarkRiftReaderTests
    {
        private Mock<IMessageBuffer> mockMessageBuffer = new Mock<IMessageBuffer>();

        private DarkRiftReader reader;

        [TestInitialize]
        public void Initialize()
        {
            // GIVEN the object cache is disabled
#pragma warning disable CS0618      // We don't care about using Server/Client specific cache settings
            ObjectCache.Initialize(ObjectCacheSettings.DontUseCache);
#pragma warning restore CS0618

            // AND a DarkRiftReader under test
            reader = DarkRiftReader.Create(mockMessageBuffer.Object);
        }

        [TestMethod]
        public void ReadByteTest()
        {
            // GIVEN a buffer of serialized data
            mockMessageBuffer.Setup(m => m.Buffer).Returns(new byte[] { 5 });
            mockMessageBuffer.Setup(m => m.Offset).Returns(0);
            mockMessageBuffer.Setup(m => m.Count).Returns(1);

            // WHEN I read a byte from the reader
            byte result = reader.ReadByte();
            
            // THEN the value is as expected
            Assert.AreEqual((byte)5, result);
        }

        [TestMethod]
        public void ReadCharTest()
        {
            // GIVEN a buffer of serialized data
            mockMessageBuffer.Setup(m => m.Buffer).Returns(new byte[] { 0, 0, 0, 2, 65, 0 });
            mockMessageBuffer.Setup(m => m.Offset).Returns(0);
            mockMessageBuffer.Setup(m => m.Count).Returns(6);

            // WHEN I read a char from the reader
            char result = reader.ReadChar();
            
            // THEN the value is as expected
            Assert.AreEqual('A', result);
        }

        [TestMethod]
        public void ReadBooleanTest()
        {
            // GIVEN a buffer of serialized data
            mockMessageBuffer.Setup(m => m.Buffer).Returns(new byte[] { 1 });
            mockMessageBuffer.Setup(m => m.Offset).Returns(0);
            mockMessageBuffer.Setup(m => m.Count).Returns(1);

            // WHEN I read a boolean from the reader
            bool result = reader.ReadBoolean();
            
            // THEN the value is as expected
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void ReadDoubleTest()
        {
            // GIVEN a buffer of serialized data
            mockMessageBuffer.Setup(m => m.Buffer).Returns(new byte[] { 0x3f, 0xE8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            mockMessageBuffer.Setup(m => m.Offset).Returns(0);
            mockMessageBuffer.Setup(m => m.Count).Returns(10);

            // WHEN I read a double from the reader
            double result = reader.ReadDouble();

            // THEN the value is as expected
            Assert.AreEqual(0.75d, result);
        }
        
        [TestMethod]
        public void ReadInt16Test()
        {
            // GIVEN a buffer of serialized data
            mockMessageBuffer.Setup(m => m.Buffer).Returns(new byte[] { 0xE8, 0xA2 });
            mockMessageBuffer.Setup(m => m.Offset).Returns(0);
            mockMessageBuffer.Setup(m => m.Count).Returns(2);

            // WHEN I read a short from the reader
            short result = reader.ReadInt16();

            // THEN the value is as expected
            Assert.AreEqual((short)-5982, result);
        }

        [TestMethod]
        public void ReadInt32Test()
        {
            // GIVEN a buffer of serialized data
            mockMessageBuffer.Setup(m => m.Buffer).Returns(new byte[] { 0x23, 0x24, 0x30, 0x5C });
            mockMessageBuffer.Setup(m => m.Offset).Returns(0);
            mockMessageBuffer.Setup(m => m.Count).Returns(4);

            // WHEN I read an int from the reader
            int result = reader.ReadInt32();

            // THEN the value is as expected
            Assert.AreEqual(589574236, result);
        }

        [TestMethod]
        public void ReadInt64Test()
        {
            // GIVEN a buffer of serialized data
            mockMessageBuffer.Setup(m => m.Buffer).Returns(new byte[] { 0x51, 0xD1, 0xE2, 0x71, 0xCA, 0x29, 0x58, 0x08 });
            mockMessageBuffer.Setup(m => m.Offset).Returns(0);
            mockMessageBuffer.Setup(m => m.Count).Returns(8);

            // WHEN I read a long from the reader
            long result = reader.ReadInt64();

            // THEN the value is as expected
            Assert.AreEqual(5895742365555578888L, result);
        }

        [TestMethod]
        public void ReadSByteTest()
        {
            // GIVEN a buffer of serialized data
            mockMessageBuffer.Setup(m => m.Buffer).Returns(new byte[] { 0xD3 });
            mockMessageBuffer.Setup(m => m.Offset).Returns(0);
            mockMessageBuffer.Setup(m => m.Count).Returns(1);

            // WHEN I read an sbyte from the reader
            sbyte result = reader.ReadSByte();

            // THEN the value is as expected
            Assert.AreEqual((sbyte)-45, result);
        }

        [TestMethod]
        public void ReadSingleTest()
        {
            // GIVEN a buffer of serialized data
            mockMessageBuffer.Setup(m => m.Buffer).Returns(new byte[] { 0x3f, 0x40, 0x00, 0x00 });
            mockMessageBuffer.Setup(m => m.Offset).Returns(0);
            mockMessageBuffer.Setup(m => m.Count).Returns(4);

            // WHEN I read a float from the reader
            float result = reader.ReadSingle();

            // THEN the value is as expected
            Assert.AreEqual(0.75f, result);
        }

        [TestMethod]
        public void ReadUInt16Test()
        {
            // GIVEN a buffer of serialized data
            mockMessageBuffer.Setup(m => m.Buffer).Returns(new byte[] { 0xE8, 0xA2 });
            mockMessageBuffer.Setup(m => m.Offset).Returns(0);
            mockMessageBuffer.Setup(m => m.Count).Returns(2);

            // WHEN I read a ushort from the reader
            ushort result = reader.ReadUInt16();

            // THEN the value is as expected
            Assert.AreEqual((ushort)59554, result);
        }

        [TestMethod]
        public void ReadUInt32Test()
        {
            // GIVEN a buffer of serialized data
            mockMessageBuffer.Setup(m => m.Buffer).Returns(new byte[] { 0x23, 0x24, 0x30, 0x5C });
            mockMessageBuffer.Setup(m => m.Offset).Returns(0);
            mockMessageBuffer.Setup(m => m.Count).Returns(4);

            // WHEN I read a uint from the reader
            uint result = reader.ReadUInt32();

            // THEN the value is as expected
            Assert.AreEqual((uint)589574236, result);
        }

        [TestMethod]
        public void ReadUInt64Test()
        {
            // GIVEN a buffer of serialized data
            mockMessageBuffer.Setup(m => m.Buffer).Returns(new byte[] { 0x51, 0xD1, 0xE2, 0x71, 0xCA, 0x29, 0x58, 0x08 });
            mockMessageBuffer.Setup(m => m.Offset).Returns(0);
            mockMessageBuffer.Setup(m => m.Count).Returns(8);

            // WHEN I read a ulong from the reader
            ulong result = reader.ReadUInt64();

            // THEN the value is as expected
            Assert.AreEqual((ulong)5895742365555578888, result);
        }

        [TestMethod]
        public void ReadStringTest()
        {
            // GIVEN a buffer of serialized data
            mockMessageBuffer.Setup(m => m.Buffer).Returns(new byte[] { 0, 0, 0, 6, 65, 0, 66, 0, 67, 0 });
            mockMessageBuffer.Setup(m => m.Offset).Returns(0);
            mockMessageBuffer.Setup(m => m.Count).Returns(10);

            // WHEN I read a string from the reader
            string result = reader.ReadString();

            // THEN the value is as expected
            Assert.AreEqual("ABC", result);
        }

        [TestMethod]
        public void ReadBooleansTest()
        {
            // GIVEN a buffer of serialized data
            mockMessageBuffer.Setup(m => m.Buffer).Returns(new byte[] { 0, 0, 0, 9, 0b11001010, 0b10000000 });
            mockMessageBuffer.Setup(m => m.Offset).Returns(0);
            mockMessageBuffer.Setup(m => m.Count).Returns(6);

            // WHEN I read a boolean array from the reader
            bool[] result = reader.ReadBooleans();

            // THEN the value is as expected
            AssertExtensions.AreEqualAndSameLength(new bool[] { true, true, false, false, true, false, true, false, true }, result);
        }
    }
}
