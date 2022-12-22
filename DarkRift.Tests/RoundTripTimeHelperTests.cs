/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace DarkRift.Tests
{
    public class RoundTripTimeHelperTests
    {
        private readonly RoundTripTimeHelper roundTripTime;

        public RoundTripTimeHelperTests()
        {
            roundTripTime = new RoundTripTimeHelper(2, 3);
        }

        [Test]
        public void Record()
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

            Assert.Throws<KeyNotFoundException>(() => roundTripTime.RecordInboundPing(91));
        }
    }
}
