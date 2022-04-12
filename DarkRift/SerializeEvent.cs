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
    ///     DEscribes the serialization in progress.
    /// </summary>
    public class SerializeEvent
    {
        /// <summary>
        ///     The writer to write the object data to.
        /// </summary>
        public DarkRiftWriter Writer { get; }

        /// <summary>
        ///     Creates a new SerializeEvent.
        /// </summary>
        /// <param name="writer">The writer to serialize to.</param>
        public SerializeEvent(DarkRiftWriter writer)
        {
            Writer = writer;
        }
    }
}
