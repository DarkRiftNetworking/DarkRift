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
    internal static class StringUtility
    {
        /// <summary>
        ///     Formats a list of IPEndPoints nicely seperated by pipe characters.
        /// </summary>
        /// <param name="endPoints">The end points to format.</param>
        /// <returns>The formatted string.</returns>
        internal static string Format(this IEnumerable<IPEndPoint> endPoints)
        {
            return string.Join("|", endPoints.Select(r => r.ToString()).ToArray());
        }
    }
}
