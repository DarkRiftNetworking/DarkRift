/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.IO;
using NUnit.Framework;

namespace DarkRift.Server.Plugins.Chat.Tests
{
    public class BadWordFilterTests
    {
        private BadWordFilter filter;

        [SetUp]
        public void SetUp()
        {
            filter = new BadWordFilter(new PluginLoadData("BadWordFilter", new NameValueCollection(), new DarkRiftInfo(DateTime.Now), new DarkRiftThreadHelper(false, null), (Logger)null, Path.GetTempPath()));

            //Populate with a set list of words here
            filter.PopulateBadWords(new string[] { "poop", "crap" });
        }

        [Test]
        public void Contains()
        {
            Assert.IsTrue(filter.ContainsBadWords("poop"));
        }

        [Test]
        public void ContainsCaseInsensitive()
        {
            Assert.IsTrue(filter.ContainsBadWords("pOOp"));
        }

        [Test]
        public void FilterToChar()
        {
            Assert.AreEqual("You smell like ~~~~.", filter.FilterToChar("You smell like crap.", '~'));
        }

        [Test]
        public void FilterToSymbols()
        {
            Assert.IsTrue(Regex.IsMatch(filter.FilterToSymbols("Poop you."), @"^[\$#@%&\*!]{4} you."));
        }

        [Test]
        public void FilterToRandomString()
        {
            string a = filter.FilterToRandomString("Crap's going down.", new string[] { "Australia", "Gravity" });
            Assert.IsTrue(a == "Australia's going down." || a == "Gravity's going down.");
        }
    }
}
