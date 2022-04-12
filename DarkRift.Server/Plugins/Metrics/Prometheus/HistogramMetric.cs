/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Server.Metrics;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;

namespace DarkRift.Server.Plugins.Metrics.Prometheus
{
#if PRO
    /// <summary>
    /// Implementation of <see cref="IHistogramMetric"/> for Prometheus.
    /// </summary>
    internal class HistogramMetric : IHistogramMetric
    {
        /// <inheritDoc/>
        internal string Name { get; }

        /// <inheritDoc/>
        internal string Description { get; }

        /// <summary>
        /// The current sum.
        /// </summary>
        internal double Sum => InterlockedDouble.Read(ref sum);

        /// <summary>
        /// The current count.
        /// </summary>
        internal long Count => Interlocked.Read(ref bucketCounts[bucketCounts.Length - 1]);

        /// <summary>
        /// The preformatted metric texts for buckets.
        /// </summary>
        internal string[] PreformattedBuckets { get; }

        /// <summary>
        /// The preformatted metric text for sum.
        /// </summary>
        internal string PreformattedSum { get; }
        
        /// <summary>
        /// The preformatted metric text for count.
        /// </summary>
        internal string PreformattedCount { get; }

        /// <summary>
        /// The current sum.
        /// </summary>
        private double sum;

        /// <summary>
        /// The current upper bound of each buckets.
        /// </summary>
        private readonly double[] bucketUpperBounds;

        /// <summary>
        /// The current counts of each buckets.
        /// </summary>
        private readonly long[] bucketCounts;

        public HistogramMetric(string name, string description, double[] buckets, string[] preformattedBuckets, string preformattedSum, string preformattedCount)
        {
            this.Name = name;
            this.Description = description;
            this.PreformattedBuckets = preformattedBuckets;
            this.PreformattedSum = preformattedSum;
            this.PreformattedCount = preformattedCount;
            this.bucketUpperBounds = buckets;
            this.bucketCounts = new long[buckets.Length];
        }

        /// <inheritDoc/>
        public void Report(double value)
        {
            // Currently we accept there are race conditions here, we could get around it with a lock but frankly it's probably ok
            // until proven otherwise
            InterlockedDouble.Add(ref sum, value);

            for (int i = 0; i < bucketUpperBounds.Length; i++)
            {
                if (value < bucketUpperBounds[i])
                    Interlocked.Increment(ref bucketCounts[i]);
            }
        }

        /// <summary>
        /// Returns the count of values in a bucket.
        /// </summary>
        /// <param name="bucketIndex">The index of the bucket to retrieve the sum for.</param>
        /// <returns>The count of values in that bucket.</returns>
        internal double GetBucketCount(int bucketIndex)
        {
            return Interlocked.Read(ref bucketCounts[bucketIndex]);
        }
    }
#endif
}
