/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DarkRift.Dispatching;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DarkRift.Testing
{
    [TestClass]
    public class DarkRiftWriterTests
    {
        private DarkRiftWriter writer;

        [TestInitialize]
        public void Initialize()
        {
            // GIVEN the object cache is disabled
#pragma warning disable CS0618      // We don't care about using Server/Client specific cache settings
            ObjectCache.Initialize(ObjectCacheSettings.DontUseCache);
#pragma warning restore CS0618

            // AND a DarkRiftWriter under test
            writer = DarkRiftWriter.Create();
        }

        [TestMethod]
        public void WriteByteTest()
        {
            // WHEN I write a byte to the writer
            writer.Write((byte)5);

            // AND I convert the writer to a Buffer
            IMessageBuffer buffer = writer.ToBuffer();

            // THEN the buffer is as expected
            AssertExtensions.AreEqualAndNotShorter(new byte[] { 5 }, buffer.Buffer);
            Assert.AreEqual(0, buffer.Offset);
            Assert.AreEqual(1, buffer.Count);

            // AND the writer has advanced its pointers
            Assert.AreEqual(1, writer.Position);
            Assert.AreEqual(1, writer.Length);
        }

        [TestMethod]
        public void WriteCharTest()
        {
            // WHEN I write a char to the writer
            writer.Write('A');

            // AND I convert the writer to a Buffer
            IMessageBuffer buffer = writer.ToBuffer();

            // THEN the buffer is as expected
            AssertExtensions.AreEqualAndNotShorter(new byte[] { 0, 0, 0, 2, 65, 0 }, buffer.Buffer);
            Assert.AreEqual(0, buffer.Offset);
            Assert.AreEqual(6, buffer.Count);

            // AND the writer has advanced its pointers
            Assert.AreEqual(6, writer.Position);
            Assert.AreEqual(6, writer.Length);
        }

        [TestMethod]
        public void WriteBooleanTest()
        {
            // WHEN I write a boolean to the writer
            writer.Write(true);

            // AND I convert the writer to a Buffer
            IMessageBuffer buffer = writer.ToBuffer();

            // THEN the buffer is as expected
            AssertExtensions.AreEqualAndNotShorter(new byte[] { 1 }, buffer.Buffer);
            Assert.AreEqual(0, buffer.Offset);
            Assert.AreEqual(1, buffer.Count);

            // AND the writer has advanced its pointers
            Assert.AreEqual(1, writer.Position);
            Assert.AreEqual(1, writer.Length);
        }

        [TestMethod]
        public void WriteDoubleTest()
        {
            // WHEN I write a double to the writer
            writer.Write(0.75d);

            // AND I convert the writer to a Buffer
            IMessageBuffer buffer = writer.ToBuffer();

            // THEN the buffer is as expected
            AssertExtensions.AreEqualAndNotShorter(new byte[] { 0x3f, 0xE8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, buffer.Buffer);
            Assert.AreEqual(0, buffer.Offset);
            Assert.AreEqual(8, buffer.Count);

            // AND the writer has advanced its pointers
            Assert.AreEqual(8, writer.Position);
            Assert.AreEqual(8, writer.Length);
        }

        [TestMethod]
        public void WriteInt16Test()
        {
            // WHEN I write a short to the writer
            writer.Write((short)-5982);

            // AND I convert the writer to a Buffer
            IMessageBuffer buffer = writer.ToBuffer();

            // THEN the buffer is as expected
            AssertExtensions.AreEqualAndNotShorter(new byte[] { 0xE8, 0xA2 }, buffer.Buffer);
            Assert.AreEqual(0, buffer.Offset);
            Assert.AreEqual(2, buffer.Count);

            // AND the writer has advanced its pointers
            Assert.AreEqual(2, writer.Position);
            Assert.AreEqual(2, writer.Length);
        }

        [TestMethod]
        public void WriteInt32Test()
        {
            // WHEN I write an int to the writer
            writer.Write(589574236);

            // AND I convert the writer to a Buffer
            IMessageBuffer buffer = writer.ToBuffer();

            // THEN the buffer is as expected
            AssertExtensions.AreEqualAndNotShorter(new byte[] { 0x23, 0x24, 0x30, 0x5C }, buffer.Buffer);
            Assert.AreEqual(0, buffer.Offset);
            Assert.AreEqual(4, buffer.Count);

            // AND the writer has advanced its pointers
            Assert.AreEqual(4, writer.Position);
            Assert.AreEqual(4, writer.Length);
        }

        [TestMethod]
        public void WriteInt64Test()
        {
            // WHEN I write a long to the writer
            writer.Write(5895742365555578888);

            // AND I convert the writer to a Buffer
            IMessageBuffer buffer = writer.ToBuffer();

            // THEN the buffer is as expected
            AssertExtensions.AreEqualAndNotShorter(new byte[] { 0x51, 0xD1, 0xE2, 0x71, 0xCA, 0x29, 0x58, 0x08 }, buffer.Buffer);
            Assert.AreEqual(0, buffer.Offset);
            Assert.AreEqual(8, buffer.Count);

            // AND the writer has advanced its pointers
            Assert.AreEqual(8, writer.Position);
            Assert.AreEqual(8, writer.Length);
        }

        [TestMethod]
        public void WriteSByteTest()
        {
            // WHEN I write an sbyte to the writer
            writer.Write((sbyte)-45);

            // AND I convert the writer to a Buffer
            IMessageBuffer buffer = writer.ToBuffer();

            // THEN the buffer is as expected
            AssertExtensions.AreEqualAndNotShorter(new byte[] { 0xD3 }, buffer.Buffer);
            Assert.AreEqual(0, buffer.Offset);
            Assert.AreEqual(1, buffer.Count);

            // AND the writer has advanced its pointers
            Assert.AreEqual(1, writer.Position);
            Assert.AreEqual(1, writer.Length);
        }

        [TestMethod]
        public void WriteFloatTest()
        {
            // WHEN I write a float to the writer
            writer.Write(0.75f);

            // AND I convert the writer to a Buffer
            IMessageBuffer buffer = writer.ToBuffer();

            // THEN the buffer is as expected
            AssertExtensions.AreEqualAndNotShorter(new byte[] { 0x3f, 0x40, 0x00, 0x00 }, buffer.Buffer);
            Assert.AreEqual(0, buffer.Offset);
            Assert.AreEqual(4, buffer.Count);

            // AND the writer has advanced its pointers
            Assert.AreEqual(4, writer.Position);
            Assert.AreEqual(4, writer.Length);
        }

        [TestMethod]
        public void WriteUInt16Test()
        {
            // WHEN I write a ushort to the writer
            writer.Write((ushort)59554);

            // AND I convert the writer to a Buffer
            IMessageBuffer buffer = writer.ToBuffer();

            // THEN the buffer is as expected
            AssertExtensions.AreEqualAndNotShorter(new byte[] { 0xE8, 0xA2 }, buffer.Buffer);
            Assert.AreEqual(0, buffer.Offset);
            Assert.AreEqual(2, buffer.Count);

            // AND the writer has advanced its pointers
            Assert.AreEqual(2, writer.Position);
            Assert.AreEqual(2, writer.Length);
        }

        [TestMethod]
        public void WriteUInt32Test()
        {
            // WHEN I write a uint to the writer
            writer.Write((uint)589574236);

            // AND I convert the writer to a Buffer
            IMessageBuffer buffer = writer.ToBuffer();

            // THEN the buffer is as expected
            AssertExtensions.AreEqualAndNotShorter(new byte[] { 0x23, 0x24, 0x30, 0x5C }, buffer.Buffer);
            Assert.AreEqual(0, buffer.Offset);
            Assert.AreEqual(4, buffer.Count);

            // AND the writer has advanced its pointers
            Assert.AreEqual(4, writer.Position);
            Assert.AreEqual(4, writer.Length);
        }

        [TestMethod]
        public void WriteUInt64Test()
        {
            // WHEN I write a ulong to the writer
            writer.Write((ulong)5895742365555578888);

            // AND I convert the writer to a Buffer
            IMessageBuffer buffer = writer.ToBuffer();

            // THEN the buffer is as expected
            AssertExtensions.AreEqualAndNotShorter(new byte[] { 0x51, 0xD1, 0xE2, 0x71, 0xCA, 0x29, 0x58, 0x08 }, buffer.Buffer);
            Assert.AreEqual(0, buffer.Offset);
            Assert.AreEqual(8, buffer.Count);

            // AND the writer has advanced its pointers
            Assert.AreEqual(8, writer.Position);
            Assert.AreEqual(8, writer.Length);
        }

        [TestMethod]
        public void WriteStringTest()
        {
            // WHEN I write a string to the writer
            writer.Write("ABC");

            // AND I convert the writer to a Buffer
            IMessageBuffer buffer = writer.ToBuffer();

            // THEN the buffer is as expected
            AssertExtensions.AreEqualAndNotShorter(new byte[] { 0, 0, 0, 6, 65, 0, 66, 0, 67, 0 }, buffer.Buffer);
            Assert.AreEqual(0, buffer.Offset);
            Assert.AreEqual(10, buffer.Count);

            // AND the writer has advanced its pointers
            Assert.AreEqual(10, writer.Position);
            Assert.AreEqual(10, writer.Length);
        }

        [TestMethod]
        public void WriteBooleansTest()
        {
            // WHEN I write a boolean array to the writer
            writer.Write(new bool[] { true, true, false, false, true, false, true, false, true });

            // AND I convert the writer to a Buffer
            IMessageBuffer buffer = writer.ToBuffer();

            // THEN the buffer is as expected
            AssertExtensions.AreEqualAndNotShorter(new byte[] { 0, 0, 0, 9, 0b11001010, 0b10000000 }, buffer.Buffer);
            Assert.AreEqual(0, buffer.Offset);
            Assert.AreEqual(6, buffer.Count);

            // AND the writer has advanced its pointers
            Assert.AreEqual(6, writer.Position);
            Assert.AreEqual(6, writer.Length);
        }

        [TestMethod]
        public void ToArrayTest()
        {
            // WHEN I write a string to the writer
            writer.Write("ABC");

            // AND I convert the writer to an array
            byte[] array = writer.ToArray();

            // THEN the array is as expected
            AssertExtensions.AreEqualAndSameLength(new byte[] { 0, 0, 0, 6, 65, 0, 66, 0, 67, 0 }, array);
        }
    }
}
