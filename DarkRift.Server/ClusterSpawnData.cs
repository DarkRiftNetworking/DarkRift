/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml.Linq;

namespace DarkRift.Server
{
#if PRO
    /// <summary>
    ///     Details of the architecture of the cluster.
    /// </summary>
    // TODO DR3 rename to ClusterConfiguration
    [Serializable]
    public class ClusterSpawnData
    {
        /// <summary>
        ///     Holds the groups in the cluster.
        /// </summary>
        public GroupsSettings Groups { get; set; } = new GroupsSettings();

        /// <summary>
        ///     Details the groups in the cluster.
        /// </summary>
        [Serializable]
        public class GroupsSettings
        {
            /// <summary>
            ///     The groups in the cluster.
            /// </summary>
            public List<GroupSettings> Groups { get; } = new List<GroupSettings>();

            /// <summary>
            ///     Details of a group in the cluster
            /// </summary>
            [Serializable]
            public class GroupSettings
            {
                /// <summary>
                ///     The name of the group.
                /// </summary>
                public string Name { get; set; }

                /// <summary>
                ///     Whether the server is external facing or internal facing.
                /// </summary>
                public ServerVisibility Visibility { get; set; }

                /// <summary>
                ///     The groups this group connects to.
                /// </summary>
                public List<ConnectsToSettings> ConnectsTo { get; } = new List<ConnectsToSettings>();

                /// <summary>
                ///     Holds details about server links.
                /// </summary>
                [Serializable]
                public class ConnectsToSettings
                {
                    /// <summary>
                    ///     The name of the group to connect to.
                    /// </summary>
                    public string Name { get; set; }

                    /// <summary>
                    ///     Loads the groups settings from the specified XML element.
                    /// </summary>
                    /// <param name="element">The XML element to load from.</param>
                    /// <param name="helper">The XML configuration helper being used.</param>
                    internal void LoadFromXmlElement(XElement element, ConfigurationFileHelper helper)
                    {
                        Name = helper.ReadStringAttribute(
                            element,
                            "name"
                        );
                    }
                }

                /// <summary>
                ///     Constructor for a new group settings instance with null values.
                /// </summary>
                public GroupSettings()
                {

                }

                /// <summary>
                ///     Constructor for a new group settings instance.
                /// </summary>
                /// <param name="name">The group name.</param>
                /// <param name="visibility">The group's visibility.</param>
                public GroupSettings(string name, ServerVisibility visibility)
                {
                    this.Name = name;
                    this.Visibility = visibility;
                }

                /// <summary>
                ///     Loads the groups settings from the specified XML element.
                /// </summary>
                /// <param name="element">The XML element to load from.</param>
                /// <param name="helper">The XML configuration helper being used.</param>
                internal void LoadFromXmlElement(XElement element, ConfigurationFileHelper helper)
                {
                    Name = helper.ReadStringAttribute(
                        element,
                        "name"
                    );

                    Visibility = helper.ReadServerVisibilityAttribute(
                        element,
                        "visibility"
                    );

                    helper.ReadElementCollectionTo(
                        element,
                        "connectsTo",
                        e => {
                            ConnectsToSettings s = new ConnectsToSettings();
                            s.LoadFromXmlElement(e, helper);
                            return s;
                        },
                        ConnectsTo
                    );
                }
            }

            /// <summary>
            ///     Loads the groups settings from the specified XML element.
            /// </summary>
            /// <param name="element">The XML element to load from.</param>
            /// <param name="helper">The XML configuration helper being used.</param>
            internal void LoadFromXmlElement(XElement element, ConfigurationFileHelper helper)
            {
                helper.ReadElementCollectionTo(
                    element,
                    "group",
                    e => {
                        GroupSettings s = new GroupSettings();
                        s.LoadFromXmlElement(e, helper);
                        return s;
                    },
                    Groups
                );
            }
        }

        /// <summary>
        ///     Creates a cluster spawn data from specified XML configuration file.
        /// </summary>
        /// <param name="filePath">The path of the XML file.</param>
        /// <param name="variables">The variables to inject into the configuration.</param>
        /// <returns>The ClusterSpawnData created.</returns>
        public static ClusterSpawnData CreateFromXml(string filePath, NameValueCollection variables)
        {
            return CreateFromXml(XDocument.Load(filePath, LoadOptions.SetLineInfo), variables);
        }

        /// <summary>
        ///     Creates a cluster spawn data from specified XML configuration file.
        /// </summary>
        /// <param name="document">The XML file.</param>
        /// <param name="variables">The variables to inject into the configuration.</param>
        /// <returns>The ClusterSpawnData created.</returns>
        public static ClusterSpawnData CreateFromXml(XDocument document, NameValueCollection variables)
        {
            //Create a new cluster spawn data.
            ClusterSpawnData spawnData = new ClusterSpawnData();

            ConfigurationFileHelper helper = new ConfigurationFileHelper(variables, $"{new DarkRiftInfo(DateTime.Now).DocumentationRoot}configuration/cluster/", $"{new DarkRiftInfo(DateTime.Now).DocumentationRoot}advanced/configuration_variables.html");

            XElement root = document.Root;

            spawnData.Groups.LoadFromXmlElement(root.Element("groups"), helper);

            //Return the new spawn data!
            return spawnData;
        }

        /// <summary>
        ///     Creates a new cluster spawn data with necessary settings.
        /// </summary>
        public ClusterSpawnData()
        {
        }

        /// <summary>
        ///     Creates a cluster with a single, default group
        /// </summary>
        /// <returns>A new default cluster.</returns>
        internal static ClusterSpawnData CreateDefault()
        {
            return new ClusterSpawnData();
        }
    }
#endif
}
