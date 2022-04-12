/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace DarkRift.Server
{
    /// <summary>
    ///     Server representation of a client.
    /// </summary>
    public interface IClient : IMessageSinkSource
    {
#if PRO
        /// <summary>
        ///     Called when the client is given a strike for illegal behaviour.
        /// </summary>
        /// <remarks><c>Pro only.</c> </remarks>
        event EventHandler<StrikeEventArgs> StrikeOccured;
#endif

        /// <summary>
        ///     The ID of the client.
        /// </summary>
        ushort ID { get; }

        /// <summary>
        ///     The remote end point we are connected to on TCP.
        /// </summary>
        [Obsolete("Use GetRemoteEndPoint(\"TCP\") instead.")]
        IPEndPoint RemoteTcpEndPoint { get; }

        /// <summary>
        ///     The remote end point we are connected to UDP.
        /// </summary>
        [Obsolete("Use GetRemoteEndPoint(\"UDP\") instead.")]
        IPEndPoint RemoteUdpEndPoint { get; }

        /// <summary>
        ///     Is this client still available?
        /// </summary>
        [Obsolete("Use IClient.ConnectionState instead.")]
        bool IsConnected { get; }

        /// <summary>
        ///     The state of the connection;
        /// </summary>
        ConnectionState ConnectionState { get; }

        /// <summary>
        ///     The number of illegal behaviours this client has made.
        /// </summary>
        /// <remarks>
        ///     <legacyBold>Setter only available in Pro.</legacyBold>
        /// </remarks>
        byte Strikes
        {
            get;
#if PRO
            set;
#endif
        }

#if PRO
        /// <summary>
        ///     The time this client connected to the server.
        /// </summary>
        /// <remarks>
        ///     <c>Pro only.</c> 
        /// </remarks>
        DateTime ConnectionTime { get; }
#endif

#if PRO
        /// <summary>
        ///     The number of messages sent from the server.
        /// </summary>
        /// <remarks>
        ///     <c>Pro only.</c> 
        /// </remarks>
        uint MessagesSent { get; }
#endif

#if PRO
        /// <summary>
        ///     The number of messages pushed from the server.
        /// </summary>
        /// <remarks>
        ///     <c>Pro only.</c> 
        /// </remarks>
        uint MessagesPushed { get; }
#endif

#if PRO
        /// <summary>
        ///     The number of messages received at the server.
        /// </summary>
        /// <remarks>
        ///     <c>Pro only.</c> 
        /// </remarks>
        uint MessagesReceived { get; }
#endif

        /// <summary>
        ///     The collection of end points this client is connected to.
        /// </summary>
        IEnumerable<IPEndPoint> RemoteEndPoints { get; }
        
        /// <summary>
        ///     The round trip time helper for this client.
        /// </summary>
        RoundTripTimeHelper RoundTripTime { get; }

        /// <summary>
        ///     Disconnects this client from the server.
        /// </summary>
        /// <returns>Whether the disconnect was successful.</returns>
        bool Disconnect();

        /// <summary>
        ///     Gets the remote end point with the given name.
        /// </summary>
        /// <param name="name">The end point name.</param>
        /// <returns>The end point.</returns>
        IPEndPoint GetRemoteEndPoint(string name);

        #region Strikes

#if PRO
        /// <summary>
        ///     Strikes this client.
        /// </summary>
        /// <param name="message">A message describing the reason for the strike.</param>
        /// <remarks>
        ///     <c>Pro only.</c> 
        /// </remarks>
        void Strike(string message = null);

        /// <summary>
        ///     Strikes this client.
        /// </summary>
        /// <param name="message">A message describing the reason for the strike.</param>
        /// <param name="weight">The number of strikes this accounts for.</param>
        /// <remarks>
        ///     <c>Pro only.</c> 
        /// </remarks>
        void Strike(string message = null, int weight = 1);
#endif

        #endregion
    }
}
