/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace DarkRift.Server.Metrics
{
#if PRO
    /// <summary>
    /// A metric representing the current value of a property, able to increase or decrease.
    /// </summary>
    /// <remarks>
    ///     Pro only.
    /// </remarks>
    public interface IGaugeMetric
    {
        /// <summary>
        /// Set the value of this gauge to that specified.
        /// </summary>
        /// <param name="value">The value of the gauge to set.</param>
        void Report(double value);
    }
#endif
}
