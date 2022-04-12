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
    ///     Interface for specifying how an object is serialized and deserialized.
    /// </summary>
    public interface IDarkRiftSerializable
    {
        /// <summary>
        ///     Deserializes a DarkRiftReader into the object.
        /// </summary>
        /// <param name="e">Details about the deserialization.</param>
        void Deserialize(DeserializeEvent e);

        /// <summary>
        ///     Serializes the object to the DarkRiftWriter.
        /// </summary>
        /// <param name="e">Details about the serialization.</param>
        void Serialize(SerializeEvent e);
    }
}
