/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections.Specialized;

namespace DarkRift.Server
{
    /// <summary>
    ///     Load data for <see cref="LogWriter"/> plugins.
    /// </summary>
    public class LogWriterLoadData : PluginBaseLoadData
    {
        /// <summary>
        ///     Creates new load data for a <see cref="LogWriter"/>.
        /// </summary>
        /// <param name="name">The name of the log writer.</param>
        /// <param name="settings">The settings to pass the log writer.</param>
        /// <param name="serverInfo">The runtime details about the server.</param>
        /// <param name="threadHelper">The server's thread helper.</param>
        public LogWriterLoadData(string name, NameValueCollection settings, DarkRiftInfo serverInfo, DarkRiftThreadHelper threadHelper)
            : base(name, settings, serverInfo, threadHelper)
        {
        }

        internal LogWriterLoadData(string name, DarkRiftServer server, NameValueCollection settings, Logger logger)
            : base(name, server, settings, logger)
        {
        }
    }
}
