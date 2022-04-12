/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace DarkRift.Server.Metrics
{
#if PRO
    /// <summary>
    /// Implementation of <see cref="ICounterMetric"/> that does nothing.
    /// </summary>
    /// <remarks>
    ///     Pro only.
    /// </remarks>
    internal class NoOpCounterMetric : ICounterMetric
    {
        public void Increment()
        {
            // Nope
        }

        public void Increment(double value)
        {
            // Nope
        }
    }
#endif
}
