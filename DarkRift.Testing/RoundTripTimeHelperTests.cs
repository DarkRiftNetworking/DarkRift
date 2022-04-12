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
using System.Threading;
using System.Threading.Tasks;

namespace DarkRift.Testing
{
    [TestClass]
    public class RoundTripTimeHelperTests
    {
        private RoundTripTimeHelper roundTripTime;

        public RoundTripTimeHelperTests()
        {
            roundTripTime = new RoundTripTimeHelper(2, 3);
        }

        [TestMethod]
        public void RecordTest()
        {
            roundTripTime.RecordOutboundPing(89);
            roundTripTime.RecordOutboundPing(90);

            Thread.Sleep(10);
            
            roundTripTime.RecordInboundPing(90);

            Assert.IsTrue(roundTripTime.LatestRtt > 0.01);

            Thread.Sleep(10);

            roundTripTime.RecordInboundPing(89);

            Assert.IsTrue(roundTripTime.LatestRtt > 0.02);
            Assert.IsTrue(roundTripTime.SmoothedRtt > 0.01);

            Assert.ThrowsException<KeyNotFoundException>(() => roundTripTime.RecordInboundPing(91));
        }
    }
}
