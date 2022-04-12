/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

#if PRO
using System;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DarkRift.Server.Plugins.Matchmaking.Tests
{
    [TestClass]
    public class MatchmakerTests
    {
        private Mock<RankingMatchmaker<TestEntity>> matchmaker;

        [TestInitialize]
        public void Initialize()
        {
            NameValueCollection settings = new NameValueCollection()
            {
                { "discardThreshold", "0.5" },
                { "groupDiscardThreshold", "0.6" },
                { "entitiesPerGroup", "4" },
                { "tickPeriod", int.MaxValue.ToString() }       //Stick to manual matching
            };

            DarkRiftThreadHelper threadHelper = new DarkRiftThreadHelper(false, null);

            PluginLoadData loadData = new PluginLoadData("Matchmaker", settings, null, threadHelper, (Logger)null, null);

            matchmaker = new Mock<RankingMatchmaker<TestEntity>>(loadData)
            {
                CallBase = true
            };

            //Setup suitability metric function to be a simple minimization of MMRs
            matchmaker.Setup(m => m.GetSuitabilityMetric(It.IsAny<TestEntity>(), It.IsAny<TestEntity>(), It.IsAny<MatchRankingContext<TestEntity>>()))
                .Returns((TestEntity e1, TestEntity e2, MatchRankingContext<TestEntity> context) => 1 - Math.Abs(e1.Mmr - e2.Mmr) / 1);
        }

        [TestMethod]
        public void TestMatchmakerAllPerfect()
        {
            TestEntity[] entities = { new TestEntity(1f), new TestEntity(1f), new TestEntity(1f), new TestEntity(1f) };

            foreach (TestEntity entity in entities)
                matchmaker.Object.Enqueue(entity, null);

            bool invoked = false;
            matchmaker.Object.GroupFormed += (object sender, GroupFormedEventArgs<TestEntity> args) =>
            {
                invoked = true;

                Assert.AreEqual(4, args.Group.Count());
                Assert.AreEqual(4, args.SubGroups.Count());

                Assert.IsTrue(args.Group.Contains(entities[0]));
                Assert.IsTrue(args.Group.Contains(entities[1]));
                Assert.IsTrue(args.Group.Contains(entities[2]));
                Assert.IsTrue(args.Group.Contains(entities[3]));
            };

            matchmaker.Object.PerformFullSearch();

            Assert.IsTrue(invoked);
        }

        [TestMethod]
        public void TestMatchmakerMostlyPerfect()
        {
            TestEntity[] entities = { new TestEntity(1f), new TestEntity(0.25f), new TestEntity(1f), new TestEntity(1f), new TestEntity(1f) };

            foreach (TestEntity entity in entities)
                matchmaker.Object.Enqueue(entity, null);
            
            bool invoked = false;
            matchmaker.Object.GroupFormed += (object sender, GroupFormedEventArgs<TestEntity> args) =>
            {
                invoked = true;

                Assert.AreEqual(4, args.Group.Count());
                Assert.AreEqual(4, args.SubGroups.Count());

                Assert.IsTrue(args.Group.Contains(entities[0]));
                Assert.IsTrue(args.Group.Contains(entities[2]));
                Assert.IsTrue(args.Group.Contains(entities[3]));
                Assert.IsTrue(args.Group.Contains(entities[4]));
            };

            matchmaker.Object.PerformFullSearch();

            Assert.IsTrue(invoked);
        }

        [TestMethod]
        public void TestMatchmakerTooFew()
        {
            TestEntity[] entities = { new TestEntity(1f), new TestEntity(1f), new TestEntity(1f) };

            foreach (TestEntity entity in entities)
                matchmaker.Object.Enqueue(entity, null);

            bool invoked = false;
            matchmaker.Object.GroupFormed += (object sender, GroupFormedEventArgs<TestEntity> args) =>
            {
                invoked = true;
            };

            matchmaker.Object.PerformFullSearch();

            Assert.IsFalse(invoked);
        }

        [TestMethod]
        public void TestMatchmakerMetrics()
        {
            TestEntity[] entities = { new TestEntity(1f), new TestEntity(0f), new TestEntity(0.5f), new TestEntity(0.25f), new TestEntity(0.75f), new TestEntity(0.6f), new TestEntity(0.55f), new TestEntity(0.45f) };

            foreach (TestEntity entity in entities)
                matchmaker.Object.Enqueue(entity, null);

            bool invoked = false;
            matchmaker.Object.GroupFormed += (object sender, GroupFormedEventArgs<TestEntity> args) =>
            {
                invoked = true;

                Assert.AreEqual(4, args.Group.Count());
                Assert.AreEqual(4, args.SubGroups.Count());

                Assert.IsTrue(args.Group.Contains(entities[2]));
                Assert.IsTrue(args.Group.Contains(entities[4]));
                Assert.IsTrue(args.Group.Contains(entities[5]));
                Assert.IsTrue(args.Group.Contains(entities[7]));
            };

            matchmaker.Object.PerformFullSearch();

            Assert.IsTrue(invoked);
        }

        [TestMethod]
        public void TestMatchmakerGroups()
        {
            TestEntity[][] entities = {
                new TestEntity[] { new TestEntity(1f), new TestEntity(1f) },
                new TestEntity[] { new TestEntity(1f), new TestEntity(1f) }
            };

            foreach (TestEntity[] group in entities)
                matchmaker.Object.EnqueueGroup(group, null);

            bool invoked = false;
            matchmaker.Object.GroupFormed += (object sender, GroupFormedEventArgs<TestEntity> args) =>
            {
                invoked = true;

                Assert.AreEqual(4, args.Group.Count());
                Assert.AreEqual(2, args.SubGroups.Count());

                Assert.IsTrue(args.Group.Contains(entities[0][0]));
                Assert.IsTrue(args.Group.Contains(entities[0][1]));
                Assert.IsTrue(args.Group.Contains(entities[1][0]));
                Assert.IsTrue(args.Group.Contains(entities[1][1]));
            };

            matchmaker.Object.PerformFullSearch();

            Assert.IsTrue(invoked);
        }

        public void TestMatchmakerGroupsTooMany()
        {
            TestEntity[][] entities = {
                new TestEntity[] { new TestEntity(1f), new TestEntity(1f) },
                new TestEntity[] {new TestEntity(1f), new TestEntity(1f), new TestEntity(1f) }
            };

            foreach (TestEntity[] group in entities)
                matchmaker.Object.EnqueueGroup(group, null);

            bool invoked = false;
            matchmaker.Object.GroupFormed += (object sender, GroupFormedEventArgs<TestEntity> args) =>
            {
                invoked = true;
            };

            matchmaker.Object.PerformFullSearch();

            Assert.IsFalse(invoked);
        }

        [TestMethod]
        public void TestMatchmakerMixed()
        {
            TestEntity[] group = new TestEntity[] { new TestEntity(1f), new TestEntity(1f), new TestEntity(1f) };
            TestEntity individual = new TestEntity(1f);

            matchmaker.Object.EnqueueGroup(group, null);
            matchmaker.Object.Enqueue(individual, null);

            bool invoked = false;
            matchmaker.Object.GroupFormed += (object sender, GroupFormedEventArgs<TestEntity> args) =>
            {
                invoked = true;

                Assert.AreEqual(4, args.Group.Count());
                Assert.AreEqual(2, args.SubGroups.Count());

                Assert.IsTrue(args.Group.Contains(individual));
                Assert.IsTrue(args.Group.Contains(group[0]));
                Assert.IsTrue(args.Group.Contains(group[1]));
                Assert.IsTrue(args.Group.Contains(group[2]));
            };

            matchmaker.Object.PerformFullSearch();

            Assert.IsTrue(invoked);
        }

        public class TestEntity
        {
            public float Mmr { get; }

            public TestEntity(float mmr)
            {
                if (mmr < 0 || mmr > 1)
                    throw new ArgumentException("Invalid MMR.");

                this.Mmr = mmr;
            }
        }

        private class TestRankingProvider
        {
            public float GetSuitabilityMetric(TestEntity entity1, TestEntity entity2, MatchRankingContext<TestEntity> context)
            {
                return entity1.Mmr + entity2.Mmr / 2;
            }
        }
    }
}
#endif
