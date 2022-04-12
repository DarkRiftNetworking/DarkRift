/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Server.Metrics;
using System.Security.Cryptography;
using System.Threading;

namespace DarkRift.Server.Plugins.Metrics.Prometheus
{
#if PRO
    /// <summary>
    /// Implementation of <see cref="IGaugeMetric"/> for Prometheus.
    /// </summary>
    internal class GaugeMetric : IGaugeMetric
    {
        /// <inheritDoc/>
        internal string Name { get; }

        /// <inheritDoc/>
        internal string Description { get; }

        /// <summary>
        /// The current gauge value.
        /// </summary>
        internal double Value => InterlockedDouble.Read(ref value);

        /// <summary>
        ///     The preformatted metric text.
        /// </summary>
        internal string Preformatted { get; }

        /// <summary>
        ///     The current counter value.
        /// </summary>
        private double value;

        public GaugeMetric(string name, string description, string preformatted)
        {
            this.Name = name;
            this.Description = description;
            this.Preformatted = preformatted;
        }

        /// <inheritDoc/>
        public void Report(double value)
        {
            Interlocked.Exchange(ref this.value, value);
        }
    }
#endif
}
