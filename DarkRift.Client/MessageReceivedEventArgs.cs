/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkRift.Client
{
    /// <summary>
    ///     Arguments for <see cref="DarkRiftClient.MessageReceived">MessageReceived</see> events.
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs, IDisposable
    {
        /// <summary>
        ///     The method the data was sent using.
        /// </summary>
        public SendMode SendMode { get; private set;  }
        
        /// <summary>
        ///     The tag the message was sent with.
        /// </summary>
        public ushort Tag => message.Tag;

        /// <summary>
        ///     The message received.
        /// </summary>
        private Message message;

        /// <summary>
        ///     Whether this args object is currently in an object pool waiting or not.
        /// </summary>
        private volatile bool isCurrentlyLoungingInAPool;

        /// <summary>
        ///     Creates a new args object for the <see cref="DarkRiftClient.MessageReceived"/> event.
        /// </summary>
        /// <param name="message">The message received.</param>
        /// <param name="sendMode">The send mode the message was received with.</param>
        public static MessageReceivedEventArgs Create(Message message, SendMode sendMode)
        {
            MessageReceivedEventArgs messageReceivedEventArgs = ClientObjectCache.GetMessageReceivedEventArgs();

            messageReceivedEventArgs.message = message;
            messageReceivedEventArgs.SendMode = sendMode;

            messageReceivedEventArgs.isCurrentlyLoungingInAPool = false;

            return messageReceivedEventArgs;
        }

        /// <summary>
        ///     Creates a new MessageReceivedEventArgs. For use from the ObjectCache.
        /// </summary>
        internal MessageReceivedEventArgs()
        {

        }

        /// <summary>
        ///     Gets the message received.
        /// </summary>
        /// <returns>An new instance of the message received.</returns>
        public Message GetMessage()
        {
            return message.Clone();
        }

        /// <summary>
        ///     Recycles this object back into the pool.
        /// </summary>
        public void Dispose()
        {
            ClientObjectCache.ReturnMessageReceivedEventArgs(this);
            isCurrentlyLoungingInAPool = true;
        }

        /// <summary>
        ///     Finalizer so we can inform the cache system we were not recycled correctly.
        /// </summary>
        ~MessageReceivedEventArgs()
        {
            if (!isCurrentlyLoungingInAPool)
                ClientObjectCacheHelper.MessageReceivedEventArgsWasFinalized();
        }
    }
}
