/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace DarkRift.Server.Metrics
{
#if PRO
    /// <summary>
    /// A metric measuring a statistical disribution of values.
    /// </summary>
    /// <remarks>
    ///     Pro only.
    /// </remarks>
    public interface IHistogramMetric
    {
        /// <summary>
        /// Adds the value to the histogram.
        /// </summary>
        /// <param name="value">The value to add.</param>
        void Report(double value);
    }
#endif
}
