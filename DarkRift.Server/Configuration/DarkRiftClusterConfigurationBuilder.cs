/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;

#if PRO

namespace DarkRift.Server.Configuration
{
    /// <summary>
    /// Builder for DarkRift cluster configurations. Helps build a <see cref="ClusterSpawnData"/> object.
    /// </summary>
    public class DarkRiftClusterConfigurationBuilder
    {
        /// <summary>
        /// The <see cref="ClusterSpawnData"/> being constructed.
        /// </summary>
        public ClusterSpawnData ClusterSpawnData { get; }

        // TODO add to docs

        // TODO test

        private DarkRiftClusterConfigurationBuilder(ClusterSpawnData clusterSpawnData)
        {
            ClusterSpawnData = clusterSpawnData;
        }

        /// <summary>
        /// Creates a blank builder to begin configuration.
        /// </summary>
        /// <returns>The created builder.</returns>
        public static DarkRiftClusterConfigurationBuilder Create()
        {
            return new DarkRiftClusterConfigurationBuilder(ClusterSpawnData.CreateDefault());
        }

        /// <summary>
        /// Creates a builder from the given XML cluster configuration to begin configuration.
        /// </summary>
        /// <param name="path">The path to the XML config.</param>
        /// <returns>The created builder.</returns>
        public static DarkRiftClusterConfigurationBuilder CreateFromXml(string path)
        {
            return CreateFromXml(path, new NameValueCollection());
        }

        /// <summary>
        /// Creates a builder from the given XML cluster configuration to begin configuration.
        /// </summary>
        /// <param name="path">The path to the XML config.</param>
        /// <param name="variables">The variable to substitute into the configuration.</param>
        /// <returns>The created builder.</returns>
        public static DarkRiftClusterConfigurationBuilder CreateFromXml(string path, NameValueCollection variables)
        {
            return new DarkRiftClusterConfigurationBuilder(ClusterSpawnData.CreateFromXml(path, variables));
        }

        /// <summary>
        /// Configures a group in the cluster.
        /// </summary>
        /// <param name="name">The name of the group.</param>
        /// <param name="visibility">The visibility of the group.</param>
        /// <param name="connectsTo">The name of groups this group connects to.</param>
        /// <returns>The configuration builder to continue construction.</returns>
        public DarkRiftClusterConfigurationBuilder AddGroup(string name, ServerVisibility visibility, params string[] connectsTo)
        {
            ClusterSpawnData.GroupsSettings.GroupSettings networkListenerSettings = new ClusterSpawnData.GroupsSettings.GroupSettings
            {
                Name = name,
                Visibility = visibility
            };

            foreach (string connectsToName in connectsTo)
            {
                ClusterSpawnData.GroupsSettings.GroupSettings.ConnectsToSettings connectsToSettings = new ClusterSpawnData.GroupsSettings.GroupSettings.ConnectsToSettings()
                {
                    Name = connectsToName
                };
                networkListenerSettings.ConnectsTo.Add(connectsToSettings);
            }

            ClusterSpawnData.Groups.Groups.Add(networkListenerSettings);

            return this;
        }
    }
}

#endif
