/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace DarkRift.Server.Plugins.Chat
{
#if PRO
    /// <inheritdoc/>
    internal class BadWordFilter : Plugin, IBadWordFilter
    {
        /// <inheritdoc />
        public override Version Version => new Version(1, 0, 0);

        /// <inheritdoc />
        public override Command[] Commands => new Command[]
        {
            new Command("badwordfilter", "Allows you to make changes to the bad word filter.", "badwordfilter update", CommandHandler)
        };

        /// <inheritdoc />
        public override bool ThreadSafe => true;

        /// <inheritdoc />
        internal override bool Hidden => true;

        /// <summary>
        ///     Symbols that can be used in the FilterToSymbols method.
        /// </summary>
        private readonly char[] replacementSymbols;

        /// <summary>
        ///     The location of the bad word list on the file system.
        /// </summary>
        private readonly string listLocation;

        /// <summary>
        ///     The location of the bad word list to download.
        /// </summary>
        private readonly string url;

        /// <summary>
        ///     The trie for matching bad words.
        /// </summary>
        private readonly Trie trie = new Trie();

        private struct BadWordMatch
        {
            public int Offset { get; set; }
            public int Length { get; set; }
        }

        /// <summary>
        ///     Creates a new bad word list plugin.
        /// </summary>
        /// <param name="pluginLoadData">The plugin load data from the server.</param>
        public BadWordFilter(PluginLoadData pluginLoadData)
            : base(pluginLoadData)
        {
            listLocation = Path.Combine(ResourceDirectory, "BadWordList.xml");

            replacementSymbols = (pluginLoadData.Settings["replacementSymbols"] ?? "$#@%&*!").ToCharArray();
            url = pluginLoadData.Settings["url"] ?? "https://darkriftnetworking.com/DarkRift2/Resources/BadWords.xml";
        }
        
        protected internal override void Loaded(LoadedEventArgs args)
        {
            if (File.Exists(listLocation))
            {
                try
                {
                    ReloadBadWordList();
                }
                catch
                {
                    //File may be corrupt, redownload from source
                    UpdateBadWordList(false);
                }
            }
            else
            {
                UpdateBadWordList(false);
            }
        }

        /// <summary>
        ///     Handles invocation of the badwordfilter command.
        /// </summary>
        /// <param name="sender">The command engine.</param>
        /// <param name="e">The arguments for the invocation.</param>
        private void CommandHandler(object sender, CommandEventArgs e)
        {
            if (e.Arguments.Length != 1)
                throw new CommandSyntaxException();

            if (e.Arguments[0] != "update")
                throw new CommandSyntaxException();

            Logger.Info($"Updating bad word list from {url}.");

            UpdateBadWordList(true);
        }

        /// <inheritdoc/>
        public void UpdateBadWordList(bool logConfirmation)
        {
            WebClient client = new WebClient();

            Logger.Trace($"Downloading bad word list from '{url}'.");

            client.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) =>
            {
                if (e.Error != null)
                {
                    Logger.Error($"Could not download bad word list from remote. The bad word filter cannot filter text without a source.\n\nConsider trying to reload the bad word list with the command 'badwordfilter update' or point the 'url' setting a an accessible source of bad words to filter (https://www.darkriftnetworking.com/DarkRift2/Docs/{ServerInfo.Version}/advanced/internal_plugins/bad_word_filter.html).", e.Error);
                }
                else
                {
                    ReloadBadWordList();

                    if (logConfirmation)
                        Logger.Info("Sucessfully updated bad word list.", e.Error);
                }
            };

            client.DownloadFileAsync(new Uri(url), listLocation);
        }
        
        /// <inheritdoc/>
        public bool ContainsBadWords(string text)
        {
            return Search(text).Any();
        }

        /// <inheritdoc/>
        public string FilterToChar(string text, char c = '*')
        {
            IEnumerable<BadWordMatch> matches = Search(text);

            char[] chars = text.ToCharArray();

            foreach (BadWordMatch match in matches)
            {
                for (int i = 0; i < match.Length; i++)
                    chars[match.Offset + i] = c;
            }

            return new string(chars);
        }

        /// <inheritdoc/>
        public string FilterToSymbols(string text)
        {
            IEnumerable<BadWordMatch> matches = Search(text);

            Random r = new Random();

            char[] chars = text.ToCharArray();

            foreach (BadWordMatch match in matches)
            {
                for (int i = 0; i < match.Length; i++)
                    chars[match.Offset + i] = replacementSymbols[r.Next(replacementSymbols.Length)];
            }

            return new string(chars);
        }

        /// <inheritdoc/>
        public string FilterToRandomString(string text, string[] replacements)
        {
            IEnumerable<BadWordMatch> matches = Search(text);

            Random r = new Random();

            foreach (BadWordMatch match in matches)
            {
                text = text.Substring(0, match.Offset) + replacements[r.Next(replacements.Length)] + text.Substring(match.Offset + match.Length);
            }

            return text;
        }

        /// <summary>
        ///     Reloads the regexes from the bad word list. Use <see cref="UpdateBadWordList(bool)"/> to download the latest list.
        /// </summary>
        private void ReloadBadWordList()
        {
            Logger.Trace($"Loading bad word list.");

            XElement root = XDocument.Load(listLocation).Root;

            PopulateBadWords(root.Elements().Select(x => x.Value));

            Logger.Trace("Bad word list loaded sucessfully!");
        }
        
        /// <summary>
        ///     THis
        /// </summary>
        internal void PopulateBadWords(IEnumerable<string> words)
        {
            trie.Clear();
            foreach (string word in words)
            {
                try
                {
                    trie.Add(word);
                }
                catch (InvalidOperationException)
                {
                    //We've added the same word twice, move on
                }
            }
        }

        /// <summary>
        ///     Searches for words that are evil.
        /// </summary>
        /// <param name="text">The text to search.</param>
        /// <returns>An iterable of words not allowed.</returns>
        private IEnumerable<BadWordMatch> Search(string text)
        {
            text = text.ToLower();

            for (int i = 0; i < text.Length; i++)
            {
                int start = i;
                while (i < text.Length && char.IsLetter(text[i])) i++;      //TODO Won't match multiword phrases
                int length = i - start;

                if (trie.ContainsWord(text, start, length))
                    yield return new BadWordMatch { Offset = start, Length = length};
            }
        }
    }
#endif
}
