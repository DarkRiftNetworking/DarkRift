/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkRift.DataStructures
{
    /// <summary>
    ///     A dictionary with limited spaces, once all spaces are filled the dictionary will remove the oldest elements to add new element.
    /// </summary>
    /// <typeparam name="K">The type of the key.</typeparam>
    /// <typeparam name="V">The type of the value.</typeparam>
    /// <remarks>
    ///     A number of standard dictionary methods are no implemented as they are not needed in DarkRift.
    /// </remarks>
    internal class CircularDictionary<K, V> : IDictionary<K, V> where K : IEquatable<K>
    {
        /// <summary>
        ///     The backing array behind the dictionary.
        /// </summary>
        private readonly KeyValuePair<K, V>[] backing;

        /// <summary>
        ///     The element we will next insert into.
        /// </summary>
        private int ptr;

        public V this[K key] {
            get
            {
                lock (backing)
                {
                    for (int i = 0; i < backing.Length; i++)
                    {
                        if (backing[i].Key.Equals(key))
                            return backing[i].Value;
                    }
                }

                throw new KeyNotFoundException();
            }

            set
            {
                lock (backing)
                {
                    for (int i = 0; i < backing.Length; i++)
                    {
                        if (backing[i].Key.Equals(key))
                            backing[i] = new KeyValuePair<K, V>(key, value);
                    }
                }

                throw new KeyNotFoundException();
            }
        }

        public ICollection<K> Keys => throw new NotImplementedException();

        public ICollection<V> Values => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => false;

        public CircularDictionary(int size)
        {
            backing = new KeyValuePair<K, V>[size];
        }

        public void Add(K key, V value)
        {
            Add(new KeyValuePair<K, V>(key, value));
        }

        public void Add(KeyValuePair<K, V> item)
        {
            lock (backing)
            {
                backing[ptr] = item;
                ptr = (ptr + 1) % backing.Length;
            }
        }

        public void Clear()
        {
            lock (backing)
            {
                for (int i = 0; i < backing.Length; i++)
                    backing[i] = default;
            }
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            lock (backing)
            {
                for (int i = 0; i < backing.Length; i++)
                {
                    if (backing[i].Equals(item))
                        return true;
                }
            }

            return false;
        }

        public bool ContainsKey(K key)
        {
            lock (backing)
            {
                for (int i = 0; i < backing.Length; i++)
                {
                    if (backing[i].Key.Equals(key))
                        return true;
                }
            }

            return false;
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(K key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(K key, out V value)
        {
            lock (backing)
            {
                for (int i = 0; i < backing.Length; i++)
                {
                    if (backing[i].Key.Equals(key))
                    {
                        value = backing[i].Value;
                        return true;
                    }
                }
            }

            value = default;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
