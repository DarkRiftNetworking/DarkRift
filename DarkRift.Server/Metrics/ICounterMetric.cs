/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace DarkRift.Server.Metrics
{
#if PRO
    /// <summary>
    /// A metric capable only of increasing in value.
    /// </summary>
    /// <remarks>
    ///     Pro only.
    /// </remarks>
    public interface ICounterMetric
    {
        /// <summary>
        /// Increase the value of the counter by 1.
        /// </summary>
        void Increment();

        /// <summary>
        /// Increase the value of the counter by a specified amount.
        /// </summary>
        /// <param name="value">The amount to increase the counter by.</param>
        void Increment(double value);
    }
#endif
}
