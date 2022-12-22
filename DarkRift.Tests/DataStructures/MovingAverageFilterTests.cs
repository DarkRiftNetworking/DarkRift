/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using NUnit.Framework;

namespace DarkRift.DataStructures.Tests
{
    public class MovingAverageFilterTests
    {
        private MovingAverageFilter filter;

        [SetUp]
        public void SetUp()
        {
            filter = new MovingAverageFilter(4);
        }

        [Test]
        public void Average()
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

        [Test]
        public void Reset()
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
