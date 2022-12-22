/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using NUnit.Framework;

namespace DarkRift.Server.Tests
{
    public class PluginTestUtilTest
    {
        private class MockPlugin : ExtendedPluginBase
        {
            public MockPlugin(ExtendedPluginBaseLoadData pluginLoadData) : base(pluginLoadData)
            {
            }

            private Command[] commands;

            public override bool ThreadSafe => throw new NotImplementedException();
            public override Version Version => throw new NotImplementedException();
            public override Command[] Commands => commands;

            public void SetCommands(Command[] value)
            {
                commands = value;
            }
        }

        [Test]
        public void RunCommandOnWhenCommandExists()
        {
            string commandString = "my-command with arguments -and=many -f -l -a -g -s";

            // GIVEN a command with a handler
            Command command = new Command("my-command", "xyz", "abc", (sender, e) =>
            {
                // THEN the command handler was invoked with the correct args
                Assert.IsInstanceOf<PluginTestUtil>(sender);
                Assert.IsInstanceOf<CommandEventArgs>(e);
                Assert.AreEqual(e.OriginalCommand, commandString);
                Assert.AreEqual(e.Arguments.Length, 2);
                Assert.AreEqual(e.Arguments[1], "arguments");
                Assert.AreEqual(e.Flags.Count, 6);
                Assert.AreEqual(e.Flags.Get("and"), "many");
                Assert.AreEqual(e.RawArguments.Length, 8);
                Assert.AreEqual(e.RawArguments[3], "-f");
            });

            // AND a mock plugin with that command
            MockPlugin mockPlugin = new MockPlugin(new PluginLoadData(null, null, null, null, (Logger)null, null));
            mockPlugin.SetCommands(new Command[] { command });

            // WHEN the command is run on the plugin through the test util
            PluginTestUtil pluginTestUtil = new PluginTestUtil();
            pluginTestUtil.RunCommandOn(commandString, mockPlugin);
        }

        [Test]
        public void RunCommandOnWhenCommandDoesNotExists()
        {
            // AND a command with a different name
            Command command = new Command("not-this", "xyz", "abc", (sender, e) =>
            {
                Assert.IsInstanceOf<PluginTestUtil>(sender);
                Assert.IsInstanceOf<CommandEventArgs>(e);
            });

            // AND a mock plugin with that command
            MockPlugin mockPlugin = new MockPlugin(new PluginLoadData(null, null, null, null, (Logger)null, null));
            mockPlugin.SetCommands(new Command[] { command });

            // THEN an exception is thrown
            // TODO DR3 this is a poor choice of exception
            Assert.Throws<InvalidOperationException>(() =>
            {
                // WHEN the command is run on the plugin through the test util
                PluginTestUtil pluginTestUtil = new PluginTestUtil();
                pluginTestUtil.RunCommandOn("my-command with arguments -and=many -f -l -a -g -s", mockPlugin);
            });
        }
    }
}
