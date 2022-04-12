/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkRift.Server
{
    /// <summary>
    ///     Interface for places messages can be sent to and from.
    /// </summary>
    public interface IMessageSinkSource
    {
        /// <summary>
        ///     Event fired when a message is received from this entity.
        /// </summary>
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
        
        /// <summary>
        ///     Sends a message to the client.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="sendMode">How the message should be sent.</param>
        /// <returns>Whether the send was successful.</returns>
        bool SendMessage(Message message, SendMode sendMode);
    }
}
