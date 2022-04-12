/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace DarkRift.Server.Metrics
{
#if PRO
    /// <summary>
    /// An object that manages the server's metrics writers.
    /// </summary>
    /// <remarks>
    /// Pro only
    /// </remarks>
    public interface IMetricsManager
    {
        /// <summary>
        /// The server's metrics writer.
        /// </summary>
        MetricsWriter MetricsWriter { get; }

        /// <summary>
        /// Returns a metrics collector for the given component.
        /// </summary>
        /// <param name="name">The name of the component to create metrics for.</param>
        /// <returns>The created <see cref="MetricsCollector"/></returns>
        MetricsCollector GetMetricsCollectorFor(string name);

        /// <summary>
        /// Returns a no-op metrics collector for the given component that will not record metrics.
        /// </summary>
        /// <param name="name">The name of the component to create metrics for.</param>
        /// <returns>The created <see cref="MetricsCollector"/></returns>
        MetricsCollector GetNoOpMetricsCollectorFor(string name);

        /// <summary>
        /// Returns a metrics collector for a component that will log metrics every message.
        /// </summary>
        /// <param name="name">The name of the component to create metrics for.</param>
        /// <returns>The created <see cref="MetricsCollector"/></returns>
        /// <remarks>
        /// The metrics collector returned will be a no-op metrics collector unless per message metrics are
        /// enabled in the server's metrics settings.
        /// </remarks>
        MetricsCollector GetPerMessageMetricsCollectorFor(string name);
    }
#endif
}
