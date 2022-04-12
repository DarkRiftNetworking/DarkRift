/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading;
using DarkRift.Dispatching;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DarkRift.Testing.Dispatching
{
    [TestClass]
    public class DispatcherTests
    {
        private Dispatcher dispatcher;

        [TestInitialize]
        public void Initialize()
        {

        }

        [TestMethod]
        public void InvokeAsyncTests()
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
            Thread t = new Thread(() =>
            {
                task = dispatcher.InvokeAsync(() => { executed++; return; });
            });
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

        [TestMethod]
        public void InvokeAsyncFuncTests()
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
            Thread t = new Thread(() =>
            {
                task = dispatcher.InvokeAsync(() => ++executed);
            });
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

        [TestMethod]
        public void ExecuteDispatcherTasksTests()
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

        [TestMethod]
        public void ExecuteDispatcherTasksExceptionsTests()
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

            Assert.ThrowsException<DispatcherException>(() =>
            {
                dispatcher.ExecuteDispatcherTasks();
            });

            Assert.AreEqual(1, executed);
        }
    }
}
