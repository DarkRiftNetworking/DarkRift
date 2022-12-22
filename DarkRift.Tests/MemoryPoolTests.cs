/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using NUnit.Framework;

namespace DarkRift.Tests
{
    public class MemoryPoolTests
    {
        // TODO This test should DI mock ObjectPool instances and assert on them

        /// <summary>
        ///     The memory pool under test.
        /// </summary>
        private MemoryPool memoryPool;

        [SetUp]
        public void SetUp()
        {
            memoryPool = new MemoryPool(16, 4, 64, 4, 256, 4, 1024, 4, 4096, 4);
        }

        [Test]
        public void GetExtraSmallMemory()
        {
            // GIVEN a memory pool with no previously pooled memory

            // WHEN I request an extra small memory block
            byte[] result = memoryPool.GetInstance(10);

            // THEN my memory is an extra small block
            Assert.IsNotNull(result);
            Assert.AreEqual(16, result.Length);
        }

        [Test]
        public void GetSmallMemory()
        {
            // GIVEN a memory pool with no previously pooled memory

            // WHEN I request a small memory block
            byte[] result = memoryPool.GetInstance(20);

            // THEN my memory is a small block
            Assert.IsNotNull(result);
            Assert.AreEqual(64, result.Length);
        }

        [Test]
        public void GetMediumMemory()
        {
            // GIVEN a memory pool with no previously pooled memory

            // WHEN I request a medium memory block
            byte[] result = memoryPool.GetInstance(100);

            // THEN my memory is a medium block
            Assert.IsNotNull(result);
            Assert.AreEqual(256, result.Length);
        }

        [Test]
        public void GetLargeMemory()
        {
            // GIVEN a memory pool with no previously pooled memory

            // WHEN I request a large memory block
            byte[] result = memoryPool.GetInstance(1000);

            // THEN my memory is a large block
            Assert.IsNotNull(result);
            Assert.AreEqual(1024, result.Length);
        }

        [Test]
        public void GetExtraLargeMemory()
        {
            // GIVEN a memory pool with no previously pooled memory

            // WHEN I request an extra large memory block
            byte[] result = memoryPool.GetInstance(3000);

            // THEN my memory is an extra large block
            Assert.IsNotNull(result);
            Assert.AreEqual(4096, result.Length);
        }

        [Test]
        public void GetLargerThanExtraLargeMemory()
        {
            // GIVEN a memory pool with no previously pooled memory

            // WHEN I request a memory block larger than those pooled
            byte[] result = memoryPool.GetInstance(10000);

            // THEN my memory is not a pooled block
            Assert.IsNotNull(result);
            Assert.AreEqual(10000, result.Length);
        }

        [Test]
        public void ReturnSmallerThanExtraSmallMemory()
        {
            // GIVEN a memory pool with no previously pooled memory

            // WHEN I return an memory block smaller than extra small
            byte[] oldBlock = new byte[10];
            memoryPool.ReturnInstance(oldBlock);

            // AND I request a new extra small memory block
            byte[] newBlock = memoryPool.GetInstance(10);

            // THEN my memory is not a pooled block
            Assert.IsNotNull(newBlock);
            Assert.AreEqual(16, newBlock.Length);
        }

        [Test]
        public void ReturnExtraSmallMemory()
        {
            // GIVEN a memory pool with no previously pooled memory

            // WHEN I return an extra small memory block
            byte[] oldBlock = new byte[20];
            memoryPool.ReturnInstance(oldBlock);

            // AND I request a new extra small memory block
            byte[] newBlock = memoryPool.GetInstance(10);

            // THEN my memory block is the same
            Assert.AreSame(oldBlock, newBlock);
        }

        [Test]
        public void ReturnSmallMemory()
        {
            // GIVEN a memory pool with no previously pooled memory

            // WHEN I return a small memory block
            byte[] oldBlock = new byte[80];
            memoryPool.ReturnInstance(oldBlock);

            // AND I request a new small memory block
            byte[] newBlock = memoryPool.GetInstance(60);

            // THEN my memory block is the same
            Assert.AreSame(oldBlock, newBlock);
        }

        [Test]
        public void ReturnMediumMemory()
        {
            // GIVEN a memory pool with no previously pooled memory

            // WHEN I return a medium memory block
            byte[] oldBlock = new byte[300];
            memoryPool.ReturnInstance(oldBlock);

            // AND I request a new medium memory block
            byte[] newBlock = memoryPool.GetInstance(200);

            // THEN my memory block is the same
            Assert.AreSame(oldBlock, newBlock);
        }

        [Test]
        public void ReturnLargeMemory()
        {
            // GIVEN a memory pool with no previously pooled memory

            // WHEN I return a large memory block
            byte[] oldBlock = new byte[2000];
            memoryPool.ReturnInstance(oldBlock);

            // AND I request a new large memory block
            byte[] newBlock = memoryPool.GetInstance(1000);

            // THEN my memory block is the same
            Assert.AreSame(oldBlock, newBlock);
        }

        [Test]
        public void ReturnExtraLargeMemory()
        {
            // GIVEN a memory pool with no previously pooled memory

            // WHEN I return an extra large memory block
            byte[] oldBlock = new byte[5000];
            memoryPool.ReturnInstance(oldBlock);

            // AND I request a new extra large memory block
            byte[] newBlock = memoryPool.GetInstance(4000);

            // THEN my memory block is the same
            Assert.AreSame(oldBlock, newBlock);
        }
    }
}
