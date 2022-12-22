/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Specialized;
using System.Linq;
using NUnit.Framework;

namespace DarkRift.Server.Plugins.Matchmaking.Tests
{
    public class MatchmakerTests
    {
        public class TestEntity
        {
            public float Mmr { get; }

            public TestEntity(float mmr)
            {
                if (mmr < 0 || mmr > 1)
                {
                    throw new ArgumentException("Invalid MMR.");
                }

                Mmr = mmr;
            }
        }

        public class MockMatchRankingContext : MatchRankingContext<TestEntity>
        {
            internal MockMatchRankingContext(float discardThreshold) : base(discardThreshold)
            {
            }
        }

        public class MockRankingMatchmaker : RankingMatchmaker<TestEntity>
        {
            public MockRankingMatchmaker(PluginLoadData pluginLoadData) : base(pluginLoadData)
            {
            }

            public override bool ThreadSafe => throw new NotImplementedException();
            public override Version Version => throw new NotImplementedException();

            public override float GetSuitabilityMetric(TestEntity entity1, TestEntity entity2, MatchRankingContext<TestEntity> context)
            {
                return 1 - (Math.Abs(entity1.Mmr - entity2.Mmr) / 1);
            }
        }

        private RankingMatchmaker<TestEntity> matchmaker;

        [SetUp]
        public void SetUp()
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

            matchmaker = new MockRankingMatchmaker(loadData);
        }

        [Test]
        public void MatchmakerAllPerfect()
        {
            TestEntity[] entities = { new TestEntity(1f), new TestEntity(1f), new TestEntity(1f), new TestEntity(1f) };

            foreach (TestEntity entity in entities)
            {
                matchmaker.Enqueue(entity, null);
            }

            bool invoked = false;
            matchmaker.GroupFormed += (object sender, GroupFormedEventArgs<TestEntity> args) =>
            {
                invoked = true;

                Assert.AreEqual(4, args.Group.Count());
                Assert.AreEqual(4, args.SubGroups.Count());

                Assert.IsTrue(args.Group.Contains(entities[0]));
                Assert.IsTrue(args.Group.Contains(entities[1]));
                Assert.IsTrue(args.Group.Contains(entities[2]));
                Assert.IsTrue(args.Group.Contains(entities[3]));
            };

            matchmaker.PerformFullSearch();

            Assert.IsTrue(invoked);
        }

        [Test]
        public void MatchmakerMostlyPerfect()
        {
            TestEntity[] entities = { new TestEntity(1f), new TestEntity(0.25f), new TestEntity(1f), new TestEntity(1f), new TestEntity(1f) };

            foreach (TestEntity entity in entities)
            {
                matchmaker.Enqueue(entity, null);
            }

            bool invoked = false;
            matchmaker.GroupFormed += (object sender, GroupFormedEventArgs<TestEntity> args) =>
            {
                invoked = true;

                Assert.AreEqual(4, args.Group.Count());
                Assert.AreEqual(4, args.SubGroups.Count());

                Assert.IsTrue(args.Group.Contains(entities[0]));
                Assert.IsTrue(args.Group.Contains(entities[2]));
                Assert.IsTrue(args.Group.Contains(entities[3]));
                Assert.IsTrue(args.Group.Contains(entities[4]));
            };

            matchmaker.PerformFullSearch();

            Assert.IsTrue(invoked);
        }

        [Test]
        public void TestMatchmakerTooFew()
        {
            TestEntity[] entities = { new TestEntity(1f), new TestEntity(1f), new TestEntity(1f) };

            foreach (TestEntity entity in entities)
            {
                matchmaker.Enqueue(entity, null);
            }

            bool invoked = false;
            matchmaker.GroupFormed += (object sender, GroupFormedEventArgs<TestEntity> args) =>
            {
                invoked = true;
            };

            matchmaker.PerformFullSearch();

            Assert.IsFalse(invoked);
        }

        [Test]
        public void TestMatchmakerMetrics()
        {
            TestEntity[] entities = { new TestEntity(1f), new TestEntity(0f), new TestEntity(0.5f), new TestEntity(0.25f), new TestEntity(0.75f), new TestEntity(0.6f), new TestEntity(0.55f), new TestEntity(0.45f) };

            foreach (TestEntity entity in entities)
            {
                matchmaker.Enqueue(entity, null);
            }

            bool invoked = false;
            matchmaker.GroupFormed += (object sender, GroupFormedEventArgs<TestEntity> args) =>
            {
                invoked = true;

                Assert.AreEqual(4, args.Group.Count());
                Assert.AreEqual(4, args.SubGroups.Count());

                Assert.IsTrue(args.Group.Contains(entities[2]));
                Assert.IsTrue(args.Group.Contains(entities[4]));
                Assert.IsTrue(args.Group.Contains(entities[5]));
                Assert.IsTrue(args.Group.Contains(entities[7]));
            };

            matchmaker.PerformFullSearch();

            Assert.IsTrue(invoked);
        }

        [Test]
        public void TestMatchmakerGroups()
        {
            TestEntity[][] entities = {
                new TestEntity[] { new TestEntity(1f), new TestEntity(1f) },
                new TestEntity[] { new TestEntity(1f), new TestEntity(1f) }
            };

            foreach (TestEntity[] group in entities)
            {
                matchmaker.EnqueueGroup(group, null);
            }

            bool invoked = false;
            matchmaker.GroupFormed += (object sender, GroupFormedEventArgs<TestEntity> args) =>
            {
                invoked = true;

                Assert.AreEqual(4, args.Group.Count());
                Assert.AreEqual(2, args.SubGroups.Count());

                Assert.IsTrue(args.Group.Contains(entities[0][0]));
                Assert.IsTrue(args.Group.Contains(entities[0][1]));
                Assert.IsTrue(args.Group.Contains(entities[1][0]));
                Assert.IsTrue(args.Group.Contains(entities[1][1]));
            };

            matchmaker.PerformFullSearch();

            Assert.IsTrue(invoked);
        }

        public void TestMatchmakerGroupsTooMany()
        {
            TestEntity[][] entities = {
                new TestEntity[] { new TestEntity(1f), new TestEntity(1f) },
                new TestEntity[] {new TestEntity(1f), new TestEntity(1f), new TestEntity(1f) }
            };

            foreach (TestEntity[] group in entities)
            {
                matchmaker.EnqueueGroup(group, null);
            }

            bool invoked = false;
            matchmaker.GroupFormed += (object sender, GroupFormedEventArgs<TestEntity> args) =>
            {
                invoked = true;
            };

            matchmaker.PerformFullSearch();

            Assert.IsFalse(invoked);
        }

        [Test]
        public void TestMatchmakerMixed()
        {
            TestEntity[] group = new TestEntity[] { new TestEntity(1f), new TestEntity(1f), new TestEntity(1f) };
            TestEntity individual = new TestEntity(1f);

            matchmaker.EnqueueGroup(group, null);
            matchmaker.Enqueue(individual, null);

            bool invoked = false;
            matchmaker.GroupFormed += (object sender, GroupFormedEventArgs<TestEntity> args) =>
            {
                invoked = true;

                Assert.AreEqual(4, args.Group.Count());
                Assert.AreEqual(2, args.SubGroups.Count());

                Assert.IsTrue(args.Group.Contains(individual));
                Assert.IsTrue(args.Group.Contains(group[0]));
                Assert.IsTrue(args.Group.Contains(group[1]));
                Assert.IsTrue(args.Group.Contains(group[2]));
            };

            matchmaker.PerformFullSearch();

            Assert.IsTrue(invoked);
        }
    }
}
