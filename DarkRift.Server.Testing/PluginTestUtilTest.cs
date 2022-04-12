/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DarkRift.Server.Testing
{
    [TestClass]
    public class PluginTestUtilTest
    {
        [TestMethod]
        public void TestRunCommandOnWhenCommandExists()
        {
            // GIVEN a mock event handler
            Mock<EventHandler<CommandEventArgs>> mockHandler = new Mock<EventHandler<CommandEventArgs>>();
            mockHandler.Setup(h => h(It.IsAny<PluginTestUtil>(), It.IsAny<CommandEventArgs>()));

            // AND a command with that handler
            Command command = new Command("my-command", "xyz", "abc", mockHandler.Object);

            // AND a mock plugin with that command
            Mock<ExtendedPluginBase> mockPlugin = new Mock<ExtendedPluginBase>(new PluginLoadData(null, null, null, null, (Logger)null, null));
            mockPlugin.Setup(p => p.Commands).Returns(new Command[] { command });

            // WHEN the command is run on the plugin through the test util
            PluginTestUtil pluginTestUtil = new PluginTestUtil();
            pluginTestUtil.RunCommandOn("my-command with arguments -and=many -f -l -a -g -s", mockPlugin.Object);

            // THEN the command handler was invoked with the correct args
            mockHandler.Verify(h => h(pluginTestUtil, It.Is<CommandEventArgs>(a =>
                a.OriginalCommand == "my-command with arguments -and=many -f -l -a -g -s"
                    && a.Arguments.Length == 2
                    && a.Arguments[1] == "arguments"
                    && a.Command == command
                    && a.Flags.Count == 6
                    && a.Flags.Get("and") == "many"
                    && a.RawArguments.Length == 8
                    && a.RawArguments[3] == "-f"
            )), Times.Once);
        }

        [TestMethod]
        public void TestRunCommandOnWhenCommandDoesNotExists()
        {
            // GIVEN a mock event handler
            Mock<EventHandler<CommandEventArgs>> mockHandler = new Mock<EventHandler<CommandEventArgs>>();
            mockHandler.Setup(h => h(It.IsAny<PluginTestUtil>(), It.IsAny<CommandEventArgs>()));

            // AND a command with a different name
            Command command = new Command("not-this", "xyz", "abc", mockHandler.Object);

            // AND a mock plugin with that command
            Mock<ExtendedPluginBase> mockPlugin = new Mock<ExtendedPluginBase>(new PluginLoadData(null, null, null, null, (Logger)null, null));
            mockPlugin.Setup(p => p.Commands).Returns(new Command[] { command });

            // THEN an exception is thrown
            // TODO DR3 this is a poor choice of exception
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                // WHEN the command is run on the plugin through the test util
                PluginTestUtil pluginTestUtil = new PluginTestUtil();
                pluginTestUtil.RunCommandOn("my-command with arguments -and=many -f -l -a -g -s", mockPlugin.Object);
            });
        }
    }
}
