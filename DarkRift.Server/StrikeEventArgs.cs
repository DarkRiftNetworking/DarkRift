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
#if PRO
    /// <summary>
    ///     Event arguments for the <see cref="Client.StrikeOccured"/> event.
    /// </summary>
    public sealed class StrikeEventArgs : EventArgs
    {
        /// <summary>
        ///     The reason the strike was given.
        /// </summary>
        public StrikeReason Reason { get; private set; }

        /// <summary>
        ///     The message supplied with the strike.
        /// </summary>
        /// <remarks>
        ///     May be null in the case that no message is supplied.
        /// </remarks>
        public string Message { get; private set; }

        /// <summary>
        ///     Has this strike been forgiven by a plugin?
        /// </summary>
        public bool Forgiven { get; private set; }

        /// <summary>
        ///     The number of strikes this accounts for.
        /// </summary>
        public int Weight {
            get => weight;
            set
            {
                if (value < 1)
                    throw new ArgumentException("Weight nust be at least 1. Use Forgive() instead of setting weight to zero.");

                weight = value;
            }
        }

        /// <summary>
        ///     The number of strikes this accounts for.
        /// </summary>
        private int weight;

        /// <summary>
        ///     Creates a new StrikeEventArgs object.
        /// </summary>
        /// <param name="reason">The reason for the strike.</param>
        /// <param name="message">The message supplied with the strike.</param>
        /// <param name="weight">The weight of the strike.</param>
        public StrikeEventArgs(StrikeReason reason, string message, int weight)
        {
            this.Reason = reason;
            this.Message = message;
            this.weight = weight;
        }

        /// <summary>
        ///     Forgives the client of this strike so it will not count against them.
        /// </summary>
        public void Forgive()
        {
            Forgiven = true;
        }
    }
#endif
}
