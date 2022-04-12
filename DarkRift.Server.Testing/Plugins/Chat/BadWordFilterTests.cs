/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

#if PRO
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Text.RegularExpressions;
using DarkRift.Server.Plugins.Chat;
using System.Collections.Specialized;
using DarkRift.Server;
using System.IO;

namespace DarkRift.Server.Plugins.Chat.Testing
{
    [TestClass]
    public class BadWordFilterTests
    {
        private BadWordFilter filter;

        [TestInitialize]
        public void Initialize()
        {
            filter = new BadWordFilter(new PluginLoadData("BadWordFilter", new NameValueCollection(), new DarkRiftInfo(DateTime.Now), new DarkRiftThreadHelper(false, null), (Logger)null, Path.GetTempPath()));

            //Populate with a set list of words here
            filter.PopulateBadWords(new string[] { "poop", "crap" });
        }

        [TestMethod]
        public void ContainsTest()
        {
            Assert.IsTrue(filter.ContainsBadWords("poop"));
        }

        [TestMethod]
        public void ContainsCaseInsensitiveTest()
        {
            Assert.IsTrue(filter.ContainsBadWords("pOOp"));
        }

        [TestMethod]
        public void FilterToCharTest()
        {
            Assert.AreEqual("You smell like ~~~~.", filter.FilterToChar("You smell like crap.", '~'));
        }

        [TestMethod]
        public void FilterToSymbolsTest()
        {
            Assert.IsTrue(Regex.IsMatch(filter.FilterToSymbols("Poop you."), @"^[\$#@%&\*!]{4} you."));
        }

        [TestMethod]
        public void FilterToRandomStringTest()
        {
            string a = filter.FilterToRandomString("Crap's going down.", new string[] { "Australia", "Gravity" });
            Assert.IsTrue(a == "Australia's going down." || a == "Gravity's going down.");
        }
    }
}
#endif
