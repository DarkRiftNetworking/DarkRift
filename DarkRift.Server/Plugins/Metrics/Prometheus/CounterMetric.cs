/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Server.Metrics;
using System.Threading;

namespace DarkRift.Server.Plugins.Metrics.Prometheus
{
#if PRO
    /// <summary>
    /// Implementation of <see cref="ICounterMetric"/> for Prometheus.
    /// </summary>
    internal class CounterMetric : ICounterMetric
    {
        /// <inheritDoc/>
        internal string Name { get; }

        /// <inheritDoc/>
        internal string Description { get; }

        /// <summary>
        /// The current counter value.
        /// </summary>
        internal double Value => InterlockedDouble.Read(ref value);

        /// <summary>
        /// The Prometheus preformatted metric string.
        /// </summary>
        internal string Preformatted { get; }

        /// <summary>
        ///  The current counter value.
        /// </summary>
        private double value;

        public CounterMetric(string name, string description, string preformatted)
        {
            this.Name = name;
            this.Description = description;
            this.Preformatted = preformatted;
        }

        /// <inheritDoc/>
        public void Increment()
        {
            Increment(1);
        }

        /// <inheritDoc/>
        public void Increment(double value)
        {
            InterlockedDouble.Add(ref this.value, value);
        }
    }
#endif
}
