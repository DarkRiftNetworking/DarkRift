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
    public class ObjectPoolTests
    {
        /// <summary>
        ///     The object pool under test.
        /// </summary>
        private ObjectPool<object> objectPool;

        /// <summary>
        ///     The new instance that was created if one was.
        /// </summary>
        private object newInstance;

        [TestInitialize]
        public void Initialize()
        {
            newInstance = null;

            objectPool = new ObjectPool<object>(2, () =>
            {
                return newInstance = new object();
            });
        }

        [TestMethod]
        public void GetInstanceGeneratesWhenEmptyTest()
        {
            // GIVEN an empty pool

            // WHEN I get an instance from the pool
            object result = objectPool.GetInstance();

            // THEN a new instance was generated
            Assert.IsNotNull(result);
            Assert.IsNotNull(newInstance);
            Assert.AreSame(newInstance, result);
        }

        [TestMethod]
        public void GetInstanceUsesPoolWhenHasInstancesTest()
        {
            // GIVEN a pool with a single element
            object pooledObject = new object();
            objectPool.ReturnInstance(pooledObject);

            // WHEN I get an instance from the pool
            object result = objectPool.GetInstance();

            // THEN a new instance is not generated
            Assert.IsNull(newInstance);

            // AND the returned instance is the pooled object
            Assert.IsNotNull(result);
            Assert.AreSame(pooledObject, result);
        }

        [TestMethod]
        public void ReturnInstanceWhenPoolIsFullTest()
        {
            // GIVEN a full pool
            objectPool.ReturnInstance(new object());
            objectPool.ReturnInstance(new object());

            // WHEN I return an instance to the pool
            object returnedObject = new object();
            bool result = objectPool.ReturnInstance(returnedObject);

            // THEN the returned instance is rejected
            Assert.IsFalse(result);

            // AND the returned object is not either of the objects in the pool
            Assert.AreNotSame(returnedObject, objectPool.GetInstance());
            Assert.AreNotSame(returnedObject, objectPool.GetInstance());
        }

        [TestMethod]
        public void ReturnInstanceWhenPoolIsEmptyTest()
        {
            // GIVEN an empty pool

            // WHEN I return an instance to the pool
            object returnedObject = new object();
            bool result = objectPool.ReturnInstance(returnedObject);

            // THEN the returned instance is accepted
            Assert.IsTrue(result);

            // AND the returned object is the first to be retuned
            Assert.AreSame(returnedObject, objectPool.GetInstance());
        }
    }
}
