/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using NUnit.Framework;

namespace DarkRift.Server.Plugins.Matchmaking.Tests
{
    public class MatchmakerRankingBuilderTests
    {
        private MatchmakerRankingBuilder builder;

        [SetUp]
        public void SetUp()
        {
            builder = new MatchmakerRankingBuilder();
        }

        [Test]
        public void MinimiseDifferenceLinearFloat()
        {
            //Zero difference
            builder.MinimiseDifferenceLinear(0.5f, 0.5f, 1f, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Maximum difference
            builder.MinimiseDifferenceLinear(0f, 1f, 1f, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Half difference
            builder.MinimiseDifferenceLinear(0.25f, 0.75f, 1f, 0.2f);

            Assert.AreEqual(0.6f, builder.Ranking);
        }

        [Test]
        public void MinimiseDifferenceLinearDouble()
        {
            //Zero difference
            builder.MinimiseDifferenceLinear(0.5, 0.5, 1.0, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Maximum difference
            builder.MinimiseDifferenceLinear(0.0, 1.0, 1.0, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Half difference
            builder.MinimiseDifferenceLinear(0.25, 0.75, 1, 0.2f);

            Assert.AreEqual(0.6f, builder.Ranking);
        }

        [Test]
        public void MinimiseDifferenceLinearInt()
        {
            //Zero difference
            builder.MinimiseDifferenceLinear(5, 5, 10, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Maximum difference
            builder.MinimiseDifferenceLinear(0, 10, 10, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Half difference
            builder.MinimiseDifferenceLinear(2, 7, 10, 0.2f);

            Assert.AreEqual(0.6f, builder.Ranking);
        }

        [Test]
        public void MinimiseDifferenceLinearLong()
        {
            //Zero difference
            builder.MinimiseDifferenceLinear(5L, 5L, 10L, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Maximum difference
            builder.MinimiseDifferenceLinear(0L, 10L, 10L, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Half difference
            builder.MinimiseDifferenceLinear(2L, 7L, 10L, 0.2f);

            Assert.AreEqual(0.6f, builder.Ranking);
        }

        [Test]
        public void MaximiseDifferenceLinearFloat()
        {
            //Maximum difference
            builder.MaximiseDifferenceLinear(0f, 1f, 1f, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Zero difference
            builder.MaximiseDifferenceLinear(0.5f, 0.5f, 1f, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Half difference
            builder.MaximiseDifferenceLinear(0.25f, 0.75f, 1f, 0.2f);

            Assert.AreEqual(0.6f, builder.Ranking);
        }

        [Test]
        public void MaximiseDifferenceLinearDouble()
        {
            //Maximum difference
            builder.MaximiseDifferenceLinear(0, 1, 1, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Zero difference
            builder.MaximiseDifferenceLinear(0.5, 0.5, 1, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Half difference
            builder.MaximiseDifferenceLinear(0.25, 0.75, 1, 0.2f);

            Assert.AreEqual(0.6f, builder.Ranking);
        }

        [Test]
        public void MaximiseDifferenceLinearInt()
        {
            //Maximum difference
            builder.MaximiseDifferenceLinear(0, 10, 10, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Zero difference
            builder.MaximiseDifferenceLinear(5, 5, 10, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Half difference
            builder.MaximiseDifferenceLinear(2, 7, 10, 0.2f);

            Assert.AreEqual(0.6f, builder.Ranking);
        }

        [Test]
        public void MaximiseDifferenceLinearLong()
        {
            //Maximum difference
            builder.MaximiseDifferenceLinear(0L, 10L, 10L, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Zero difference
            builder.MaximiseDifferenceLinear(5L, 5L, 10L, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Half difference
            builder.MaximiseDifferenceLinear(2L, 7L, 10L, 0.2f);

            Assert.AreEqual(0.6f, builder.Ranking);
        }

        [Test]
        public void EqualFloat()
        {
            //Equal
            builder.Equal(1f, 1f, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Not equal
            builder.Equal(0f, 1f, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);
        }

        [Test]
        public void EqualDouble()
        {
            //Equal
            builder.Equal(1.0, 1.0, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Not equal
            builder.Equal(0.0, 1.0, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);
        }

        [Test]
        public void EqualInt()
        {
            //Equal
            builder.Equal(1, 1, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Not equal
            builder.Equal(0, 1, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);
        }

        [Test]
        public void EqualLong()
        {
            //Equal
            builder.Equal(1L, 1L, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Not equal
            builder.Equal(0L, 1L, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);
        }

        [Test]
        public void EqualIEquatable()
        {
            //Equal
            builder.Equal((IEquatable<int>)1, (IEquatable<int>)1, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Not equal
            builder.Equal((IEquatable<int>)0, (IEquatable<int>)1, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);
        }

        [Test]
        public void NotEqualFloat()
        {
            //Not equal
            builder.NotEqual(0f, 1f, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Equal
            builder.NotEqual(1f, 1f, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);
        }

        [Test]
        public void NotEqualDouble()
        {
            //Not equal
            builder.NotEqual(0.0, 1.0, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Equal
            builder.NotEqual(1.0, 1.0, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);
        }

        [Test]
        public void NotEqualInt()
        {
            //Not equal
            builder.NotEqual(0, 1, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Equal
            builder.NotEqual(1, 1, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);
        }

        [Test]
        public void NotEqualLong()
        {
            //Not equal
            builder.NotEqual(0L, 1L, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Equal
            builder.NotEqual(1L, 1L, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);
        }

        [Test]
        public void NotEqualIEquatable()
        {
            //Not equal
            builder.NotEqual((IEquatable<int>)0, (IEquatable<int>)1, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //Equal
            builder.NotEqual((IEquatable<int>)1, (IEquatable<int>)1, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);
        }

        [Test]
        public void IsTrue()
        {
            //True
            builder.IsTrue(true, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //False
            builder.IsTrue(false, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);
        }

        [Test]
        public void IsFalse()
        {
            //False
            builder.IsFalse(false, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);

            //True
            builder.IsFalse(true, 0.5f);

            Assert.AreEqual(0.5f, builder.Ranking);
        }

        [Test]
        public void Fail()
        {
            Assert.AreEqual(false, builder.Failed);

            //Fail
            builder.Fail();

            Assert.AreEqual(true, builder.Failed);

            //Add ranking as should always return 0
            builder.IsTrue(true, 0.5f);

            Assert.AreEqual(0.0f, builder.Ranking);
        }

        [Test]
        public void Clear()
        {
            //Fail and give ranking
            builder.Fail();

            //Clear
            builder.Clear();

            Assert.AreEqual(0.0f, builder.Ranking);
            Assert.AreEqual(false, builder.Failed);
        }
    }
}
