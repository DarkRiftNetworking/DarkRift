﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace DarkRift.Server.Metrics
{
    /// <summary>
    /// Implementation of <see cref="IGaugeMetric"/> that does nothing.
    /// </summary>
    internal class NoOpGaugeMetric : IGaugeMetric
    {
        public void Report(double value)
        {
            // Nope
        }
    }
}
