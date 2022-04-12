/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.DataStructures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace DarkRift.DataStructures.Testing
{
    [TestClass]
    public class MovingAverageFilterTests
    {
        private MovingAverageFilter filter;

        [TestInitialize]
        public void Initialize()
        {
            filter = new MovingAverageFilter(4);
        }

        [TestMethod]
        public void AverageTest()
        {
            Assert.AreEqual(0, filter.Average);

            //Initial population
            filter.Add(10);
            filter.Add(10);
            filter.Add(7);
            filter.Add(11);

            Assert.AreEqual(9.5, filter.Average);

            //Should push off first 10
            filter.Add(4);

            Assert.AreEqual(8, filter.Average);
        }

        [TestMethod]
        public void ResetTest()
        {
            Assert.AreEqual(0, filter.Average);

            //Initial population
            filter.Add(10);
            filter.Add(5);
            filter.Add(3);
            filter.Add(4);

            Assert.AreEqual(5.5, filter.Average);

            //Check reset removes all values
            filter.Reset();

            Assert.AreEqual(0, filter.Average);

            //Check we can still add elements
            filter.Add(4);

            Assert.AreEqual(1, filter.Average);
        }
    }
}
