/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;

namespace DarkRift.Server
{
    /// <summary>
    ///     Class containing info about the DarkRift server running.
    /// </summary>
    public class DarkRiftInfo
    {
        /// <summary>
        ///     The time the server was started.
        /// </summary>
        // TODO expose UTC version of this
        public DateTime StartTime { get; }

        /// <summary>
        ///     The version of the server.
        /// </summary>
        public Version Version => typeof(DarkRiftInfo).Assembly.GetName().Version;

        /// <summary>
        /// The root URL of the server's documentation.
        /// </summary>
        public string DocumentationRoot => $"https://www.darkriftnetworking.com/DarkRift2/Docs/{Version.ToString(3)}/";

        /// <summary>
        ///     The type of server running.
        /// </summary>
#if PRO
        public ServerType Type => ServerType.Pro;
#else
        public ServerType Type => ServerType.Free;
#endif

        /// <summary>
        ///     The type of server.
        /// </summary>
        public enum ServerType
        {
            /// <summary>
            ///     Indicates the server is the free version.
            /// </summary>
            Free,

            /// <summary>
            ///     Indicates the server is paid for. Yay!
            /// </summary>
            Pro
        }

        /// <summary>
        ///     Creates a new DarkRiftInfo object.
        /// </summary>
        /// <param name="startTime">The time the server was started</param>
        public DarkRiftInfo(DateTime startTime)
        {
            this.StartTime = startTime;
        }
    }
}
