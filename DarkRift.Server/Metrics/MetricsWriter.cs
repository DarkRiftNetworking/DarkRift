/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace DarkRift.Server.Metrics
{
#if PRO
    /// <summary>
    /// Base class for plugins that handle writing out metrics.
    /// </summary>
    /// <remarks>
    ///     Pro only.
    /// </remarks>
    public abstract class MetricsWriter : PluginBase
    {
        /// <summary>
        ///     Creates a new MetricsWriter.
        /// </summary>
        /// <param name="metricsWriterLoadData">The data to start the metrics writer with.</param>
        public MetricsWriter(MetricsWriterLoadData metricsWriterLoadData) : base(metricsWriterLoadData)
        {
        }

        // TODO would be good to have global tags configurable

        /// <summary>
        /// Creates an <see cref="ICounterMetric"/> that writes to this writer.
        /// </summary>
        /// <param name="metricsCollector">The <see cref="MetricsCollector"/> used to create the metric.</param>
        /// <param name="name">The name of the metric.</param>
        /// <param name="description">The description of the metric.</param>
        /// <returns>The created <see cref="ICounterMetric"/>.</returns>
        protected internal abstract ICounterMetric CreateCounter(MetricsCollector metricsCollector, string name, string description);

        /// <summary>
        /// Creates a builder for an <see cref="ICounterMetric"/> that writes to this writer.
        /// </summary>
        /// <param name="metricsCollector">The <see cref="MetricsCollector"/> used to create the metric.</param>
        /// <param name="name">The name of the metric.</param>
        /// <param name="description">The description of the metric.</param>
        /// <param name="tags">The set of tags describing this metric as colon separated pairs.</param>
        /// <returns>The created <see cref="ICounterMetric"/>.</returns>
        protected internal abstract TaggedMetricBuilder<ICounterMetric> CreateCounter(MetricsCollector metricsCollector, string name, string description, string[] tags);

        /// <summary>
        /// Creates an <see cref="IGaugeMetric"/> that writes to this writer.
        /// </summary>
        /// <param name="metricsCollector">The <see cref="MetricsCollector"/> used to create the metric.</param>
        /// <param name="name">The name of the metric.</param>
        /// <param name="description">The description of the metric.</param>
        /// <returns>The created <see cref="IGaugeMetric"/>.</returns>
        protected internal abstract IGaugeMetric CreateGauge(MetricsCollector metricsCollector, string name, string description);

        /// <summary>
        /// Creates a builder for an <see cref="IGaugeMetric"/> that writes to this writer.
        /// </summary>
        /// <param name="metricsCollector">The <see cref="MetricsCollector"/> used to create the metric.</param>
        /// <param name="name">The name of the metric.</param>
        /// <param name="description">The description of the metric.</param>
        /// <param name="tags">The set of tags describing this metric as colon separated pairs.</param>
        /// <returns>The created <see cref="IGaugeMetric"/>.</returns>
        protected internal abstract TaggedMetricBuilder<IGaugeMetric> CreateGauge(MetricsCollector metricsCollector, string name, string description, string[] tags);

        /// <summary>
        /// Creates an <see cref="IHistogramMetric"/> that writes to this writer.
        /// </summary>
        /// <param name="metricsCollector">The <see cref="MetricsCollector"/> used to create the metric.</param>
        /// <param name="name">The name of the metric.</param>
        /// <param name="description">The description of the metric.</param>
        /// <returns>The created <see cref="IHistogramMetric"/>.</returns>
        protected internal abstract IHistogramMetric CreateHistogram(MetricsCollector metricsCollector, string name, string description);

        /// <summary>
        /// Creates a builder for an <see cref="IHistogramMetric"/> that writes to this writer.
        /// </summary>
        /// <param name="metricsCollector">The <see cref="MetricsCollector"/> used to create the metric.</param>
        /// <param name="name">The name of the metric.</param>
        /// <param name="description">The description of the metric.</param>
        /// <param name="tags">The set of tags describing this metric as colon separated pairs.</param>
        /// <returns>The created <see cref="IHistogramMetric"/>.</returns>
        protected internal abstract TaggedMetricBuilder<IHistogramMetric> CreateHistogram(MetricsCollector metricsCollector, string name, string description, string[] tags);
    }
#endif
}
