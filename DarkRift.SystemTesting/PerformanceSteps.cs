/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading;
using DarkRift.Client;
using DarkRift.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace DarkRift.SystemTesting
{
    [Binding]
    public class PerformanceSteps
    {

        /// <summary>
        ///     The number of <see cref="AutoRecyclingArray"/> objects that are allowed to be unaccounted for.
        /// </summary>
        public int ExpectedUnaccountedForAutoRecyclingArrays { get; set; } = 0;

        /// <summary>
        ///     The number of <see cref="DarkRiftReader"/> objects that are allowed to be unaccounted for.
        /// </summary>
        public int ExpectedUnaccountedForDarkRiftReaders { get; set; } = 0;

        /// <summary>
        ///     The number of <see cref="DarkRiftWriter"/> objects that are allowed to be unaccounted for.
        /// </summary>
        public int ExpectedUnaccountedForDarkRiftWriters { get; set; } = 0;

        /// <summary>
        ///     The number of <see cref="Message"/> objects that are allowed to be unaccounted for.
        /// </summary>
        public int ExpectedUnaccountedForMessages { get; set; } = 0;

        /// <summary>
        ///     The number of <see cref="MessageBuffer"/> objects that are allowed to be unaccounted for.
        /// </summary>
        public int ExpectedUnaccountedForMessageBuffers { get; set; } = 0;

        /// <summary>
        ///     The number of <see cref="SocketAsyncEventArgs"/> objects that are allowed to be unaccounted for.
        /// </summary>
        public int ExpectedUnaccountedForSocketAsyncEventArgs { get; set; } = 0;

        /// <summary>
        ///     The number of <see cref="Dispatching.ActionDispatcherTask"/> objects that are allowed to be unaccounted for.
        /// </summary>
        public int ExpectedUnaccountedForActionDispatcherTasks { get; set; } = 0;

        /// <summary>
        ///     The number of <see cref="Server.MessageReceivedEventArgs"/> objects that are allowed to be unaccounted for.
        /// </summary>
        public int ExpectedUnaccountedForServerMessageReceviedEventArgs { get; set; } = 0;

        /// <summary>
        ///     The number of <see cref="Server.ServerMessageReceivedEventArgs"/> objects that are allowed to be unaccounted for.
        /// </summary>
        public int ExpectedUnaccountedForServerServerMessageReceviedEventArgs { get; set; } = 0;

        /// <summary>
        ///     The number of <see cref="Client.MessageReceivedEventArgs"/> objects that are allowed to be unaccounted for.
        /// </summary>
        public int ExpectedUnaccountedForClientMessageReceviedEventArgs { get; set; } = 0;

        /// <summary>
        ///     The number of memory segments that are allowed to be unaccounted for.
        /// </summary>
        public int ExpectedUnaccountedForMemory { get; set; } = 0;

        [BeforeScenario]
        public void BeforeScenario()
        {
#if DEBUG
            // Ensure recycling issues are not carried over
            ObjectCacheTestHelper.ResetCounters();
            ServerObjectCacheTestHelper.ResetCounters();
            ClientObjectCacheTestHelper.ResetCounters();

            ObjectCacheHelper.ResetCounters();
            ServerObjectCacheHelper.ResetCounters();
            ClientObjectCacheHelper.ResetCounters();

            ExpectedUnaccountedForAutoRecyclingArrays = 0;
            ExpectedUnaccountedForDarkRiftReaders = 0;
            ExpectedUnaccountedForDarkRiftWriters = 0;
            ExpectedUnaccountedForMessages = 0;
            ExpectedUnaccountedForMessageBuffers = 0;
            ExpectedUnaccountedForSocketAsyncEventArgs = 0;
            ExpectedUnaccountedForActionDispatcherTasks = 0;
            ExpectedUnaccountedForServerMessageReceviedEventArgs = 0;
            ExpectedUnaccountedForServerServerMessageReceviedEventArgs = 0;
            ExpectedUnaccountedForClientMessageReceviedEventArgs = 0;
            ExpectedUnaccountedForMemory = 0;
#endif
        }

        /// <summary>
        ///     Make sure there are no recycling issues.
        /// </summary>
        [Then(@"there are no recycling issues")]
        public void ThenThereAreNoRecyclingWarnings()
        {
            AssertNoRecyclingIssues();
            AssertNoFinalizations();
        }

        /// <summary>
        ///     Asserts that all objects that were allowedUnaccountedFor from the <see cref="ObjectCache"/> were returned.
        /// </summary>
        private void AssertNoRecyclingIssues()
        {
#if DEBUG
            WaitUtility.WaitUntil("Objects unacounted for.", () => {
                //Check each metric
                int readers = ObjectCacheTestHelper.RetrievedDarkRiftReaders - ObjectCacheTestHelper.ReturnedDarkRiftReaders;
                if (readers != ExpectedUnaccountedForDarkRiftReaders)
                    Assert.Fail(readers + " DarkRiftReader objects are unaccounted for. Expected only " + ExpectedUnaccountedForDarkRiftReaders + ".");

                int writers = ObjectCacheTestHelper.ReturnedDarkRiftWriters - ObjectCacheTestHelper.ReturnedDarkRiftWriters;
                if (writers != ExpectedUnaccountedForDarkRiftWriters)
                    Assert.Fail(writers + " DarkRiftWriter objects are unaccounted for. Expected only " + ExpectedUnaccountedForDarkRiftWriters + ".");

                int messages = ObjectCacheTestHelper.ReturnedMessages - ObjectCacheTestHelper.ReturnedMessages;
                if (messages != ExpectedUnaccountedForMessages)
                    Assert.Fail(messages + " Message objects are unaccounted for. Expected only " + ExpectedUnaccountedForMessages + ".");

                int messageBuffers = ObjectCacheTestHelper.ReturnedMessageBuffers - ObjectCacheTestHelper.ReturnedMessageBuffers;
                if (messageBuffers != ExpectedUnaccountedForMessageBuffers)
                    Assert.Fail(messageBuffers + " MessageBuffer objects are unaccounted for. Expected only " + ExpectedUnaccountedForMessageBuffers + ".");

                int actionDispatcherTasks = ObjectCacheTestHelper.ReturnedActionDispatcherTasks - ObjectCacheTestHelper.ReturnedActionDispatcherTasks;
                if (actionDispatcherTasks != ExpectedUnaccountedForActionDispatcherTasks)
                    Assert.Fail(actionDispatcherTasks + " ActionDispatcherTask objects are unaccounted for. Expected only " + ExpectedUnaccountedForActionDispatcherTasks + ".");

                int autoRecyclingArrays = ObjectCacheTestHelper.ReturnedAutoRecyclingArrays - ObjectCacheTestHelper.ReturnedAutoRecyclingArrays;
                if (autoRecyclingArrays != ExpectedUnaccountedForAutoRecyclingArrays)
                    Assert.Fail(autoRecyclingArrays + " AutoRecyclingArray objects are unaccounted for. Expected only " + ExpectedUnaccountedForAutoRecyclingArrays + ".");

                int socketAsyncEventArgs = ObjectCacheTestHelper.RetrievedSocketAsyncEventArgs - ObjectCacheTestHelper.ReturnedSocketAsyncEventArgs;
                if (socketAsyncEventArgs != ExpectedUnaccountedForSocketAsyncEventArgs)
                    Assert.Fail(socketAsyncEventArgs + " SocketAsyncEventArgs objects are unaccounted for. Expected only " + ExpectedUnaccountedForSocketAsyncEventArgs + ".");

                int serverMessageReceivedEventArgs = ServerObjectCacheTestHelper.RetrievedMessageReceivedEventArgs - ServerObjectCacheTestHelper.ReturnedMessageReceivedEventArgs;
                if (serverMessageReceivedEventArgs != ExpectedUnaccountedForServerMessageReceviedEventArgs)
                    Assert.Fail(serverMessageReceivedEventArgs + " MessageReceivedEventArgs (server) objects are unaccounted for. Expected only " + ExpectedUnaccountedForServerMessageReceviedEventArgs + ".");
                
                int serverServerMessageReceivedEventArgs = ServerObjectCacheTestHelper.RetrievedServerMessageReceivedEventArgs - ServerObjectCacheTestHelper.ReturnedServerMessageReceivedEventArgs;
                if (serverServerMessageReceivedEventArgs != ExpectedUnaccountedForServerMessageReceviedEventArgs)
                    Assert.Fail(serverServerMessageReceivedEventArgs + " ServerMessageReceivedEventArgs (server) objects are unaccounted for. Expected only " + ExpectedUnaccountedForServerServerMessageReceviedEventArgs + ".");

                int clientMessageReceivedEventArgs = ClientObjectCacheTestHelper.RetrievedMessageReceivedEventArgs - ClientObjectCacheTestHelper.ReturnedMessageReceivedEventArgs;
                if (clientMessageReceivedEventArgs != ExpectedUnaccountedForClientMessageReceviedEventArgs)
                    Assert.Fail(clientMessageReceivedEventArgs + " MessageReceivedEventArgs (client) objects are unaccounted for. Expected only " + ExpectedUnaccountedForClientMessageReceviedEventArgs + ".");

                int memory = ObjectCacheTestHelper.ReturnedMemory - ObjectCacheTestHelper.ReturnedMemory;
                if (memory != ExpectedUnaccountedForMemory)
                    Assert.Fail(memory + " memory segments are unaccounted for. Expected only " + ExpectedUnaccountedForMemory + ".");
            });
#endif
        }

        /// <summary>
        ///     Asserts that no finalizations on recyclable objects occurred.
        /// </summary>
        private void AssertNoFinalizations()
        {
            // Now everything's disposed we can assert that all objects are accounted for
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (ObjectCacheHelper.FinalizedDarkRiftReaders > 0)
                Assert.Fail(ObjectCacheHelper.FinalizedDarkRiftReaders + " DarkRiftReader objects were finalized last period.");

            if (ObjectCacheHelper.FinalizedDarkRiftWriters > 0)
                Assert.Fail(ObjectCacheHelper.FinalizedDarkRiftWriters + " DarkRiftWriter objects were finalized last period.");

            if (ObjectCacheHelper.FinalizedMessages > 0)
                Assert.Fail(ObjectCacheHelper.FinalizedMessages + " Message objects were finalized last period.");

            if (ObjectCacheHelper.FinalizedMessageBuffers > 0)
                Assert.Fail(ObjectCacheHelper.FinalizedMessageBuffers + " MessageBuffer objects were finalized last period.");

            if (ObjectCacheHelper.FinalizedAutoRecyclingArrays > 0)
                Assert.Fail(ObjectCacheHelper.FinalizedAutoRecyclingArrays + " AutoRecyclingArray objects were finalized last period.");

            if (ServerObjectCacheHelper.FinalizedMessageReceivedEventArgs > 0)
                Assert.Fail(ServerObjectCacheHelper.FinalizedMessageReceivedEventArgs + " MessageReceivedEventArgs (server) objects were finalized last period.");

#if PRO
            if (ServerObjectCacheHelper.FinalizedServerMessageReceivedEventArgs > 0)
                Assert.Fail(ServerObjectCacheHelper.FinalizedServerMessageReceivedEventArgs + " ServeMessageReceivedEventArgs (server) objects were finalized last period.");
#endif
            if (ClientObjectCacheHelper.FinalizedMessageReceivedEventArgs > 0)
                Assert.Fail(ClientObjectCacheHelper.FinalizedMessageReceivedEventArgs + " MessageReceivedEventArgs (client) objects were finalized last period.");
        }
    }
}
