/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.DataStructures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRift.DataStructures.Testing
{
    [TestClass]
    public class CircularDictionaryTests
    {
        private CircularDictionary<int, int> dictionary;

        [TestInitialize]
        public void Initialize()
        {
            dictionary = new CircularDictionary<int, int>(4);
        }

        [TestMethod]
        public void LookupTest()
        {
            Assert.ThrowsException<KeyNotFoundException>(() => dictionary[9]);

            //Initial population
            dictionary.Add(10, 5);
            dictionary.Add(12, 6);
            dictionary.Add(14, 7);
            dictionary.Add(16, 8);

            Assert.AreEqual(5, dictionary[10]);
            Assert.AreEqual(6, dictionary[12]);
            Assert.AreEqual(7, dictionary[14]);
            Assert.AreEqual(8, dictionary[16]);

            //Should push off first 10
            dictionary.Add(18, 9);

            Assert.ThrowsException<KeyNotFoundException>(() => dictionary[10]);

            Assert.AreEqual(9, dictionary[18]);
        }
    }
}
