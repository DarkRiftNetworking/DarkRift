/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkRift.Server.Plugins.Commands
{
    /// <summary>
    ///     Group of sniffer rules.
    /// </summary>
    internal class RuleGroup : HashSet<IRule>
    {
        /// <summary>
        ///     Whether this rull group should show data on log or not.
        /// </summary>
        public bool OutputData { get; }

        public RuleGroup(bool outputData)
        {
            this.OutputData = outputData;
        }

        internal bool Accepts(Message message, Client client)
        {
            foreach (IRule rule in this)
            {
                if (!rule.Accepts(message, client))
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            string[] outs = new string[Count];

            int i = 0;

            foreach (IRule rule in this)
            {
                outs[i++] = rule.ToString();
            }

            return string.Join(" ", outs);
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to RuleGroup return false.
            if (!(obj is RuleGroup))
            {
                return false;
            }

            RuleGroup g = (RuleGroup)obj;

            // Return true if the inside rules match
            foreach (IRule rule in this)
            {
                if (!g.Contains(rule))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int i = 17;
            foreach (IRule rule in this)
                i = (i + rule.GetHashCode()) * 31;

            return i;
        }
    }
}
