/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading;
using NUnit.Framework;

namespace DarkRift.Dispatching.Tests
{
    public class DispatcherTests
    {
        private Dispatcher dispatcher;

        [SetUp]
        public void SetUp()
        {
            //Object cache needs to be initialized for create
#pragma warning disable CS0618      // We don't care about using Server/Client specific cache settings
            ObjectCache.Initialize(ObjectCacheSettings.DontUseCache);
#pragma warning restore CS0618
        }

        [Test]
        public void InvokeAsync()
        {
            dispatcher = new Dispatcher(true);
            int executed = 0;

            Assert.AreEqual(0, dispatcher.Count);

            //Should invoke synchronously
            ActionDispatcherTask task = dispatcher.InvokeAsync(() => { executed++; return; });

            Assert.AreEqual(0, dispatcher.Count);
            Assert.AreEqual(1, executed);
            Assert.AreEqual(DispatcherTaskState.CompletedImmediate, task.TaskState);

            //Should invoke asynchronously
            Thread t = new Thread(() => task = dispatcher.InvokeAsync(() => { executed++; return; }));
            t.Start();
            t.Join();

            Assert.AreEqual(1, dispatcher.Count);
            Assert.AreEqual(1, executed);
            Assert.AreEqual(DispatcherTaskState.Queued, task.TaskState);

            dispatcher.ExecuteDispatcherTasks();

            Assert.AreEqual(0, dispatcher.Count);
            Assert.AreEqual(2, executed);
            Assert.AreEqual(DispatcherTaskState.CompletedQueued, task.TaskState);
        }

        [Test]
        public void InvokeAsyncFunc()
        {
            dispatcher = new Dispatcher(true);
            int executed = 0;

            Assert.AreEqual(0, dispatcher.Count);

            //Should invoke synchronously
            FunctionDispatcherTask<int> task = dispatcher.InvokeAsync(() => ++executed);

            Assert.AreEqual(0, dispatcher.Count);
            Assert.AreEqual(1, executed);
            Assert.AreEqual(1, task.Result);
            Assert.AreEqual(DispatcherTaskState.CompletedImmediate, task.TaskState);

            //Should invoke asynchronously
            Thread t = new Thread(() => task = dispatcher.InvokeAsync(() => ++executed));
            t.Start();
            t.Join();

            Assert.AreEqual(1, dispatcher.Count);
            Assert.AreEqual(1, executed);
            Assert.AreEqual(DispatcherTaskState.Queued, task.TaskState);

            dispatcher.ExecuteDispatcherTasks();

            Assert.AreEqual(0, dispatcher.Count);
            Assert.AreEqual(2, executed);
            Assert.AreEqual(2, task.Result);
            Assert.AreEqual(DispatcherTaskState.CompletedQueued, task.TaskState);
        }

        [Test]
        public void ExecuteDispatcherTasks()
        {
            dispatcher = new Dispatcher(false);

            //Should execute all current tasks, exception suppressed
            int executed = 0;
            Thread t = new Thread(() =>
            {
                dispatcher.InvokeAsync(() => executed++);
                dispatcher.InvokeAsync(() => executed++);
                dispatcher.InvokeAsync(() => executed++);
                dispatcher.InvokeAsync(() => throw new Exception());
            });
            t.Start();
            t.Join();

            dispatcher.ExecuteDispatcherTasks();

            Assert.AreEqual(3, executed);
        }

        [Test]
        public void ExecuteDispatcherTasksExceptions()
        {
            dispatcher = new Dispatcher(true);

            //Should execute 1st task then raise second task exception
            int executed = 0;
            Thread t = new Thread(() =>
            {
                dispatcher.InvokeAsync(() => executed++);
                dispatcher.InvokeAsync(() => throw new DivideByZeroException());
                dispatcher.InvokeAsync(() => executed++);
            });
            t.Start();
            t.Join();

            Assert.Throws<DispatcherException>(() => dispatcher.ExecuteDispatcherTasks());

            Assert.AreEqual(1, executed);
        }
    }
}
