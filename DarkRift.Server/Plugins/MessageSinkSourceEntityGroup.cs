/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;

#if PRO
namespace DarkRift.Server.Plugins
{
    /// <summary>
    ///     Holds a group of entities that implement <see cref="IMessageSinkSource"/> such as <see cref="IClient"/>.
    /// </summary>
    /// <remarks>
    ///     This type is not thread safe.
    /// </remarks>
    /// <typeparam name="T">The type of entities to contain.</typeparam>
    public class MessageSinkSourceEntityGroup<T> : EntityGroup<T>, IMessageSinkSource where T : IMessageSinkSource
    {
        /// <inheritdoc />
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        ///     Creates an empty <see cref="EntityGroup{T}"/>.
        /// </summary>
        public MessageSinkSourceEntityGroup()
        {

        }

        /// <summary>
        ///     Shallow copies the entities from the group provided.
        /// </summary>
        /// <param name="group">The group to copy elements from</param>
        public MessageSinkSourceEntityGroup(IEnumerable<T> group)
        {
            foreach (T item in group)
                Add(item);
        }

        /// <inheritdoc/>
        public override bool Add(T item)
        {
            bool success = base.Add(item);

            item.MessageReceived += MessageReceivedHandler;

            return success;
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            foreach (IMessageSinkSource item in this)
                item.MessageReceived -= MessageReceivedHandler;

            base.Clear();
        }

        /// <inheritdoc/>
        public override bool Remove(T item)
        {
            item.MessageReceived -= MessageReceivedHandler;

            return base.Remove(item);
        }

        /// <summary>
        ///     Handles a message received from any group memeber.
        /// </summary>
        /// <param name="sender">The group member that sent it.</param>
        /// <param name="e">The event args.</param>
        private void MessageReceivedHandler(object sender, MessageReceivedEventArgs e)
        {
            MessageReceived.Invoke(this, e);
        }

        /// <inheritdoc/>
        public bool SendMessage(Message message, SendMode sendMode)
        {
            bool failed = false;
            foreach (IMessageSinkSource item in this)
            {
                if (!item.SendMessage(message, sendMode))
                    failed = true;
            }

            return !failed;
        }
    }
}
#endif
