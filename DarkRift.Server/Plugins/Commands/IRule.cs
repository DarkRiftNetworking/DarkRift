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

namespace DarkRift.Server.Plugins.Commands
{
    /// <summary>
    ///     Base interface for sniffer rules.
    /// </summary>
    internal interface IRule
    {
        bool Accepts(Message message, Client client);
    }

    internal struct ALLTHERULES : IRule
    {
        public bool Accepts(Message message, Client client)
        {
            return true;
        }

        public override string ToString()
        {
            return "all";
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Rule return false.
            if (!(obj is ALLTHERULES))
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }

    internal struct TagRule : IRule
    {
        private ushort Tag { get; set; }

        public TagRule(ushort tag)
        {
            this.Tag = tag;
        }

        public bool Accepts(Message message, Client client)
        {
            return message.Tag == Tag;
        }

        public override string ToString()
        {
            return $"Tag={this.Tag}";
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Rule return false.
            if (!(obj is TagRule))
            {
                return false;
            }

            TagRule r = (TagRule)obj;

            // Return true if the fields match
            return Tag == r.Tag;
        }

        public override int GetHashCode()
        {
            return Tag;
        }
    }

    internal struct IDRule : IRule
    {
        private uint ID { get; set; }

        public IDRule(uint id)
        {
            this.ID = id;
        }

        public bool Accepts(Message message, Client client)
        {
            return client.ID == ID;
        }

        public override string ToString()
        {
            return $"ID={this.ID}";
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Rule return false.
            if (!(obj is IDRule))
            {
                return false;
            }

            IDRule r = (IDRule)obj;

            // Return true if the fields match
            return ID == r.ID;
        }

        public override int GetHashCode()
        {
            return (int)ID;
        }
    }

    internal struct IPRule : IRule
    {
        private IPAddress IP { get; set; }

        public IPRule(IPAddress ip)
        {
            this.IP = ip;
        }

        public bool Accepts(Message message, Client client)
        {
            return client.RemoteTcpEndPoint.Address.Equals(IP) || client.RemoteUdpEndPoint.Address.Equals(IP);     //TCP and UDP addresses should be the same at the moment but you never know
        }

        public override string ToString()
        {
            return $"IP={this.IP}";
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Rule return false.
            if (!(obj is IPRule))
            {
                return false;
            }

            IPRule r = (IPRule)obj;

            // Return true if the fields match
            return IP.Equals(r.IP);
        }

        public override int GetHashCode()
        {
            return IP.GetHashCode();
        }
    }
}
