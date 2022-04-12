/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using DarkRift.Dispatching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DarkRift.Testing.Dispatching
{
    [TestClass]
    public class ActionDispatcherTaskTests
    {
        [TestInitialize]
        public void Initialize()
        {
            //Object cache needs to be initialized for create
#pragma warning disable CS0618      // We don't care about using Server/Client specific cache settings
            ObjectCache.Initialize(ObjectCacheSettings.DontUseCache);
#pragma warning restore CS0618
        }

        [TestMethod]
        public void TestExecuteSynchronous()
        {
            bool set = false;
            ActionDispatcherTask task = ActionDispatcherTask.Create(() => set = true);

            Assert.AreEqual(DispatcherTaskState.Queued, task.TaskState);

            task.Execute(true);

            Assert.IsTrue(set);
            Assert.AreEqual(DispatcherTaskState.CompletedImmediate, task.TaskState);
        }

        [TestMethod]
        public void TestExecuteAsynchronous()
        {
            bool set = false;
            ActionDispatcherTask task = ActionDispatcherTask.Create(() => set = true);

            Assert.AreEqual(DispatcherTaskState.Queued, task.TaskState);

            task.Execute(false);

            Assert.IsTrue(set);
            Assert.AreEqual(DispatcherTaskState.CompletedQueued, task.TaskState);
        }

        [TestMethod]
        public void TestExecuteException()
        {
            Exception exception = new Exception();
            ActionDispatcherTask task = ActionDispatcherTask.Create(() => throw exception);

            Assert.AreEqual(DispatcherTaskState.Queued, task.TaskState);

            Assert.ThrowsException<DispatcherException>(() =>
            {
                task.Execute(true);
            });

            Assert.AreEqual(DispatcherTaskState.Failed, task.TaskState);
            Assert.AreEqual(exception, task.Exception);
        }
    }
}
