/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace DarkRift.Server
{
    /// <summary>
    ///     An exception raised for invalid XML configuration files.
    /// </summary>
    [Serializable]
    public class XmlConfigurationException : Exception
    {
        /// <summary>
        /// The location of documentation for this issue.
        /// </summary>
        public string DocumentationLink { get; }

        /// <summary>
        /// The line information about where this issue is.
        /// </summary>
        public IXmlLineInfo LineInfo { get; }

        /// <summary>
        ///     Create and new exception for XML configurations with a message.
        /// </summary>
        /// <param name="msg">The message for the exception.</param>
        /// <param name="documentationLink">The location of documentation for this issue.</param>
        /// <param name="lineInfo">The line information about where this issue is.</param>
        public XmlConfigurationException(string msg, string documentationLink, IXmlLineInfo lineInfo) : base(msg) {
            DocumentationLink = documentationLink;
            LineInfo = lineInfo;
        }

        /// <summary>
        ///     Create and new exception for XML configurations with a message and inner exception.
        /// </summary>
        /// <param name="msg">The message for the exception.</param>
        /// <param name="documentationLink">The location of documentation for this issue.</param>
        /// <param name="lineInfo">The line information about where this issue is.</param>
        /// <param name="innerException">The inner exception.</param>
        public XmlConfigurationException(string msg, string documentationLink, IXmlLineInfo lineInfo, Exception innerException) : base(msg, innerException) {
            DocumentationLink = documentationLink;
            LineInfo = lineInfo;
        }
    }
}
