/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DarkRift.Server.Metrics
{
#if PRO
    /// <summary>
    /// Provides an interface for reporting basic metrics.
    /// </summary>
    /// <remarks>
    ///     Pro only.
    /// </remarks>
    public class MetricsCollector
    {
        /// <summary>
        /// The prefix to add to metrics created from this <see cref="MetricsCollector"/>.
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// The <see cref="MetricsWriter"/> this collector is sending metrics to.
        /// </summary>
        public MetricsWriter Writer { get; }

        internal MetricsCollector(string prefix, MetricsWriter writer)
        {
            this.Prefix = prefix;
            this.Writer = writer;
        }

        /// <summary>
        /// Creates a new counter metric.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="description">The description of the metric.</param>
        /// <returns>The created counter.</returns>
        public ICounterMetric Counter(string name, string description)
        {
            ValidateName(name);
            if (Writer != null)
                return Writer.CreateCounter(this, name, description);
            else
                return new NoOpCounterMetric();
        }

        /// <summary>
        /// Creates a new counter metric with tags.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="description">The description of the metric.</param>
        /// <param name="tags">The tag keys the metric will use.</param>
        /// <returns>The created counter.</returns>
        public TaggedMetricBuilder<ICounterMetric> Counter(string name, string description, params string[] tags)
        {
            ValidateName(name);
            ValidateTags(tags);
            if (Writer != null)
                return Writer.CreateCounter(this, name, description, tags);
            else
                return new TaggedMetricBuilder<ICounterMetric>(tags.Length, tagValues => new NoOpCounterMetric());
        }

        /// <summary>
        /// Creates a new gauge metric.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="description">The description of the metric.</param>
        /// <returns>The created gauge.</returns>
        public IGaugeMetric Gauge(string name, string description)
        {
            ValidateName(name);
            if (Writer != null)
                return Writer.CreateGauge(this, name, description);
            else
                return new NoOpGaugeMetric();
        }

        /// <summary>
        /// Creates a new gauge metric with tags.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="description">The description of the metric.</param>
        /// <param name="tags">The tag keys the metric will use.</param>
        /// <returns>The created gauge.</returns>
        public TaggedMetricBuilder<IGaugeMetric> Gauge(string name, string description, params string[] tags)
        {
            ValidateName(name);
            ValidateTags(tags);
            if (Writer != null)
                return Writer.CreateGauge(this, name, description, tags);
            else
                return new TaggedMetricBuilder<IGaugeMetric>(tags.Length, tagValues => new NoOpGaugeMetric());
        }

        /// <summary>
        /// Creates a new histogram metric.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="description">The description of the metric.</param>
        /// <returns>The created histogram.</returns>
        public IHistogramMetric Histogram(string name, string description)
        {
            ValidateName(name);
            if (Writer != null)
                return Writer.CreateHistogram(this, name, description);
            else
                return new NoOpHistogramMetric();
        }

        /// <summary>
        /// Creates a new histogram metric.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="description">The description of the metric.</param>
        /// <param name="tags">The tag keys the metric will use.</param>
        /// <returns>The created histogram.</returns>
        public TaggedMetricBuilder<IHistogramMetric> Histogram(string name, string description, params string[] tags)
        {
            ValidateName(name);
            ValidateTags(tags);
            if (Writer != null)
                return Writer.CreateHistogram(this, name, description, tags);
            else
                return new TaggedMetricBuilder<IHistogramMetric>(tags.Length, tagValues => new NoOpHistogramMetric());
        }

        /// <summary>
        /// Validate the given name.
        /// </summary>
        /// <param name="name">The name to validate.</param>
        private static void ValidateName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name", "Name must not be null. Metric names cannot be null and must be in snake case format: 'metric_name_here'.");

            if (!Regex.IsMatch(name, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
                throw new ArgumentException($@"Name '{name}' must be in snake case and match regex '[a-zA-Z_][a-zA-Z0-9_]*'. For example: 'metric_name_here'. ");
        }

        /// <summary>
        /// Validate the given tags.
        /// </summary>
        /// <param name="tags">The tags to validate.</param>
        private static void ValidateTags(string[] tags)
        {
            if (tags == null)
                throw new ArgumentNullException("tags", "Tags must not be null, use an empty array instead.");

            foreach (string tag in tags)
            {
                if (!Regex.IsMatch(tag, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
                    throw new ArgumentException($@"Tag '{tag}' must match regex '^[a-zA-Z_][a-zA-Z0-9_]*'. For example: 'tag_name_here'.");
            }
        }
    }
#endif
}
