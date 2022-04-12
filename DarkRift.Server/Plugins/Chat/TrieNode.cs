/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;

namespace DarkRift.Server.Plugins.Chat
{
#if PRO
    /// <summary>
    /// The Trie Node.
    /// </summary>
    public class TrieNode
    {
        /// <summary>
        ///     The node's key.
        /// </summary>
        public virtual char Key { get; set; }

        /// <summary>
        ///     If this trie node is a leaf node.
        /// </summary>
        public virtual bool IsTerminal { get; set; }

        /// <summary>
        ///     The parent node of this trie node.
        /// </summary>
        public virtual TrieNode Parent { get; set; }
        
        /// <summary>
        ///     The child nodes of this trie node.
        /// </summary>
        public virtual Dictionary<char, TrieNode> Children { get; set; }

        /// <summary>
        ///     Creates a new non-leaf trie node with no children.
        /// </summary>
        public TrieNode(char key) : this(key, false) { }

        /// <summary>
        ///     Creates a new trie node with no children.
        /// </summary>
        /// <param name="key">This node's key.</param>
        /// <param name="isTerminal">True, if this node is a leaf node.</param>
        public TrieNode(char key, bool isTerminal)
        {
            Key = key;
            IsTerminal = isTerminal;
            Children = new Dictionary<char, TrieNode>();
        }

        /// <summary>
        /// Return the word at this node if the node is terminal; otherwise, return null
        /// </summary>
        public virtual string Word
        {
            get
            {
                if (!IsTerminal)
                    return null;

                var curr = this;
                var stack = new Stack<char>();

                while (curr.Parent != null)
                {
                    stack.Push(curr.Key);
                    curr = curr.Parent;
                }

                return new string(stack.ToArray());
            }

        }

        /// <summary>
        /// Returns an enumerable list of key-value pairs of all the words that start 
        /// with the prefix that maps from the root node until this node.
        /// </summary>
        public virtual IEnumerable<string> GetByPrefix()
        {
            if (IsTerminal)
                yield return Word;

            foreach (var childKeyVal in Children)
                foreach (var terminalNode in childKeyVal.Value.GetByPrefix())
                    yield return terminalNode;
        }

        /// <summary>
        /// Returns an enumerable collection of terminal child nodes.
        /// </summary>
        public virtual IEnumerable<TrieNode> GetTerminalChildren()
        {
            foreach (var child in Children.Values)
            {
                if (child.IsTerminal)
                    yield return child;

                foreach (var grandChild in child.GetTerminalChildren())
                    if (grandChild.IsTerminal)
                        yield return grandChild;
            }
        }

        /// <summary>
        /// Remove this element upto its parent.
        /// </summary>
        public virtual void Remove()
        {
            IsTerminal = false;

            if (Children.Count == 0 && Parent != null)
            {
                Parent.Children.Remove(Key);

                if (!Parent.IsTerminal)
                    Parent.Remove();
            }
        }

        /// <summary>
        /// IComparer interface implementation
        /// </summary>
        public int CompareTo(TrieNode other)
        {
            if (other == null)
                return -1;

            return this.Key.CompareTo(other.Key);

        }

        /// <summary>
        /// Clears this node instance
        /// </summary>
        public void Clear()
        {
            Children.Clear();
            Children = null;
        }
    }
#endif
}
