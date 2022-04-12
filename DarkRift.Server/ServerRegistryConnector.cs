/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;

namespace DarkRift.Server
{
#if PRO
    /// <summary>
    ///     A plugin that provides the server with information about the current architecture.
    /// </summary>
    public abstract class ServerRegistryConnector : ExtendedPluginBase
    {
        /// <summary>
        ///     The manager for all server registry connectors on this server.
        /// </summary>
        public IServerRegistryConnectorManager ServerRegistryConnectorManager { get; }

        /// <summary>
        ///     The server manager for remote servers.
        /// </summary>
        public IRemoteServerManager RemoteServerManager { get; }

        /// <summary>
        ///     The server we belong to.
        /// </summary>
        private readonly DarkRiftServer server;

        /// <summary>
        ///     Creates a new server registry connector.
        /// </summary>
        /// <param name="serverRegistryConnectorLoadData">The data to load the connector with.</param>
        public ServerRegistryConnector(ServerRegistryConnectorLoadData serverRegistryConnectorLoadData) : base(serverRegistryConnectorLoadData)
        {
            server = serverRegistryConnectorLoadData.Server;
            ServerRegistryConnectorManager = serverRegistryConnectorLoadData.ServerRegistryConnectorManager;
            RemoteServerManager = serverRegistryConnectorLoadData.RemoteServerManager;
        }

        /// <summary>
        ///     Registers the server with the cluster registry.
        /// </summary>
        /// <remarks>
        ///     This will be called when the server starts up and indicates that the server should be added to the registry to be discovered by
        ///     other servers.
        /// </remarks>
        /// <param name="group">The group this server belongs to.</param>
        /// <param name="host">The advertised host of this server.</param>
        /// <param name="port">The advertised port of this server.</param>
        /// <param name="properties">Any additional properties of this server.</param>
        /// <returns>The ID of the server in the registry.</returns>
        protected internal abstract ushort RegisterServer(string group, string host, ushort port, IDictionary<string, string> properties);

        /// <summary>
        ///     Deregisters the server from the cluster registry.
        /// </summary>
        /// <remarks>
        ///     This will be called when the server closes down and indicates that the server should be removed from the registry. This may not
        ///     always be called depending on how the server closed and so health checks should be used or an implementation of
        ///     <see cref="HandleConnectionFailure(ushort)"/> supplied as a failsafe.
        /// </remarks>
        protected internal abstract void DeregisterServer();

        /// <summary>
        ///     Called when DarkRift is unable to connect to a server that the registry supplied.
        /// </summary>
        /// <remarks>
        ///     This can be used as an alternative to health checks to search for entries in the registry that have become unresponsive.
        ///
        ///     This method will be called whenever a server is unable to be connected to after the configured number of retries. It will be
        ///     called regardless of if this is the first time connecting following a call to
        ///     <see cref="HandleServerJoin(ushort, string, string, ushort, IDictionary{string, string})"/> or if it is a reconnection attempt
        ///     following a lost connection.
        ///
        ///     By default this method does nothing.
        /// </remarks>
        /// <param name="id">The ID of the server unable to connect to.</param>
        protected internal virtual void HandleConnectionFailure(ushort id)
        {

        }

        /// <summary>
        ///     Instructs the server that a new server has joined the cluster.
        /// </summary>
        /// <remarks>
        ///     Upon calling this method DarkRift will decide whether is it necessary to track this server and whether or not a connection
        ///     needs to be made to it based on the contents of the system configuration file. Therefore it is ok to call this method for
        ///     all servers regardless of whether the server's group is relevant to this server or not.
        ///
        ///     Similarly this method can be called on a server when that server is the server joining (i.e. notifying it of itself) and
        ///     aditional checks do not need to be put in place in the connector as that call will be discarded automatically.
        /// </remarks>
        /// <param name="id">The ID of the server.</param>
        /// <param name="group">The group the server is part of.</param>
        /// <param name="host">The host of the server.</param>
        /// <param name="port">The port of the server.</param>
        /// <param name="properties">The additional properties to connect with.</param>
        protected void HandleServerJoin(ushort id, string group, string host, ushort port, IDictionary<string, string> properties)
        {
            server.InternalRemoteServerManager.HandleServerJoin(id, group, host, port, properties);
        }

        /// <summary>
        ///     Instructs the server that a server has left the cluster.
        /// </summary>
        /// <remarks>
        ///     Upon calling this method DarkRift will check to see if it is actually connected to the server before performing any action.
        ///     Therefore it is ok to call this method for all servers regardless of whether the server's group is relevant to this server or
        ///     not.
        /// </remarks>
        /// <param name="id">The ID of the server.</param>
        protected void HandleServerLeave(ushort id)
        {
            server.InternalRemoteServerManager.HandleServerLeave(id);
        }
    }
#endif
}
