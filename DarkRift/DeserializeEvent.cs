/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkRift
{
    /// <summary>
    ///     Describes the deserialization in progress.
    /// </summary>
    public class DeserializeEvent
    {
        /// <summary>
        ///     The reader to read the data from.
        /// </summary>
        public DarkRiftReader Reader { get; }

        /// <summary>
        ///     Creates a new DeserializeEvent.
        /// </summary>
        /// <param name="reader">The reader to deserialize from.</param>
        public DeserializeEvent(DarkRiftReader reader)
        {
            Reader = reader;
        }
    }
}
