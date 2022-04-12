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

#if PRO
namespace DarkRift.Server.Configuration.Testing
{
    [TestClass]
    public class DarkRiftClusterConfigurationBuilderTests
    {
        [TestMethod]
        public void TestAddGroup()
        {
            // GIVEN an empty config builder
            DarkRiftClusterConfigurationBuilder builder = DarkRiftClusterConfigurationBuilder.Create();

            // WHEN a group is added
            builder.AddGroup("name", ServerVisibility.Internal, "group 1", "group 2");

            // THEN the group is added to the spawn data
            Assert.AreEqual(1, builder.ClusterSpawnData.Groups.Groups.Count);
            Assert.AreEqual("name", builder.ClusterSpawnData.Groups.Groups[0].Name);
            Assert.AreEqual(ServerVisibility.Internal, builder.ClusterSpawnData.Groups.Groups[0].Visibility);
            Assert.AreEqual(2, builder.ClusterSpawnData.Groups.Groups[0].ConnectsTo.Count);
            Assert.AreEqual("group 1", builder.ClusterSpawnData.Groups.Groups[0].ConnectsTo[0].Name);
            Assert.AreEqual("group 2", builder.ClusterSpawnData.Groups.Groups[0].ConnectsTo[1].Name);
        }
    }
}
#endif
