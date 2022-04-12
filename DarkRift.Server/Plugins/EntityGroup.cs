/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections;
using System.Collections.Generic;

#if PRO
namespace DarkRift.Server.Plugins
{
    /// <summary>
    ///     Holds a group of entities.
    /// </summary>
    /// <remarks>
    ///     This type is not thread safe.
    /// </remarks>
    /// <typeparam name="T">The type of entities to contain.</typeparam>
    public class EntityGroup<T> : ICollection<T>
    {
        /// <inheritdoc/>
        public int Count => backing.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <summary>
        ///     The set backing this collection.
        /// </summary>
        private HashSet<T> backing = new HashSet<T>();

        /// <summary>
        ///     Creates an empty <see cref="EntityGroup{T}"/>.
        /// </summary>
        public EntityGroup()
        {

        }

        /// <summary>
        ///     Shallow copies the entities from the group provided.
        /// </summary>
        /// <param name="group">The group to copy elements from</param>
        public EntityGroup(IEnumerable<T> group)
        {
            foreach (T item in group)
                Add(item);
        }

        /// <inheritdoc/>
        public virtual bool Add(T item)
        {
            return backing.Add(item);
        }

        /// <inheritdoc/>
        public virtual void Clear()
        {
            backing.Clear();
        }

        /// <inheritdoc/>
        public virtual bool Contains(T item)
        {    
            return backing.Contains(item);
        }

        /// <inheritdoc/>
        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            backing.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public virtual IEnumerator<T> GetEnumerator()
        {
            return backing.GetEnumerator();
        }

        /// <inheritdoc/>
        public virtual bool Remove(T item)
        {
            return backing.Remove(item);
        }
        
        /// <inheritdoc/>
        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return backing.GetEnumerator();
        }
    }
}
#endif
