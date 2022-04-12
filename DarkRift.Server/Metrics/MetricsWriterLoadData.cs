/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections.Specialized;

namespace DarkRift.Server.Metrics
{
#if PRO
    /// <summary>
    ///     Load data for <see cref="MetricsWriter"/> plugins.
    /// </summary>
    /// <remarks>
    ///     Pro only.
    /// </remarks>
    public class MetricsWriterLoadData : PluginBaseLoadData
    {
        /// <summary>
        ///     Creates new load data for a <see cref="MetricsWriter"/>.
        /// </summary>
        /// <param name="name">The name of the metrics writer.</param>
        /// <param name="settings">The settings to pass the metrics writer.</param>
        /// <param name="serverInfo">The runtime details about the server.</param>
        /// <param name="threadHelper">The server's thread helper.</param>
        public MetricsWriterLoadData(string name, NameValueCollection settings, DarkRiftInfo serverInfo, DarkRiftThreadHelper threadHelper)
            : base(name, settings, serverInfo, threadHelper)
        {
        }

        internal MetricsWriterLoadData(string name, DarkRiftServer server, NameValueCollection settings, Logger logger)
            : base(name, server, settings, logger)
        {
        }
    }
#endif
}
