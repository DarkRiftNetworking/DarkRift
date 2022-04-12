/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DarkRift.Server.Configuration.Testing
{
    [TestClass]
    public class DarkRiftServerConfigurationBuilderTests
    {
        [TestMethod]
        public void TestWithDataDirectory()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN a data directory is set
            builder.WithDataDirectory("directory");

            // THEN the directory is set in the spawn data
            Assert.AreEqual("directory", builder.ServerSpawnData.Data.Directory);
        }

        [TestMethod]
        public void TestWithDispatcherExecutorThreadID()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN an executor thread ID is set
            builder.WithDispatcherExecutorThreadID(1234);

            // THEN the thread ID is set in the spawn data
            Assert.AreEqual(1234, builder.ServerSpawnData.DispatcherExecutorThreadID);
        }

        [TestMethod]
        public void TestWithEventsFromDispatcher()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN events from dispatcher is set
            builder.WithEventsFromDispatcher(true);

            // THEN the flag is set in the spawn data
            Assert.IsTrue(builder.ServerSpawnData.EventsFromDispatcher);
        }

        [TestMethod]
        public void TestAddListener()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN a listener is added
            builder.AddListener("name", "type", IPAddress.Broadcast, 1234);

            // THEN the listener is added to the spawn data
            Assert.AreEqual(1, builder.ServerSpawnData.Listeners.NetworkListeners.Count);
            Assert.AreEqual("name", builder.ServerSpawnData.Listeners.NetworkListeners[0].Name);
            Assert.AreEqual("type", builder.ServerSpawnData.Listeners.NetworkListeners[0].Type);
            Assert.AreEqual(IPAddress.Broadcast, builder.ServerSpawnData.Listeners.NetworkListeners[0].Address);
            Assert.AreEqual(1234, builder.ServerSpawnData.Listeners.NetworkListeners[0].Port);
            Assert.AreEqual(0, builder.ServerSpawnData.Listeners.NetworkListeners[0].Settings.Count);
        }

        [TestMethod]
        public void TestAddListenerWithSettings()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN a listener is added
            NameValueCollection settings = new NameValueCollection()
            {
                { "key", "value" }
            };
            builder.AddListener("name", "type", IPAddress.Broadcast, 1234, settings);

            // THEN the listener is added to the spawn data
            Assert.AreEqual(1, builder.ServerSpawnData.Listeners.NetworkListeners.Count);
            Assert.AreEqual("name", builder.ServerSpawnData.Listeners.NetworkListeners[0].Name);
            Assert.AreEqual("type", builder.ServerSpawnData.Listeners.NetworkListeners[0].Type);
            Assert.AreEqual(IPAddress.Broadcast, builder.ServerSpawnData.Listeners.NetworkListeners[0].Address);
            Assert.AreEqual(1234, builder.ServerSpawnData.Listeners.NetworkListeners[0].Port);
            Assert.AreEqual(1, builder.ServerSpawnData.Listeners.NetworkListeners[0].Settings.Count);
            Assert.AreEqual("value", builder.ServerSpawnData.Listeners.NetworkListeners[0].Settings["key"]);
        }

        [TestMethod]
        public void TestWithStartupLogLevels()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN startup log levels are set
            builder.WithStartupLogLevels(LogType.Trace, LogType.Fatal);

            // THEN the log levels are set in the spawn data
            Assert.AreEqual(2, builder.ServerSpawnData.Logging.StartupLogLevels.Length);
            Assert.AreEqual(LogType.Trace, builder.ServerSpawnData.Logging.StartupLogLevels[0]);
            Assert.AreEqual(LogType.Fatal, builder.ServerSpawnData.Logging.StartupLogLevels[1]);
        }

        [TestMethod]
        public void TestAddLogWriter()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN a log writer is added
            builder.AddLogWriter("name", "type", LogType.Trace);

            // THEN the log writer is added to the spawn data
            Assert.AreEqual(1, builder.ServerSpawnData.Logging.LogWriters.Count);
            Assert.AreEqual("name", builder.ServerSpawnData.Logging.LogWriters[0].Name);
            Assert.AreEqual("type", builder.ServerSpawnData.Logging.LogWriters[0].Type);
            Assert.AreEqual(1, builder.ServerSpawnData.Logging.LogWriters[0].LogLevels.Length);
            Assert.AreEqual(LogType.Trace, builder.ServerSpawnData.Logging.LogWriters[0].LogLevels[0]);
            Assert.AreEqual(0, builder.ServerSpawnData.Logging.LogWriters[0].Settings.Count);
        }

        [TestMethod]
        public void TestAddLogWriterWithSettings()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN a log writer is added
            NameValueCollection settings = new NameValueCollection()
            {
                { "key", "value" }
            };
            builder.AddLogWriter("name", "type", settings, LogType.Trace);

            // THEN the log writer is added to the spawn data
            Assert.AreEqual(1, builder.ServerSpawnData.Logging.LogWriters.Count);
            Assert.AreEqual("name", builder.ServerSpawnData.Logging.LogWriters[0].Name);
            Assert.AreEqual("type", builder.ServerSpawnData.Logging.LogWriters[0].Type);
            Assert.AreEqual(1, builder.ServerSpawnData.Logging.LogWriters[0].LogLevels.Length);
            Assert.AreEqual(LogType.Trace, builder.ServerSpawnData.Logging.LogWriters[0].LogLevels[0]);
            Assert.AreEqual(1, builder.ServerSpawnData.Logging.LogWriters[0].Settings.Count);
            Assert.AreEqual("value", builder.ServerSpawnData.Logging.LogWriters[0].Settings["key"]);
        }

        [TestMethod]
        public void TestWithPerMessageMetrics()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN per message metrics is set
            builder.WithPerMessageMetrics(true);

            // THEN the per message metrics is set in the spawn data
            Assert.IsTrue(builder.ServerSpawnData.Metrics.EnablePerMessageMetrics);
        }

        [TestMethod]
        public void TestWithMetricsWriter()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN a metrics writer is added
            builder.WithMetricsWriter("type");

            // THEN the metrics writer is added to the spawn data
            Assert.AreEqual("type", builder.ServerSpawnData.Metrics.MetricsWriter.Type);
            Assert.AreEqual(0, builder.ServerSpawnData.Metrics.MetricsWriter.Settings.Count);
        }

        [TestMethod]
        public void TestWithMetricsWriterWithSettings()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN a log writer is added
            NameValueCollection settings = new NameValueCollection()
            {
                { "key", "value" }
            };
            builder.WithMetricsWriter("type", settings);

            // THEN the metrics writer is added to the spawn data
            Assert.AreEqual("type", builder.ServerSpawnData.Metrics.MetricsWriter.Type);
            Assert.AreEqual(1, builder.ServerSpawnData.Metrics.MetricsWriter.Settings.Count);
            Assert.AreEqual("value", builder.ServerSpawnData.Metrics.MetricsWriter.Settings["key"]);
        }

        [TestMethod]
        public void TestWithLoadPluginsByDefault()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN load by default is set
            builder.WithLoadPluginsByDefault(true);

            // THEN the load by deafult is set in the spawn data
            Assert.IsTrue(builder.ServerSpawnData.Plugins.LoadByDefault);
        }

        [TestMethod]
        public void TestAddPlugin()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN a plugin is added
            builder.AddPlugin("type");

            // THEN the plugin is added to the spawn data
            Assert.AreEqual(1, builder.ServerSpawnData.Plugins.Plugins.Count);
            Assert.AreEqual("type", builder.ServerSpawnData.Plugins.Plugins[0].Type);
            Assert.IsTrue(builder.ServerSpawnData.Plugins.Plugins[0].Load);
            Assert.AreEqual(0, builder.ServerSpawnData.Plugins.Plugins[0].Settings.Count);
        }

        [TestMethod]
        public void TestAddPluginWithSettings()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN a log writer is added
            NameValueCollection settings = new NameValueCollection()
            {
                { "key", "value" }
            };
            // WHEN a plugin is added
            builder.AddPlugin("type", settings);

            // THEN the plugin is added to the spawn data
            Assert.AreEqual(1, builder.ServerSpawnData.Plugins.Plugins.Count);
            Assert.AreEqual("type", builder.ServerSpawnData.Plugins.Plugins[0].Type);
            Assert.IsTrue(builder.ServerSpawnData.Plugins.Plugins[0].Load);
            Assert.AreEqual(1, builder.ServerSpawnData.Plugins.Plugins[0].Settings.Count);
            Assert.AreEqual("value", builder.ServerSpawnData.Plugins.Plugins[0].Settings["key"]);
        }

        [TestMethod]
        public void TestExceptPlugin()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN a plugin is added
            builder.ExceptPlugin("type");

            // THEN the plugin is added to the spawn data
            Assert.AreEqual(1, builder.ServerSpawnData.Plugins.Plugins.Count);
            Assert.AreEqual("type", builder.ServerSpawnData.Plugins.Plugins[0].Type);
            Assert.IsFalse(builder.ServerSpawnData.Plugins.Plugins[0].Load);
            Assert.AreEqual(0, builder.ServerSpawnData.Plugins.Plugins[0].Settings.Count);
        }

        [TestMethod]
        public void TestAddPluginSearchPath()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN a plugin search path is added
            builder.AddPluginSearchPath("path");

            // THEN the plugin search path is added to the spawn data
            Assert.AreEqual(1, builder.ServerSpawnData.PluginSearch.PluginSearchPaths.Count);
            Assert.AreEqual("path", builder.ServerSpawnData.PluginSearch.PluginSearchPaths[0].Source);
            Assert.IsFalse(builder.ServerSpawnData.PluginSearch.PluginSearchPaths[0].CreateDirectory);
            Assert.AreEqual(DependencyResolutionStrategy.Standard, builder.ServerSpawnData.PluginSearch.PluginSearchPaths[0].DependencyResolutionStrategy);
        }

        [TestMethod]
        public void TestAddPluginSearchPathWithCreateDir()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN a plugin search path is added
            builder.AddPluginSearchPath("path", true);

            // THEN the plugin search path is added to the spawn data
            Assert.AreEqual(1, builder.ServerSpawnData.PluginSearch.PluginSearchPaths.Count);
            Assert.AreEqual("path", builder.ServerSpawnData.PluginSearch.PluginSearchPaths[0].Source);
            Assert.IsTrue(builder.ServerSpawnData.PluginSearch.PluginSearchPaths[0].CreateDirectory);
            Assert.AreEqual(DependencyResolutionStrategy.Standard, builder.ServerSpawnData.PluginSearch.PluginSearchPaths[0].DependencyResolutionStrategy);
        }

        [TestMethod]
        public void TestAddPluginSearchPathWithDependencyResolutionStrategy()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN a plugin search path is added
            builder.AddPluginSearchPath("path", DependencyResolutionStrategy.RecursiveFromDirectory);

            // THEN the plugin search path is added to the spawn data
            Assert.AreEqual(1, builder.ServerSpawnData.PluginSearch.PluginSearchPaths.Count);
            Assert.AreEqual("path", builder.ServerSpawnData.PluginSearch.PluginSearchPaths[0].Source);
            Assert.IsFalse(builder.ServerSpawnData.PluginSearch.PluginSearchPaths[0].CreateDirectory);
            Assert.AreEqual(DependencyResolutionStrategy.RecursiveFromDirectory, builder.ServerSpawnData.PluginSearch.PluginSearchPaths[0].DependencyResolutionStrategy);
        }

        [TestMethod]
        public void TestAddPluginSearchPathWithCreateDirAndDependencyResolutionStrategy()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN a plugin search path is added
            builder.AddPluginSearchPath("path", DependencyResolutionStrategy.RecursiveFromDirectory, true);

            // THEN the plugin search path is added to the spawn data
            Assert.AreEqual(1, builder.ServerSpawnData.PluginSearch.PluginSearchPaths.Count);
            Assert.AreEqual("path", builder.ServerSpawnData.PluginSearch.PluginSearchPaths[0].Source);
            Assert.IsTrue(builder.ServerSpawnData.PluginSearch.PluginSearchPaths[0].CreateDirectory);
            Assert.AreEqual(DependencyResolutionStrategy.RecursiveFromDirectory, builder.ServerSpawnData.PluginSearch.PluginSearchPaths[0].DependencyResolutionStrategy);
        }

        [TestMethod]
        public void TestAddPluginType()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN a plugin typer is added
            builder.AddPluginType(typeof(DateTime));

            // THEN the plugin search path is added to the spawn data
            Assert.AreEqual(1, builder.ServerSpawnData.PluginSearch.PluginTypes.Count);
            Assert.AreSame(typeof(DateTime), builder.ServerSpawnData.PluginSearch.PluginTypes[0]);
        }

        [TestMethod]
        public void TestWithMaxStrikes()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN max strikes are set
            builder.WithMaxStrikes(123);

            // THEN the max strikes are set in the spawn data
            Assert.AreEqual(123, builder.ServerSpawnData.Server.MaxStrikes);
        }

        [TestMethod]
        public void TestWithMaxReconnectAttempts()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN max reconnect attempts are set
            builder.WithMaxReconnectAttempts(123);

            // THEN the max reconnect attempts are set in the spawn data
            Assert.AreEqual(123, builder.ServerSpawnData.Server.ReconnectAttempts);
        }

#if PRO
        [TestMethod]
        public void TestWithServerGroup()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN the server group is set
            builder.WithServerGroup("group");

            // THEN the server group is set in the spawn data
            Assert.AreEqual("group", builder.ServerSpawnData.Server.ServerGroup);
        }
#endif

        [TestMethod]
        public void TestWithAdvertisedHost()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN the advertised host is set
            builder.WithAdvertisedHost("host");

            // THEN the host is set in the spawn data
            Assert.AreEqual("host", builder.ServerSpawnData.ServerRegistry.AdvertisedHost);
        }

        [TestMethod]
        public void TestWithAdvertisedPort()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN the advertised port is set
            builder.WithAdvertisedPort(1234);

            // THEN the port is set in the spawn data
            Assert.AreEqual(1234, builder.ServerSpawnData.ServerRegistry.AdvertisedPort);
        }

        [TestMethod]
        public void TestWithServerRegistryConnector()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN a connector is added
            builder.WithServerRegistryConnector("type");

            // THEN the connector is added to the spawn data
            Assert.AreEqual("type", builder.ServerSpawnData.ServerRegistry.ServerRegistryConnector.Type);
            Assert.AreEqual(0, builder.ServerSpawnData.ServerRegistry.ServerRegistryConnector.Settings.Count);
        }

        [TestMethod]
        public void TestWithServerRegistryConnectorWithSettings()
        {
            // GIVEN an empty config builder
            DarkRiftServerConfigurationBuilder builder = DarkRiftServerConfigurationBuilder.Create();

            // WHEN a log writer is added
            NameValueCollection settings = new NameValueCollection()
            {
                { "key", "value" }
            };
            // WHEN a connector is added
            builder.WithServerRegistryConnector("type", settings);

            // THEN the connector is added to the spawn data
            Assert.AreEqual("type", builder.ServerSpawnData.ServerRegistry.ServerRegistryConnector.Type);
            Assert.AreEqual(1, builder.ServerSpawnData.ServerRegistry.ServerRegistryConnector.Settings.Count);
            Assert.AreEqual("value", builder.ServerSpawnData.ServerRegistry.ServerRegistryConnector.Settings["key"]);
        }

    }
}
