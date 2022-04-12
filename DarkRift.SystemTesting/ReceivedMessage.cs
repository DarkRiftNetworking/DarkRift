/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace DarkRift.SystemTesting
{
    /// <summary>
    ///     Holder for messages.
    /// </summary>
    public struct ReceivedMessage
    {
        public string Message { get; set; }
        public ushort Source { get; set; }
        public ushort Destination { get; set; }
        public ushort Tag { get; set; }
        public SendMode SendMode { get; set; }

        public ReceivedMessage(string message, ushort source, ushort destination, ushort tag, SendMode sendMode)
        {
            this.Message = message;
            this.Source = source;
            this.Destination = destination;
            this.Tag = tag;
            this.SendMode = sendMode;
        }
    }
}
