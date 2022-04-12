/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DarkRift.Server.Plugins.Metrics.Prometheus
{
#if PRO
    /// <summary>
    /// Helpful interlocked methods for doubles.
    /// </summary>
    internal static class InterlockedDouble
    {
        /// <summary>
        /// Read a double atomically.
        /// </summary>
        /// <param name="location">The reference to the double to read.</param>
        /// <returns>The value of the double.</returns>
        public static double Read(ref double location)
        {
            // TODO DR3 Use Volitile.Read
            return Interlocked.CompareExchange(ref location, 0, 0);
        }

        /// <summary>
        /// Add a value to a double atomically.
        /// </summary>
        /// <param name="location">The location of the double to add to.</param>
        /// <param name="value">The value to add to the double.</param>
        /// <returns>The new value of the double.</returns>
        public static double Add(ref double location, double value)
        {
            double newCurrentValue = location; // non-volatile read, so may be stale
            while (true)
            {
                double currentValue = newCurrentValue;
                double newValue = currentValue + value;
                newCurrentValue = Interlocked.CompareExchange(ref location, newValue, currentValue);
                if (newCurrentValue == currentValue)
                    return newValue;
            }
        }
    }
#endif
}
