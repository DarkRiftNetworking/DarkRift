/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.DataStructures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DarkRift
{
    /// <summary>
    ///     Helper class for calculating the round trip time of messages.
    /// </summary>
    public sealed class RoundTripTimeHelper
    {
        /// <summary>
        ///     Returns the smoothed round trip time to the remote and back in seconds.
        /// </summary>
        [Obsolete("Use SmoothedRtt instead.")]
        public float SmothedRtt => SmoothedRtt;

        /// <summary>
        ///     Returns the smoothed round trip time to the remote and back in seconds.
        /// </summary>
        public float SmoothedRtt => movingAverage.Average;

        /// <summary>
        ///     Returns the latest recorded round trip time to the remote and back in seconds.
        /// </summary>
        public float LatestRtt { get; private set; }
        
        /// <summary>
        ///     The number of samples used to calculate the smoothed round trip time.
        /// </summary>
        public int RttSampleCount { get; }

        /// <summary>
        ///     Moving average filter for round trip time.
        /// </summary>
        private readonly MovingAverageFilter movingAverage;

        /// <summary>
        ///     The pings currently awaiting a response.
        /// </summary>
        private readonly CircularDictionary<ushort, long> waitingPings;

        /// <summary>
        ///     Creates a new RoundTripTimeHelper.
        /// </summary>
        public RoundTripTimeHelper(int rttSampleCount, int pingBacklogSize)
        {
            this.RttSampleCount = rttSampleCount;

            movingAverage = new MovingAverageFilter(rttSampleCount);
            waitingPings = new CircularDictionary<ushort, long>(pingBacklogSize);
        }

        /// <summary>
        ///     Records a ping being sent to this client.
        /// </summary>
        /// <param name="pingCode">The code to identify the ping.</param>
        internal void RecordOutboundPing(ushort pingCode)
        {
            waitingPings.Add(pingCode, Stopwatch.GetTimestamp());
        }

        /// <summary>
        ///     Records a ping being acknowledged by this client.
        /// </summary>
        /// <param name="pingCode">The code to identify the ping.</param>
        internal void RecordInboundPing(ushort pingCode)
        {
            long sendTimestamp = waitingPings[pingCode];

            long receiveTimestamp = Stopwatch.GetTimestamp();

            float rtt = (float)(receiveTimestamp - sendTimestamp) / Stopwatch.Frequency;
            
            movingAverage.Add(rtt);
            LatestRtt = rtt;
        }
    }
}
