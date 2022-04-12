/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Net.Sockets;

namespace DarkRift.Server.Plugins.Listeners.Bichannel
{
    /// <summary>
    ///     Abstract base class for bichannel listeners.
    /// </summary>
    // TODO this would be better as an interface but that would require an INetworkListener to also exist
    public abstract class AbstractBichannelListener : NetworkListener
    {
        /// <summary>
        ///     The UDP port being listened on.
        /// </summary>
        public abstract ushort UdpPort { get; protected set; }

        /// <summary>
        ///     Whether Nagle's algorithm should be disabled.
        /// </summary>
        public abstract bool NoDelay { get; set; }

        /// <summary>
        ///     The maximum size the client can ask a TCP body to be without being striked.
        /// </summary>
        /// <remarks>This defaults to 65KB.</remarks>
        public abstract int MaxTcpBodyLength { get; }

        /// <summary>
        ///     Creates a new bichannel listener with the given load data.
        /// </summary>
        /// <param name="networkListenerLoadData">The load data for thsi listener.</param>
        protected AbstractBichannelListener(NetworkListenerLoadData networkListenerLoadData)
            : base(networkListenerLoadData)
        {

        }
    }
}
