/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace DarkRift.Server.Plugins.Chat
{
#if PRO
    /// <summary>
    ///     Helper plugin for filtering bad words out of text. To access an instance use <see cref="Plugin.BadWordFilter"/>.
    /// </summary>
    /// <remarks>
    ///     <c>Pro only.</c>
    /// </remarks>
    public interface IBadWordFilter
    {
        /// <summary>
        ///     Analyzes if the string contains bad words or not.
        /// </summary>
        /// <param name="text">The string to analyze.</param>
        /// <returns>Whether the string contains bad words.</returns>
        bool ContainsBadWords(string text);
        
        /// <summary>
        ///     Filters a string so that all bad words are replaced with the given char.
        /// </summary>
        /// <param name="text">The string to filter.</param>
        /// <param name="c">The char to replace bad words with.</param>
        /// <returns>The filtered string.</returns>
        string FilterToChar(string text, char c = '*');

        /// <summary>
        ///     Filters a string so that all bad words are replaced with a randomly chosen string.
        /// </summary>
        /// <param name="text">The string to filter.</param>
        /// <param name="replacements">The strings to replace bad words with.</param>
        /// <returns>The filtered string.</returns>
        string FilterToRandomString(string text, string[] replacements);
        
        /// <summary>
        ///     Filters a string so that all bad words are replaced with random symbols.
        /// </summary>
        /// <param name="text">The string to filter.</param>
        /// <returns>The filtered string.</returns>
        string FilterToSymbols(string text);

        /// <summary>
        ///     Downloads a bad word list from the specifed location.
        /// </summary>
        /// <param name="logConfirmation">If true, an Info log will be written on success.</param>
        void UpdateBadWordList(bool logConfirmation);
    }
#endif
}