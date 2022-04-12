/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

/***
 * Trie from https://github.com/aalhour/C-Sharp-Algorithms and modified.
 * 
 * This is the standard/vanilla implementation of a Trie. For an associative version of Trie, checkout the TrieMap<TRecord> class.
 * 
 * This class implements the IEnumerable interface.
 */

using System;
using System.Linq;
using System.Collections.Generic;

namespace DarkRift.Server.Plugins.Chat
{
#if PRO
    /// <summary>
    /// The vanila Trie implementation.
    /// </summary>
    public class Trie : IEnumerable<string>
    {
        private int _count { get; set; }
        private TrieNode _root { get; set; }

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        public Trie()
        {
            _count = 0;
            _root = new TrieNode(' ', false);
        }

        /// <summary>
        /// Return count of words.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Checks if element is empty.
        /// </summary>
        public bool IsEmpty => _count == 0;

        /// <summary>
        /// Add word to trie
        /// </summary>
        public void Add(string word)
        {
            if (string.IsNullOrEmpty(word))
                throw new ArgumentException("Word is empty or null.");

            var current = _root;

            for (int i = 0; i < word.Length; ++i)
            {
                if (!current.Children.ContainsKey(word[i]))
                {
                    var newTrieNode = new TrieNode(word[i]) { Parent = current };
                    current.Children.Add(word[i], newTrieNode);
                }

                current = current.Children[word[i]];
            }

            if (current.IsTerminal)
                throw new InvalidOperationException("Word already exists in Trie.");

            ++_count;
            current.IsTerminal = true;
        }

        /// <summary>
        /// Removes a word from the trie.
        /// </summary>
        public void Remove(string word)
        {
            if (string.IsNullOrEmpty(word))
                throw new ArgumentException("Word is empty or null.");

            var current = _root;

            for (int i = 0; i < word.Length; ++i)
            {
                if (!current.Children.ContainsKey(word[i]))
                    throw new KeyNotFoundException("Word doesn't belong to trie.");

                current = current.Children[word[i]];
            }

            if (!current.IsTerminal)
                throw new KeyNotFoundException("Word doesn't belong to trie.");

            --_count;
            current.Remove();
        }

        /// <summary>
        /// Checks whether the trie has a specific word.
        /// </summary>
        public bool ContainsWord(string word, int offset, int length)
        {
            if (string.IsNullOrEmpty(word))
                throw new InvalidOperationException("Word is either null or empty.");

            var current = _root;

            for (int i = 0; i < length; ++i)
            {
                if (!current.Children.ContainsKey(word[offset + i]))
                    return false;

                current = current.Children[word[offset + i]];
            }

            return current.IsTerminal;
        }

        /// <summary>
        /// Checks whether the trie has a specific prefix.
        /// </summary>
        public bool ContainsPrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
                throw new InvalidOperationException("Prefix is either null or empty.");

            var current = _root;

            for (int i = 0; i < prefix.Length; ++i)
            {
                if (!current.Children.ContainsKey(prefix[i]))
                    return false;

                current = current.Children[prefix[i]];
            }

            return true;
        }

        /// <summary>
        /// Searches the entire trie for words that has a specific prefix.
        /// </summary>
        public IEnumerable<string> SearchByPrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
                throw new InvalidOperationException("Prefix is either null or empty.");

            var current = _root;

            for (int i = 0; i < prefix.Length; ++i)
            {
                if (!current.Children.ContainsKey(prefix[i]))
                    return null;

                current = current.Children[prefix[i]];
            }

            return current.GetByPrefix();
        }

        /// <summary>
        /// Clears this insance.
        /// </summary>
        public void Clear()
        {
            _count = 0;
            _root.Clear();
            _root = new TrieNode(' ', false);
        }


        #region IEnumerable<String> Implementation
        /// <summary>
        /// IEnumerable.IEnumerator implementation.
        /// </summary>
        public IEnumerator<string> GetEnumerator()
        {
            return _root.GetTerminalChildren().Select(node => node.Word).GetEnumerator();
        }

        /// <summary>
        /// IEnumerable.IEnumerator implementation.
        /// </summary>
        /// <returns>The enumerator for this instance.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion IEnumerable<String> Implementation

    }
#endif
}
