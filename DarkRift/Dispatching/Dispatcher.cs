/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DarkRift.Dispatching
{
    /// <summary>
    ///     Dispatcher for running tasks on the main thread.
    /// </summary>
    /// <remarks>
    ///     The dispatcher is used by DarkRift to move code to the main thread to be executed, either for convienience for programmers that do not know 
    ///     how to use multithreading safely or for use on frameworks where execution across multiple threads is not allowed. Similarly other DarkRift 
    ///     code can also make use of the dispatcher if they require threads to merge execution.
    /// </remarks>
    public sealed class Dispatcher : IDispatcher, IDisposable
    {
        /// <summary>
        ///     A wait handle for new dispatcher jobs.
        /// </summary>
        /// <remarks>
        ///     This is released when a new task is assigned to the dispatcher and so can be used to hold the task executing 
        ///     thread idle until a job is posted rather than constantly consuming resources.
        /// </remarks>
        public WaitHandle WaitHandle => jobMutex;

        /// <summary>
        ///     The tasks that need executing.
        /// </summary>
        private Queue<DispatcherTask> tasks = new Queue<DispatcherTask>();

        /// <summary>
        ///     The mutex for new jobs in the queue.
        /// </summary>
        private ManualResetEvent jobMutex = new ManualResetEvent(false);

        /// <summary>
        ///     The ID of the thread that is executing tasks.
        /// </summary>
        private readonly int executorThreadID;

        /// <summary>
        ///     Whether exceptions should be raised on the executor thread.
        /// </summary>
        /// <remarks>
        ///     When set true, any unhandled exceptions that are rasised during the execution of a task will be unsurpressed and thrown on the thread 
        ///     executing the tasks. The exceptions thrown within task executions can still be retreived by accessing the task object.
        /// </remarks>
        public bool ExceptionsOnExecutorThread { get; }

        /// <summary>
        ///     The number of items waiting to be dispatched.
        /// </summary>
        public int Count
        {
            get
            {
                lock (tasks)
                    return tasks.Count;
            }
        }

        /// <summary>
        ///     Creates a new Dispatcher indicating whether exceptions should be thrown on the executing thread and setting the executor thread to the calling thread.
        /// </summary>
        /// <param name="exceptionsOnExecutorThread">Whether exceptions should be thrown from the executing thread.</param>
        public Dispatcher(bool exceptionsOnExecutorThread)
            : this(exceptionsOnExecutorThread, Thread.CurrentThread.ManagedThreadId)
        {

        }

        /// <summary>
        ///     Creates a new Dispatcher indicating whether exceptions should be thrown on the executing thread and specifying the executor thread.
        /// </summary>
        /// <remarks>
        ///     This overload allows the executor thread ID to be passed in so that a specific thread can be chosen. If the thread specified tried to enqueue a task 
        ///     it will immediately be processed synchronously to protect against deadlocks. No other threads can be used to call <see cref="ExecuteDispatcherTasks"/>.
        ///     To disable this functionaility pass -1 as the thread ID.
        /// </remarks>
        /// <param name="exceptionsOnExecutorThread">Whether exceptions should be thrown from the executing thread.</param>
        /// <param name="executorThreadID">The thread that will be dequeueing tasks.</param>
        public Dispatcher(bool exceptionsOnExecutorThread, int executorThreadID)
        {
            this.ExceptionsOnExecutorThread = exceptionsOnExecutorThread;
            this.executorThreadID = executorThreadID;
        }

        /// <summary>
        ///     Queues the operation for execution on the main thread and waits until it has completed.
        /// </summary>
        /// <remarks>
        ///     If an exception occurs during the processing of the action the event will be re-thrown by this function for you
        ///     to handle.
        /// </remarks>
        /// <param name="action">The operation to execute.</param>
        /// <exception cref="DispatcherException">Thrown if an unhandled exception was raised while executing the dispatcher task.</exception>
        public void InvokeWait(Action action)
        {
            //Invoke async and wait for it.
            using (DispatcherTask task = InvokeAsync(action))
            {
                task.WaitHandle.WaitOne();

                if (task.TaskState == DispatcherTaskState.Failed)
                    throw new DispatcherException("An unhandled exception was thrown inside the dispatcher task, see inner exception for more details.", task.Exception);
            }
        }

        /// <summary>
        ///     Queues the operation for execution on the main thread and waits until it has completed
        /// </summary>
        /// <remarks>
        ///     If an exception occurs during the processing of the function the event will be re-thrown by this function for you
        ///     to handle.
        /// </remarks>
        /// <typeparam name="T">The result of the function</typeparam>
        /// <param name="function">The function to execute.</param>
        /// <returns>The result of the function executed.</returns>
        /// <exception cref="DispatcherException">Thrown if an unhandled exception was raised while executing the dispatcher task.</exception>
        public T InvokeWait<T>(Func<T> function)
        {
            //Invoke async and wait for it.
            using (FunctionDispatcherTask<T> task = InvokeAsync(function))
            {
                task.WaitHandle.WaitOne();

                if (task.TaskState == DispatcherTaskState.Failed)
                    throw new DispatcherException("An unhandled exception was thrown inside the dispatcher task, see inner exception for more details.", task.Exception);
                else
                    return task.Result;
            }
        }

        /// <summary>
        ///     Queues the operation for execution on the main thread.
        /// </summary>
        /// <param name="action">The operation to execute.</param>
        /// <returns>A DispatcherTask for this operation.</returns>
        /// <remarks>
        ///     This returns an IDisposable object, it is your responsibility to dispose of it when you're done!
        /// </remarks>
        /// <exception cref="DispatcherException">Thrown if an unhandled exception was raised while executing the dispatcher task when completing synchronously.</exception>
        public ActionDispatcherTask InvokeAsync(Action action)
        {
            //Queue the operation and wait.
            ActionDispatcherTask task = ActionDispatcherTask.Create(action);

            //Complete synchronously if already executor thread
            if (Thread.CurrentThread.ManagedThreadId == executorThreadID)
            {
                task.Execute(true);
            }
            else
            {
                lock (tasks)
                    tasks.Enqueue(task);

                jobMutex.Set();
            }

            return task;
        }

        /// <summary>
        ///     Queues the operation for execution on the main thread.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="function">The function to execute.</param>
        /// <returns>A DispatcherTask for this operation.</returns>
        /// <remarks>
        ///     This returns an IDisposable object, it is your responsibility to dispose of it when you're done!
        /// </remarks>
        /// <exception cref="DispatcherException">Thrown if an unhandled exception was raised while executing the dispatcher task when completing synchronously.</exception>
        public FunctionDispatcherTask<T> InvokeAsync<T>(Func<T> function)
        {
            //Queue the operation and wait.
            FunctionDispatcherTask<T> task = new FunctionDispatcherTask<T>(function);

            //Complete synchronously if already executor thread
            if (Thread.CurrentThread.ManagedThreadId == executorThreadID)
            {
                task.Execute(true);
            }
            else
            {
                lock (tasks)
                    tasks.Enqueue(task);

                jobMutex.Set();
            }

            return task;
        }

        /// <summary>
        ///     Queues the operation for execution on the main thread.
        /// </summary>
        /// <param name="action">The operation to execute.</param>
        /// <param name="callback">The callback to invoke once this is complete.</param>
        /// <returns>A DispatcherTask for this operation.</returns>
        /// <remarks>
        ///     This returns an IDisposable object, it is your responsibility to dispose of it when you're done!
        /// </remarks>
        /// <exception cref="DispatcherException">Thrown if an unhandled exception was raised while executing the dispatcher task when completing synchronously.</exception>
        public ActionDispatcherTask InvokeAsync(Action action, ActionDispatchCompleteCallback callback)
        {
            //Queue the operation and wait.
            ActionDispatcherTask task = ActionDispatcherTask.Create(action, callback);

            //Complete synchronously if already executor thread
            if (Thread.CurrentThread.ManagedThreadId == executorThreadID)
            {
                task.Execute(true);
            }
            else
            {
                lock (tasks)
                    tasks.Enqueue(task);

                jobMutex.Set();
            }

            return task;
        }

        /// <summary>
        ///     Queues the operation for execution on the main thread.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="function">The function to execute.</param>
        /// <param name="callback">The callback to invoke once this is complete.</param>
        /// <returns>A DispatcherTask for this operation.</returns>
        /// <remarks>
        ///     This returns an IDisposable object, it is your responsibility to dispose of it when you're done!
        /// </remarks>
        /// <exception cref="DispatcherException">Thrown if an unhandled exception was raised while executing the dispatcher task when completing synchronously.</exception>
        public FunctionDispatcherTask<T> InvokeAsync<T>(Func<T> function, FunctionDispatchCompleteCallback<T> callback)
        {
            //Queue the operation and wait.
            FunctionDispatcherTask<T> task = new FunctionDispatcherTask<T>(function, callback);

            //Complete synchronously if already executor thread
            if (Thread.CurrentThread.ManagedThreadId == executorThreadID)
            {
                task.Execute(true);
            }
            else
            {
                lock (tasks)
                    tasks.Enqueue(task);

                jobMutex.Set();
            }

            return task;
        }

        /// <summary>
        ///     Executes all tasks queued for execution.
        /// </summary>
        /// <exception cref="DispatcherException">Thrown if an unhandled exception was raised while executing a dispatcher task.</exception>
        public void ExecuteDispatcherTasks()
        {
            if (executorThreadID != -1 && Thread.CurrentThread.ManagedThreadId != executorThreadID)
                throw new InvalidOperationException("Can only execute tasks from the thread that created the dispatcher.");
            
            //Get the number to execute
            int countAtStart = Count;
            
            //Execute that number of tasks
            for (int i = 0; i < countAtStart; i++)
            {
                //Get the task
                DispatcherTask task;
                lock (tasks)
                    task = tasks.Dequeue();

                //Execute the task
                try
                {
                    task.Execute(false);
                }
                catch
                {
                    //Raise exception on main thread if necessary, else suppress and pick up later
                    if (ExceptionsOnExecutorThread)
                        throw;
                }
            }

            //Reset job mutex if empty
            lock (tasks)
            {
                if (tasks.Count == 0)
                    jobMutex.Reset();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)        //TODO 1 dispose items in queue and possibly throw ObjectDisposedExceptions
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    WaitHandle.Close();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        ///     Disposes of the dispatcher object.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
