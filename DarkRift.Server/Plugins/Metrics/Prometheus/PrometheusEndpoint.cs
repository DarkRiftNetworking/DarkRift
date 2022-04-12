/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Server.Metrics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace DarkRift.Server.Plugins.Metrics.Prometheus
{
#if PRO
    /// <summary>
    /// Implements a HTTP endpoint exposing metrics for Prometheus.
    /// </summary>
    internal class PrometheusEndpoint : MetricsWriter
    {
        /// <summary>
        /// The default buckets to use in a histogram metric.
        /// </summary>
        /// <remarks>
        /// Last bucket must always be +Inf.
        /// </remarks>
        private static readonly double[] DEFAULT_BUCKETS = new double[] { 0.005, 0.01, 0.02, 0.04, 0.08, 0.16, 0.5, 1, 2, 5, 10, double.PositiveInfinity };

        public override Version Version => new Version(1, 0, 0);

        internal override bool Hidden => true;

        /// <summary>
        /// Static empty array to reduce GC.
        /// </summary>
        private static readonly byte[] emptyArray = new byte[0];

        /// <summary>
        /// The HTTP listener in use.
        /// </summary>
        private readonly HttpListener httpListener = new HttpListener();

        /// <summary>
        /// The HTTP host we are listening on.
        /// </summary>
        private readonly string host;

        /// <summary>
        /// The HTTP port we are listening on.
        /// </summary>
        private readonly ushort port;

        /// <summary>
        /// The HTTP path we are listening on.
        /// </summary>
        private readonly string path;

        /// <summary>
        /// The background thread listening for health check requests.
        /// </summary>
        private readonly Thread listenThread;

        /// <summary>
        /// If the serevr is still running or not.
        /// </summary>
        private volatile bool running = true;

        private readonly List<CounterMetric> counters = new List<CounterMetric>();
        private readonly List<GaugeMetric> gauges = new List<GaugeMetric>();
        private readonly List<HistogramMetric> histograms = new List<HistogramMetric>();
        

        public PrometheusEndpoint(MetricsWriterLoadData metricsWriterLoadData) : base(metricsWriterLoadData)
        {
            host = metricsWriterLoadData.Settings["host"] ?? "localhost";

            port = 9796;
            if(metricsWriterLoadData.Settings["port"] != null)
            {
                if (!ushort.TryParse(metricsWriterLoadData.Settings["port"], out port))
                    Logger.Error($"Prometheus port not an valid value. Using a value of {port} instead.");
            }

            path = metricsWriterLoadData.Settings["path"] ?? "/metrics";

            httpListener.Prefixes.Add($"http://{host}:{port}/");

            // TODO support adding labels from config

            httpListener.Start();

            listenThread = new Thread(Listen);
            listenThread.Start();

            Logger.Trace($"Prometheus endpoint started at 'http://{host}:{port}{path}'");
        }

        /// <inheritdoc />
        protected internal override ICounterMetric CreateCounter(MetricsCollector metricsCollector, string name, string description)
        {
            string formattedMetric = FormatMetric(metricsCollector.Prefix, name);
            return GetOrCreateCounter(name, description, formattedMetric);
        }

        /// <inheritdoc />
        protected internal override TaggedMetricBuilder<ICounterMetric> CreateCounter(MetricsCollector metricsCollector, string name, string description, string[] tags)
        {
            return new TaggedMetricBuilder<ICounterMetric>(tags.Length, tagValues =>
            {
                string formattedMetric = FormatMetric(metricsCollector.Prefix, name, tags, tagValues);
                return GetOrCreateCounter(name, description, formattedMetric);
            });
        }

        /// <inheritdoc />
        protected internal override IGaugeMetric CreateGauge(MetricsCollector metricsCollector, string name, string description)
        {
            string formattedMetric = FormatMetric(metricsCollector.Prefix, name);
            return GetOrCreateGauge(name, description, formattedMetric);
        }

        /// <inheritdoc />
        protected internal override TaggedMetricBuilder<IGaugeMetric> CreateGauge(MetricsCollector metricsCollector, string name, string description, string[] tags)
        {
            return new TaggedMetricBuilder<IGaugeMetric>(tags.Length, tagValues =>
            {
                string formattedMetric = FormatMetric(metricsCollector.Prefix, name, tags, tagValues);
                return GetOrCreateGauge(name, description, formattedMetric);
            });
        }

        /// <inheritdoc />
        protected internal override IHistogramMetric CreateHistogram(MetricsCollector metricsCollector, string name, string description)
        {
            return CreateHistogram(metricsCollector, name, description, DEFAULT_BUCKETS);
        }

        /// <inheritdoc />
        protected internal override TaggedMetricBuilder<IHistogramMetric> CreateHistogram(MetricsCollector metricsCollector, string name, string description, string[] tags)
        {
            return CreateHistogram(metricsCollector, name, description, DEFAULT_BUCKETS, tags);
        }

        /// <summary>
        /// Creates an <see cref="IHistogramMetric"/> that writes to this writer.
        /// </summary>
        /// <param name="metricsCollector">The <see cref="MetricsCollector"/> used to create the metric.</param>
        /// <param name="name">The name of the metric.</param>
        /// <param name="description">The description of the metric.</param>
        /// <param name="buckets">The upper bounds of the buckets to aggregate into in ascending order, a bucket with value <see cref="double.PositiveInfinity"/> is always present.</param>
        /// <returns>The created <see cref="IHistogramMetric"/>.</returns>
        internal IHistogramMetric CreateHistogram(MetricsCollector metricsCollector, string name, string description, double[] buckets)
        {
            string formattedSumMetric = FormatMetric(metricsCollector.Prefix, name + "_sum");
            string formattedCountMetric = FormatMetric(metricsCollector.Prefix, name + "_count");
            string[] formattedBuckets = buckets.Select(b => FormatMetric(metricsCollector.Prefix, name + "_bucket", b)).ToArray();
            return GetOrCreateHistogram(name, description, buckets, formattedSumMetric, formattedCountMetric, formattedBuckets);
        }

        /// <summary>
        /// Creates a tagged metric builder for an <see cref="IHistogramMetric"/> that writes to this writer.
        /// </summary>
        /// <param name="metricsCollector">The <see cref="MetricsCollector"/> used to create the metric.</param>
        /// <param name="name">The name of the metric.</param>
        /// <param name="description">The description of the metric.</param>
        /// <param name="buckets">The upper bounds of the buckets to aggregate into in ascending order, a bucket with value <see cref="double.PositiveInfinity"/> is always present.</param>
        /// <param name="tags">The set of tags describing this metric as colon separated pairs.</param>
        /// <returns>The created <see cref="IHistogramMetric"/>.</returns>
        internal TaggedMetricBuilder<IHistogramMetric> CreateHistogram(MetricsCollector metricsCollector, string name, string description, double[] buckets, string[] tags)
        {
            return new TaggedMetricBuilder<IHistogramMetric>(tags.Length, tagValues =>
            {
                string formattedSumMetric = FormatMetric(metricsCollector.Prefix, name + "_sum", tags, tagValues);
                string formattedCountMetric = FormatMetric(metricsCollector.Prefix, name + "_count", tags, tagValues);
                string[] formattedBuckets = buckets.Select(b => FormatMetric(metricsCollector.Prefix, name + "_bucket", tags, tagValues, b)).ToArray();
                return GetOrCreateHistogram(name, description, buckets, formattedSumMetric, formattedCountMetric, formattedBuckets);
            });
        }

        private void Listen()
        {
            while (running)
            {
                HttpListenerContext context;
                try
                {
                    context = httpListener.GetContext();
                }
                catch (HttpListenerException e)
                {
                    if (e.ErrorCode != 500)
                        Logger.Warning("Prometheus endpoint has exited prematurely as the HTTP server has reported an error.", e);
                    return;
                }

                if (context.Request.HttpMethod != "GET")
                {
                    context.Response.StatusCode = 405;
                    context.Response.Close(emptyArray, false);
                }
                else if (context.Request.Url.AbsolutePath != path)
                {
                    context.Response.StatusCode = 404;
                    context.Response.Close(emptyArray, false);
                }
                else
                {
                    context.Response.ContentType = "text/plain; version=0.0.4";

                    using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
                    {
                        writer.NewLine = "\n";

                        lock (counters)
                        {
                            // TODO deduplicate descriptions for different tags etc.
                            foreach (CounterMetric metric in counters)
                            {
                                WriteDocs(writer, metric.Name, metric.Description, "counter");
                                WriteMetric(writer, metric.Preformatted, metric.Value);
                                writer.WriteLine();
                            }
                        }

                        lock (gauges)
                        {
                            // TODO deduplicate descriptions for different tags etc.
                            foreach (GaugeMetric metric in gauges)
                            {
                                WriteDocs(writer, metric.Name, metric.Description, "gauge");
                                WriteMetric(writer, metric.Preformatted, metric.Value);
                                writer.WriteLine();
                            }
                        }

                        lock (histograms)
                        {
                            // TODO deduplicate descriptions for different tags etc.
                            foreach (HistogramMetric metric in histograms)
                            {
                                WriteDocs(writer, metric.Name, metric.Description, "histogram");
                                for (int i = 0; i < metric.PreformattedBuckets.Length; i++)
                                    WriteMetric(writer, metric.PreformattedBuckets[i], metric.GetBucketCount(i));

                                WriteMetric(writer, metric.PreformattedSum, metric.Sum);
                                WriteMetric(writer, metric.PreformattedCount, metric.Count);
                                writer.WriteLine();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tries to return a memoised counter metric or else creates a new one.
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="description">The metric description.</param>
        /// <param name="formattedMetric">The preformatted metric string.</param>
        /// <returns>The found or created metric.</returns>
        private ICounterMetric GetOrCreateCounter(string name, string description, string formattedMetric)
        {
            lock (counters)
            {
                // If this counter already exists use that
                foreach (CounterMetric counter in counters)
                {
                    if (counter.Preformatted == formattedMetric)
                        return counter;
                }

                CounterMetric metric = new CounterMetric(name, description, formattedMetric);
                counters.Add(metric);
                return metric;
            }
        }

        /// <summary>
        /// Tries to return a memoised gauge metric or else creates a new one.
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="description">The metric description.</param>
        /// <param name="formattedMetric">The preformatted metric string.</param>
        /// <returns>The found or created metric.</returns>
        private IGaugeMetric GetOrCreateGauge(string name, string description, string formattedMetric)
        {
            lock (gauges)
            {
                // If this gauge already exists use that
                foreach (GaugeMetric gauge in gauges)
                {
                    if (gauge.Preformatted == formattedMetric)
                        return gauge;
                }

                GaugeMetric metric = new GaugeMetric(name, description, formattedMetric);
                gauges.Add(metric);
                return metric;
            }
        }

        /// <summary>
        /// Tries to return a memoised histogram metric or else creates a new one.
        /// </summary>
        /// <param name="name">The metric name.</param>
        /// <param name="description">The metric description.</param>
        /// <param name="buckets">The buckets to create the histogram for.</param>
        /// <param name="formattedSumMetric">The preformatted sum metric string.</param>
        /// <param name="formattedCountMetric">The preformatted count metric string.</param>
        /// <param name="formattedBuckets">The preformatted bucket metric strings.</param>
        /// <returns>The found or created metric.</returns>
        private IHistogramMetric GetOrCreateHistogram(string name, string description, double[] buckets, string formattedSumMetric, string formattedCountMetric, string[] formattedBuckets)
        {
            lock (histograms)
            {
                // If this histogram already exists use that
                foreach (HistogramMetric histogram in histograms)
                {
                    if (histogram.PreformattedSum == formattedSumMetric
                        && histogram.PreformattedCount == formattedCountMetric
                        && Enumerable.SequenceEqual(histogram.PreformattedBuckets, formattedBuckets))
                        return histogram;
                }

                HistogramMetric metric = new HistogramMetric(
                    name,
                    description,
                    buckets,
                    formattedBuckets,
                    formattedSumMetric,
                    formattedCountMetric
                );

                histograms.Add(metric);
                return metric;
            }
        }

        /// <summary>
        /// Writes a comment to the promethues endpoint stream.
        /// </summary>
        /// <param name="writer">The wrtier to write to.</param>
        /// <param name="metricName">The metric the docs are for.</param>
        /// <param name="help">The help test to write.</param>
        /// <param name="type">The type of the metric.</param>
        private static void WriteDocs(StreamWriter writer, string metricName, string help, string type)
        {
            writer.Write("# HELP ");
            writer.Write(metricName);
            writer.Write(' ');
            writer.WriteLine(help);
            writer.Write("# TYPE ");
            writer.Write(metricName);
            writer.Write(' ');
            writer.WriteLine(type);
        }

        /// <summary>
        /// Writes a metric to the promethues endpoint stream.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="metricName">The name of the metric.</param>
        /// <param name="value">The value of the metric.</param>
        private static void WriteMetric(StreamWriter writer, string metricName, double value)
        {
            writer.Write(metricName);
            writer.Write(' ');
            writer.WriteLine(value.ToString(CultureInfo.InvariantCulture));     // Use invarient culture otherwise decimal points will be decimal commas in some places!
        }

        /// <summary>
        /// Calculates the currently unix timestamp in millis since epoch.
        /// </summary>
        /// <returns>The Unix timstamp.</returns>
        internal static long GetTimestamp()
        {
            // TODO when not supporting net35 just use DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            DateTimeOffset epoch = new DateTimeOffset(1970, 01, 01, 00, 00, 00, TimeSpan.Zero);
            DateTimeOffset now = DateTimeOffset.UtcNow;

            return (long) now.Subtract(epoch).TotalMilliseconds;
        }

        /// <summary>
        /// Formats a metric string for exporting.
        /// </summary>
        /// <param name="prefix">The metric prefix to apply to the name.</param>
        /// <param name="name">The metric name </param>
        /// <returns>The Prometheius format metric text.</returns>
        private static string FormatMetric(string prefix, string name)
        {
            return "darkrift_" + FormatPrefix(prefix) + "_" + name;
        }

        /// <summary>
        /// Formats a metric string for exporting.
        /// </summary>
        /// <param name="prefix">The metric prefix to apply to the name.</param>
        /// <param name="name">The metric name </param>
        /// <param name="tags">The tags to add to the metric.</param>
        /// <param name="tagValues">The values of the tags to add to the metric.</param>
        /// <returns>The Prometheius format metric text.</returns>
        private static string FormatMetric(string prefix, string name, string[] tags, string[] tagValues)
        {
            return "darkrift_" + FormatPrefix(prefix) + "_" + name + FormatTagString(tags, tagValues);
        }

        /// <summary>
        /// Formats a metric string for exporting.
        /// </summary>
        /// <param name="prefix">The metric prefix to apply to the name.</param>
        /// <param name="name">The metric name </param>
        /// <param name="bucket">The histogram bucket upper bound.</param>
        /// <returns>The Prometheius format metric text.</returns>
        private static string FormatMetric(string prefix, string name, double bucket)
        {
            return "darkrift_" + FormatPrefix(prefix) + "_" + name + FormatTagString(bucket);
        }

        /// <summary>
        /// Formats a metric string for exporting.
        /// </summary>
        /// <param name="prefix">The metric prefix to apply to the name.</param>
        /// <param name="name">The metric name </param>
        /// <param name="tags">The tags to add to the metric.</param>
        /// <param name="tagValues">The values of the tags to add to the metric.</param>
        /// <param name="bucket">The histogram bucket upper bound.</param>
        /// <returns>The Prometheius format metric text.</returns>
        private static string FormatMetric(string prefix, string name, string[] tags, string[] tagValues, double bucket)
        {
            return "darkrift_" + FormatPrefix(prefix) + "_" + name + FormatTagString(tags, tagValues, bucket);
        }

        /// <summary>
        /// Formats a prefix in PascalCase/camelCase to snake_case.
        /// </summary>
        /// <param name="prefix">The prefix to format.</param>
        /// <returns>The prefix in snake case.</returns>
        private static string FormatPrefix(string prefix)
        {
            return prefix.Select((c, i) =>
            {
                // Don't add underscores to the first character
                if (i == 0)
                    return c.ToString();

                // Don't add underscores if the previous character was upper
                if (char.IsUpper(prefix[i - 1]))
                    return c.ToString();

                if (char.IsUpper(c))
                    return '_' + c.ToString();

                return c.ToString();
            }).Aggregate((a, b) => a + b).ToLower();
        }

        /// <summary>
        /// Converts tags to Prometheus format.
        /// </summary>
        /// <param name="tags">The tags to format.</param>
        /// <param name="tagValues">The values of the tags to add to the metric.</param>
        /// <returns>The prometheus style tag string.</returns>
        private static string FormatTagString(string[] tags, string[] tagValues)
        {
            if (tags == null || tags.Length == 0)
                return "";

            return "{" + tags.Select((key, i) => key + "=\"" + tagValues[i] + "\"").Aggregate((str, next) => str + "," + next) + "}";
        }

        /// <summary>
        /// Converts tags to Prometheus format.
        /// </summary>
        /// <param name="tags">The tags to format.</param>
        /// <param name="tagValues">The values of the tags to add to the metric.</param>
        /// <param name="bucket">The histogram bucket upper bound.</param>
        /// <returns>The prometheus style tag string.</returns>
        private static string FormatTagString(string[] tags, string[] tagValues, double bucket)
        {
            string bucketText = bucket == double.PositiveInfinity ? "+Inf" : bucket.ToString();
            return "{" + tags.Select((key, i) => key + "=\"" + tagValues[i] + "\"").Select(t => t + ",").Aggregate("", (str, next) => str + next) + "le=\"" + bucketText + "\"}";
        }

        /// <summary>
        /// Converts tags to Prometheus format.
        /// </summary>
        /// <param name="bucket">The histogram bucket upper bound.</param>
        /// <returns>The prometheus style tag string.</returns>
        private static string FormatTagString(double bucket)
        {
            string bucketText = bucket == double.PositiveInfinity ? "+Inf" : bucket.ToString();
            return "{le=\"" + bucketText + "\"}";
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            running = false;
            httpListener.Close();
        }
    }
#endif
}
