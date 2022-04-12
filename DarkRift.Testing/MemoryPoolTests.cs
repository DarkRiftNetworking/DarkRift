/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRift.Testing
{
    [TestClass]
    public class MemoryPoolTests
    {
        // TODO This test should DI mock ObjectPool instances and assert on them

        /// <summary>
        ///     The memory pool under test.
        /// </summary>
        private MemoryPool memoryPool;

        [TestInitialize]
        public void Initialize()
        {
            memoryPool = new MemoryPool(16, 4, 64, 4, 256, 4, 1024, 4, 4096, 4);
        }

        [TestMethod]
        public void GetExtraSmallMemoryTest()
        {
            // GIVEN a memory pool with no previously pooled memory

            // WHEN I request an extra small memory block
            byte[] result = memoryPool.GetInstance(10);

            // THEN my memory is an extra small block
            Assert.IsNotNull(result);
            Assert.AreEqual(16, result.Length);
        }

        [TestMethod]
        public void GetSmallMemoryTest()
        {
            // GIVEN a memory pool with no previously pooled memory

            // WHEN I request a small memory block
            byte[] result = memoryPool.GetInstance(20);

            // THEN my memory is a small block
            Assert.IsNotNull(result);
            Assert.AreEqual(64, result.Length);
        }

        [TestMethod]
        public void GetMediumMemoryTest()
        {
            // GIVEN a memory pool with no previously pooled memory

            // WHEN I request a medium memory block
            byte[] result = memoryPool.GetInstance(100);

            // THEN my memory is a medium block
            Assert.IsNotNull(result);
            Assert.AreEqual(256, result.Length);
        }

        [TestMethod]
        public void GetLargeMemoryTest()
        {
            // GIVEN a memory pool with no previously pooled memory

            // WHEN I request a large memory block
            byte[] result = memoryPool.GetInstance(1000);

            // THEN my memory is a large block
            Assert.IsNotNull(result);
            Assert.AreEqual(1024, result.Length);
        }

        [TestMethod]
        public void GetExtraLargeMemoryTest()
        {
            // GIVEN a memory pool with no previously pooled memory

            // WHEN I request an extra large memory block
            byte[] result = memoryPool.GetInstance(3000);

            // THEN my memory is an extra large block
            Assert.IsNotNull(result);
            Assert.AreEqual(4096, result.Length);
        }

        [TestMethod]
        public void GetLargerThanExtraLargeMemoryTest()
        {
            // GIVEN a memory pool with no previously pooled memory

            // WHEN I request a memory block larger than those pooled
            byte[] result = memoryPool.GetInstance(10000);

            // THEN my memory is not a pooled block
            Assert.IsNotNull(result);
            Assert.AreEqual(10000, result.Length);
        }

        [TestMethod]
        public void ReturnSmallerThanExtraSmallMemoryTest()
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

        [TestMethod]
        public void ReturnExtraSmallMemoryTest()
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

        [TestMethod]
        public void ReturnSmallMemoryTest()
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

        [TestMethod]
        public void ReturnMediumMemoryTest()
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

        [TestMethod]
        public void ReturnLargeMemoryTest()
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

        [TestMethod]
        public void ReturnExtraLargeMemoryTest()
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
