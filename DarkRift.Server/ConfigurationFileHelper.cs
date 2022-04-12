/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace DarkRift.Server
{
    /// <summary>
    ///     Helper class for reading XML configuration files.
    /// </summary>
    internal class ConfigurationFileHelper
    {
        /// <summary>
        ///     The variables to inject into configuration.
        /// </summary>
        internal NameValueCollection Variables { get; }

        /// <summary>
        /// Root of documentation for this configuration file.
        /// </summary>
        private readonly string configurationDocsRoot;

        /// <summary>
        /// Page in documetation for variable resolution.
        /// </summary>
        private readonly string variablesDocsPage;

        /// <summary>
        ///     Creates a new helper with the specified variables.
        /// </summary>
        /// <param name="variables">The variables to interpolate while processing the configuration file.</param>
        /// <param name="configurationDocsRoot">The root of documentation for this configuration file.</param>
        /// <param name="variablesDocsPage">Page in documetation for variable resolution.</param>
        internal ConfigurationFileHelper(NameValueCollection variables, string configurationDocsRoot, string variablesDocsPage)
        {
            this.Variables = variables;
            this.configurationDocsRoot = configurationDocsRoot;
            this.variablesDocsPage = variablesDocsPage;
        }

        /// <summary>
        ///     Reads an IP attribute from the XML element supplied.
        /// </summary>
        /// <param name="element">The element to read from.</param>
        /// <param name="attributeName">The name of the attribute to read.</param>
        /// <returns>The IP address read.</returns>
        internal IPAddress ReadIPAttribute(XElement element, string attributeName)
        {
            var attribute = element.Attribute(attributeName);

            if (attribute == null)
                throw new XmlConfigurationException($"<{element.Name}> elements must contain an attribute with name '{attributeName}'.", $"{configurationDocsRoot}{element.Name}.html", attribute);

            string value = ResolveVariables(attribute.Value, attribute);

            try
            {
                return IPAddress.Parse(value);
            }
            catch (Exception e)
            {
                throw new XmlConfigurationException($"<{element.Name}> attribute '{attributeName}' not a valid IP address.", $"{configurationDocsRoot}{element.Name}.html", attribute, e);
            }
        }

        /// <summary>
        ///     Reads an IP attribute from the XML element supplied.
        /// </summary>
        /// <param name="element">The element to read from.</param>
        /// <param name="attributeName">The name of the attribute to read.</param>
        /// <param name="defaultValue">The default value if none is provided.</param>
        /// <returns>The IP address read.</returns>
        internal IPAddress ReadIPAttributeOrDefault(XElement element, string attributeName, IPAddress defaultValue)
        {
            var attribute = element.Attribute(attributeName);

            if (attribute == null)
                return defaultValue;

            string value = ResolveVariables(attribute.Value, attribute);

            try
            {
                return IPAddress.Parse(value);
            }
            catch (Exception e)
            {
                throw new XmlConfigurationException($"<{element.Name}> attribute '{attributeName}' not a valid IP address.", $"{configurationDocsRoot}{element.Name}.html", attribute, e);
            }
        }

        /// <summary>
        ///     Reads an IP version value from the XML element supplied.
        /// </summary>
        /// <param name="element">The element to read from.</param>
        /// <param name="attributeName">The name of the attribute to read.</param>
        /// <returns>The ip version read.</returns>
        [Obsolete("Use IPAddress.Family instead.")]
        internal IPVersion ReadIPVersionAttribute(XElement element, string attributeName)
        {
            var attribute = element.Attribute(attributeName);

            if (attribute == null)
                throw new XmlConfigurationException($"<{element.Name}> elements must contain an attribute with name '{attributeName}'.", $"{configurationDocsRoot}{element.Name}.html", attribute);

            string value = ResolveVariables(attribute.Value, attribute).ToLower();
            if (value == "ipv4")
                return IPVersion.IPv4;
            else if (value == "ipv6")
                return IPVersion.IPv6;
            else
                throw new XmlConfigurationException($"<{element.Name}> attribute '{attributeName}' not an acceptable IP version. Expected 'ipv4' or'ipv6'.", $"{configurationDocsRoot}{element.Name}.html", attribute);
        }

        /// <summary>
        ///     Reads an IP version value from the XML element supplied.
        /// </summary>
        /// <param name="element">The element to read from.</param>
        /// <param name="attributeName">The name of the attribute to read.</param>
        /// <param name="defaultValue">The default value if none is provided.</param>
        /// <returns>The ip version read.</returns>
        [Obsolete("Use IPAddress.Family instead.")]
        internal IPVersion ReadIPVersionAttributeOrDefault(XElement element, string attributeName, IPVersion defaultValue)
        {
            var attribute = element.Attribute(attributeName);

            if (attribute == null)
                return defaultValue;

            string value = ResolveVariables(attribute.Value, attribute).ToLower();
            if (value == "ipv4")
                return IPVersion.IPv4;
            else if (value == "ipv6")
                return IPVersion.IPv6;
            else
                throw new XmlConfigurationException($"<{element.Name}> attribute '{attributeName}' not an acceptable IP version. Expected 'ipv4' or'ipv6'.", $"{configurationDocsRoot}{element.Name}.html", attribute);
        }

        /// <summary>
        ///     Reads a Boolean value from the XML element supplied.
        /// </summary>
        /// <param name="element">The element to read from.</param>
        /// <param name="attributeName">The name of the attribute to read.</param>
        /// <param name="defaultValue">The default value to return if the attribute isn't present.</param>
        /// <returns>The Boolean read.</returns>
        internal bool ReadBooleanAttribute(XElement element, string attributeName, bool defaultValue)
        {
            var attribute = element.Attribute(attributeName);

            if (attribute == null)
                return defaultValue;

            string value = ResolveVariables(attribute.Value, attribute);

            try
            {
                return XmlConvert.ToBoolean(value);
            }
            catch (FormatException e)
            {
                throw new XmlConfigurationException($"<{element.Name}> attribute '{attributeName}' not a valid boolean. Expected 'true' or 'false'.", $"{configurationDocsRoot}{element.Name}.html", attribute, e);
            }
        }

        /// <summary>
        ///     Reads a byte value from the XML element supplied.
        /// </summary>
        /// <param name="element">The element to read from.</param>
        /// <param name="attributeName">The name of the attribute to read.</param>
        /// <returns>The byte read.</returns>
        internal byte ReadByteAttribute(XElement element, string attributeName)
        {
            var attribute = element.Attribute(attributeName);
            if (attribute == null)
                throw new XmlConfigurationException($"<{element.Name}> elements must contain an attribute with name '{attributeName}'.", $"{configurationDocsRoot}{element.Name}.html", attribute);

            string value = ResolveVariables(attribute.Value, attribute);

            try
            {
                return XmlConvert.ToByte(value);
            }
            catch (Exception e)
            {
                throw new XmlConfigurationException($"<{element.Name}> attribute '{attributeName}' not an acceptable number.", $"{configurationDocsRoot}{element.Name}.html", attribute, e);
            }
        }

        /// <summary>
        ///     Reads a UInt16 value from the XML element supplied.
        /// </summary>
        /// <param name="element">The element to read from.</param>
        /// <param name="attributeName">The name of the attribute to read.</param>
        /// <returns>The UInt16 read.</returns>
        internal ushort ReadUInt16Attribute(XElement element, string attributeName)
        {
            var attribute = element.Attribute(attributeName);
            if (attribute == null)
                throw new XmlConfigurationException($"<{element.Name}> elements must contain an attribute with name '{attributeName}'.", $"{configurationDocsRoot}{element.Name}.html", attribute);

            string value = ResolveVariables(attribute.Value, attribute);

            try
            {
                return XmlConvert.ToUInt16(value);
            }
            catch (Exception e)
            {
                throw new XmlConfigurationException($"<{element.Name}> attribute '{attributeName}' not an acceptable number.", $"{configurationDocsRoot}{element.Name}.html", attribute, e);
            }
        }

        /// <summary>
        ///     Reads a UInt16 value from the XML element supplied.
        /// </summary>
        /// <param name="element">The element to read from.</param>
        /// <param name="attributeName">The name of the attribute to read.</param>
        /// <param name="defaultValue">The default value to return if the attribute isn't present.</param>
        /// <returns>The UInt16 read.</returns>
        internal ushort ReadUInt16AttributeOrDefault(XElement element, string attributeName, ushort defaultValue)
        {
            var attribute = element.Attribute(attributeName);

            if (attribute == null)
                return defaultValue;

            string value = ResolveVariables(attribute.Value, attribute);

            try
            {
                return XmlConvert.ToUInt16(value);
            }
            catch (Exception e)
            {
                throw new XmlConfigurationException($"<{element.Name}> attribute '{attributeName}' not an acceptable number.", $"{configurationDocsRoot}{element.Name}.html", attribute, e);
            }
        }

        /// <summary>
        ///     Reads a UInt32 value from the XML element supplied.
        /// </summary>
        /// <param name="element">The element to read from.</param>
        /// <param name="attributeName">The name of the attribute to read.</param>
        /// <param name="defaultValue">The default value to return if the attribute isn't present.</param>
        /// <returns>The UInt32 read.</returns>
        internal uint ReadUInt32AttributeOrDefault(XElement element, string attributeName, uint defaultValue)
        {
            var attribute = element.Attribute(attributeName);

            if (attribute == null)
                return defaultValue;

            string value = ResolveVariables(attribute.Value, attribute);

            try
            {
                return XmlConvert.ToUInt32(value);
            }
            catch (FormatException e)
            {
                throw new XmlConfigurationException($"<{element.Name}> attribute '{attributeName}' not an acceptable number.", $"{configurationDocsRoot}{element.Name}.html", attribute, e);
            }
        }

        /// <summary>
        ///     Reads a string value from the XML element supplied.
        /// </summary>
        /// <param name="element">The element to read from.</param>
        /// <param name="attributeName">The name of the attribute to read.</param>
        /// <returns>The string read.</returns>
        internal string ReadStringAttribute(XElement element, string attributeName)
        {
            var attribute = element.Attribute(attributeName);

            if (attribute == null)
                throw new XmlConfigurationException($"<{element.Name}> elements must contain an attribute with name '{attributeName}'.", $"{configurationDocsRoot}{element.Name}.html", attribute);

            return ResolveVariables(attribute.Value, attribute);
        }

        /// <summary>
        ///     Reads a string value from the XML element supplied.
        /// </summary>
        /// <param name="element">The element to read from.</param>
        /// <param name="attributeName">The name of the attribute to read.</param>
        /// <param name="defaultValue">The default value to return if the attribute isn't present.</param>
        /// <returns>The string read.</returns>
        internal string ReadStringAttributeOrDefault(XElement element, string attributeName, string defaultValue)
        {
            var attribute = element.Attribute(attributeName);

            if (attribute == null)
                return defaultValue;

            return ResolveVariables(attribute.Value, attribute);
        }

#if PRO
        /// <summary>
        ///     Reads a server visibility value from the XML element supplied.
        /// </summary>
        /// <param name="element">The element to read from.</param>
        /// <param name="attributeName">The name of the attribute to read.</param>
        /// <returns>The server visibility read.</returns>
        internal ServerVisibility ReadServerVisibilityAttribute(XElement element, string attributeName)
        {
            var attribute = element.Attribute(attributeName);

            if (attribute == null)
                throw new XmlConfigurationException($"<{element.Name}> elements must contain an attribute with name '{attributeName}'.", $"{configurationDocsRoot}{element.Name}.html", attribute);

            string value = ResolveVariables(attribute.Value, attribute).ToLower();
            if (value == "external")
                return ServerVisibility.External;
            else if (value == "internal")
                return ServerVisibility.Internal;
            else
                throw new XmlConfigurationException($"<{element.Name}> attribute '{attributeName}' not a valid server visibility. Expected 'external' or 'internal'.", $"{configurationDocsRoot}{element.Name}.html", attribute);
        }
#endif

        /// <summary>
        ///     Reads a set of log levels from the XML element supplied.
        /// </summary>
        /// <param name="element">The element to read from.</param>
        /// <param name="attributeName">The name of the attribute to read.</param>
        /// <returns>The log levels read.</returns>
        internal LogType[] ReadLogLevelsAttribute(XElement element, string attributeName)
        {
            var attribute = element.Attribute(attributeName);

            if (attribute == null)
                throw new XmlConfigurationException($"<{element.Name}> elements must contain an attribute with name '{attributeName}'.", $"{configurationDocsRoot}{element.Name}.html", attribute);

            return ResolveVariables(attribute.Value, attribute)
                        .ToLower()
                        .Split(',')
                        .Select(l =>
                        {
                            switch (l.Trim().ToLower())
                            {
                                case "trace":
                                    return LogType.Trace;
                                case "info":
                                    return LogType.Info;
                                case "warning":
                                    return LogType.Warning;
                                case "error":
                                    return LogType.Error;
                                case "fatal":
                                    return LogType.Fatal;
                                default:
                                    throw new XmlConfigurationException($"<{element.Name}> attribute '{attributeName}' '{l}' is not a valid log level. Expected 'trace', 'info', 'warning', 'error' or 'fatal'.", $"{configurationDocsRoot}{element.Name}.html", attribute);
                            }
                        })
                        .ToArray();
        }

        /// <summary>
        ///     Reads a set of log levels from the XML element supplied or returns the default.
        /// </summary>
        /// <param name="element">The element to read from.</param>
        /// <param name="attributeName">The name of the attribute to read.</param>
        /// <param name="defaultValue">The default value to return if the attribute isn't present.</param>
        /// <returns>The log levels read.</returns>
        internal LogType[] ReadLogLevelsAttributeOrDefault(XElement element, string attributeName, LogType[] defaultValue)
        {
            var attribute = element.Attribute(attributeName);

            if (attribute == null)
                return defaultValue;

            return ResolveVariables(attribute.Value, attribute)
                        .ToLower()
                        .Split(',')
                        .Select(l =>
                        {
                            switch (l.Trim().ToLower())
                            {
                                case "trace":
                                    return LogType.Trace;
                                case "info":
                                    return LogType.Info;
                                case "warning":
                                    return LogType.Warning;
                                case "error":
                                    return LogType.Error;
                                case "fatal":
                                    return LogType.Fatal;
                                default:
                                    throw new XmlConfigurationException($"<{element.Name}> attribute '{attributeName}' '{l}' is not a valid log level. Expected 'trace', 'info', 'warning', 'error' or 'fatal'.", $"{configurationDocsRoot}{element.Name}.html", attribute);
                            }
                        })
                        .ToArray();
        }

        /// <summary>
        ///     Reads a DependencyResolutionStrategy value from the XML element supplied.
        /// </summary>
        /// <param name="element">The element to read from.</param>
        /// <param name="attributeName">The name of the attribute to read.</param>
        /// <param name="defaultValue">The default value to return if the attribute isn't present.</param>
        /// <returns>The string read.</returns>
        internal DependencyResolutionStrategy ReadDependencyResolutionStrategy(XElement element, string attributeName, DependencyResolutionStrategy defaultValue)
        {
            var attribute = element.Attribute(attributeName);

            if (attribute == null)
                return defaultValue;

            switch (ResolveVariables(attribute.Value, attribute).Trim().ToLower())
            {
                case "standard":
                    return DependencyResolutionStrategy.Standard;
                case "recursivefromfile":
                    return DependencyResolutionStrategy.RecursiveFromFile;
                case "recursivefromdirectory":
                    return DependencyResolutionStrategy.RecursiveFromDirectory;
                default:
                    // TODO docs
                    throw new XmlConfigurationException($"<{element.Name}> attribute '{attributeName}' is not a dependency resolution strategy. Expected 'standard', 'recursivefromfile' or 'recursivefromdirectory'.", $"{configurationDocsRoot}{element.Name}.html", attribute);
            }
        }

        /// <summary>
        ///     Reads a collection of attributes from the XML element supplied.
        /// </summary>
        /// <param name="element">The element to read from.</param>
        /// <param name="collection">The collection to read into.</param>
        internal void ReadAttributeCollectionTo(XElement element, NameValueCollection collection)
        {
            if (element == null)
                return;

            foreach (var attribute in element.Attributes())
                collection.Add(attribute.Name.LocalName, ResolveVariables(attribute.Value, attribute));
        }

        /// <summary>
        ///     Reads a collection of elements from the XML element supplied.
        /// </summary>
        /// <typeparam name="T">The type of the elements to read.</typeparam>
        /// <param name="element">The element to read from.</param>
        /// <param name="elementName">The name of the child elements to parse.</param>
        /// <param name="parseFunction">The function to parse each child element.</param>
        /// <param name="collection">The collection to write the elements to.</param>
        internal void ReadElementCollectionTo<T>(XElement element, string elementName, Func<XElement, T> parseFunction, ICollection<T> collection)
        {
            var results = element.Elements(elementName).Select(parseFunction);

            foreach (var result in results)
                collection.Add(result);
        }

        /// <summary>
        ///     Resolves variables to their values in the given string.
        /// </summary>
        /// <param name="str">The string to resolve variables in.</param>
        /// <param name="lineInfo">The line information about where this resolution is occuring.</param>
        /// <returns>The resolved string.</returns>
        internal string ResolveVariables(string str, IXmlLineInfo lineInfo)
        {
            Regex pattern = new Regex(@"\$\((\w+)\)");

            Match match;
            while ((match = pattern.Match(str)).Success)
            {
                string key = match.Groups[1].Value;
                string value = Variables[key];

                if (value == null)
                    throw new XmlConfigurationException("Unable to find variable for '" + key + "'.", variablesDocsPage, lineInfo);

                str = str.Substring(0, match.Index) + value + str.Substring(match.Index + match.Length);
            }

            return str;
        }

        /// <summary>
        /// Returns a child element or throws an exception if not present.
        /// </summary>
        /// <param name="from">The element to load from.</param>
        /// <param name="name">The name of the child element to return.</param>
        /// <returns>The child element.</returns>
        internal XElement GetRequiredElement(XElement from, string name)
        {
            return from.Element(name) ?? throw new XmlConfigurationException($"Missing required element <{name}> from <{from.Name}>.", $"{configurationDocsRoot}{from.Name}.html", from);
        }
    }
}
