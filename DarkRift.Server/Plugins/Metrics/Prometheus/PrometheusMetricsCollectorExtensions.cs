/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Server.Metrics;
using System;
using System.Text.RegularExpressions;

namespace DarkRift.Server.Plugins.Metrics.Prometheus
{
#if PRO
    /// <summary>
    /// Provides extensions to the <see cref="MetricsCollector"/> that enrich it for use with the Prometheus endpoint.
    /// </summary>
    public static class PrometheusMetricsCollectorExtensions
    {
        /// <summary>
        /// Returns whether the backing <see cref="MetricsWriter"/> of this collector is a Prometheus endpoint.
        /// </summary>
        /// <param name="metricsCollector">The metrics collector being extended.</param>
        /// <returns>true, if the writer is a Prometheus endpoint; else, false.</returns>
        public static bool IsWritingToPrometheus(this MetricsCollector metricsCollector)
        {
            return metricsCollector.Writer is PrometheusEndpoint;
        }

        /// <summary>
        /// Creates a new histogram metric.
        /// </summary>
        /// <param name="metricsCollector">The metrics collector being extended.</param>
        /// <param name="name">The name of the metric.</param>
        /// <param name="description">The description of the metric.</param>
        /// <param name="buckets">The upper bounds of the buckets to aggregate into in ascending order, a bucket with value <see cref="double.PositiveInfinity"/> will be automattically added if omitted.</param>
        /// <returns>The created histogram.</returns>
        public static IHistogramMetric Histogram(this MetricsCollector metricsCollector, string name, string description, double[] buckets)
        {
            if (!metricsCollector.IsWritingToPrometheus())
                throw new InvalidOperationException("Prometheus metrics extensions cannot be used without a non-prometheus metric writer. Consider using core metric types, exensions for your chosen writer or switch to the Promethus metrics writer.");


            // Ensure buckets always get larger
            double lastBucket = double.NegativeInfinity;
            foreach (double bucket in buckets)
            {
                if (lastBucket >= bucket)
                    throw new ArgumentException("Buckets must be in ascending order and not start at double.NegativeInfinity.");
                lastBucket = bucket;
            }

            // Ensure the last bucket upper bound is always infinity
            if (buckets.Length == 0 || buckets[buckets.Length - 1] != double.PositiveInfinity)
            {
                Array.Resize(ref buckets, buckets.Length + 1);
                buckets[buckets.Length - 1] = double.PositiveInfinity;
            }

            return ((PrometheusEndpoint)metricsCollector.Writer).CreateHistogram(metricsCollector, name, description);
        }

        /// <summary>
        /// Creates a new histogram metric with tags.
        /// </summary>
        /// <param name="metricsCollector">The metrics collector being extended.</param>
        /// <param name="name">The name of the metric.</param>
        /// <param name="description">The description of the metric.</param>
        /// <param name="buckets">The upper bounds of the buckets to aggregate into in ascending order, a bucket with value <see cref="double.PositiveInfinity"/> will be automattically added if omitted.</param>
        /// <param name="tags">The tags to attach to the metric in the form "key:value".</param>
        /// <returns>The created histogram.</returns>
        public static TaggedMetricBuilder<IHistogramMetric> Histogram(this MetricsCollector metricsCollector, string name, string description, double[] buckets, params string[] tags)
        {
            if (!metricsCollector.IsWritingToPrometheus())
                throw new InvalidOperationException("Prometheus metrics extensions cannot be used without a non-prometheus metric writer. Consider using core metric types, exensions for your chosen writer or switch to the Promethus metrics writer.");

            ValidateTags(tags);

            // Ensure buckets always get larger
            double lastBucket = double.NegativeInfinity;
            foreach (double bucket in buckets)
            {
                if (lastBucket >= bucket)
                    throw new ArgumentException("Buckets must be in ascending order and not start at double.NegativeInfinity.");
                lastBucket = bucket;
            }

            // Ensure the last bucket upper bound is always infinity
            if (buckets.Length == 0 || buckets[buckets.Length - 1] != double.PositiveInfinity)
            {
                Array.Resize(ref buckets, buckets.Length + 1);
                buckets[buckets.Length - 1] = double.PositiveInfinity;
            }

            return ((PrometheusEndpoint)metricsCollector.Writer).CreateHistogram(metricsCollector, name, description, tags);
        }

        // TODO support summary metrics

        /// <summary>
        /// Clone of <see cref="MetricsCollector.ValidateTags"/>.
        /// </summary>
        /// <param name="tags">The tags to validate.</param>
        private static void ValidateTags(string[] tags)
        {
            if (tags == null)
                throw new ArgumentNullException("Tags must not be null, use an empty array instead.");

            foreach (string tag in tags)
            {
                if (!Regex.IsMatch(tag, @"\w+:\w+"))
                    throw new ArgumentException($@"Tag '{tag}' must match regex '^[a-zA-Z_][a-zA-Z0-9_]*'. For example: 'tag_name_here'.");
            }
        }
    }
#endif
}
