/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace DarkRift.Server
{
#if PRO
    /// <summary>
    ///     Manages the connections to other server groups.
    /// </summary>
    public interface IRemoteServerManager
    {
        /// <summary>
        ///     The ID of the server in the cluster.
        /// </summary>
        ushort ServerID { get; }

        /// <summary>
        ///     The group the server is in.
        /// </summary>
        string Group { get; }

        /// <summary>
        ///     The visibility of the server.
        /// </summary>
        ServerVisibility Visibility { get; }

        /// <summary>
        ///     Returns a server group by name.
        /// </summary>
        /// <param name="name">The name of the server group.</param>
        /// <returns>The server group.</returns>
        IServerGroup this[string name] { get; }

        /// <summary>
        ///     Returns all server groups.
        /// </summary>
        /// <returns>An array of all server groups.</returns>
        IServerGroup[] GetAllGroups();

        /// <summary>
        ///     Returns a server group by name.
        /// </summary>
        /// <param name="name">The name of the server group.</param>
        /// <returns>The server group.</returns>
        IServerGroup GetGroup(string name);

        /// <summary>
        ///     Find the server with the given ID, if connected to it.
        /// </summary>
        /// <param name="id">The ID to find</param>
        /// <returns>The server found.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">If this server is not connected to the specified server.</exception>
        IRemoteServer FindServer(ushort id);
    }
#endif
}
