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
    ///     Base class for all tasks on the dispatcher.
    /// </summary>
    /// <remarks>
    ///     Note that DispatcherTask is marked <see cref="IDisposable"/> so must be disposed of accordingly after use.
    /// </remarks>
    public abstract class DispatcherTask : IDisposable
    {
        /// <summary>
        ///     The wait handle that will be set when the operation completes.
        /// </summary>
        /// <remarks>
        ///     This can be used to pause another thread's execution until the task has completed on the main 
        ///     dispatcher's execution thread.
        /// </remarks>
        public WaitHandle WaitHandle => manualResetEvent;

        /// <summary>
        ///     The ManualResetEvent that will be set once executed.
        /// </summary>
        private ManualResetEvent manualResetEvent;

        /// <summary>
        ///     The state of this dispatcher task.
        /// </summary>
        public DispatcherTaskState TaskState { get; private set; }

        /// <summary>
        ///     The exception that occured while the event was being processed.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        ///     Creates a new DispatcherTask.
        /// </summary>
        internal DispatcherTask()
        {
            this.manualResetEvent = new ManualResetEvent(false);
            this.TaskState = DispatcherTaskState.Queued;
        }

        /// <summary>
        ///     Executes the task.
        /// </summary>
        /// <param name="invokedImmediate">Was this called immediately?</param>
        internal abstract void Execute(bool invokedImmediate);

        /// <summary>
        ///     Sets the wait handle for this task and updates the TaskState to completed.
        /// </summary>
        protected void SetTaskComplete(bool completedImmediate)
        {
            if (completedImmediate)
                TaskState = DispatcherTaskState.CompletedImmediate;
            else
                TaskState = DispatcherTaskState.CompletedQueued;

            manualResetEvent.Set();
        }

        /// <summary>
        ///     Sets the wait handle for this task and updates the task state to failed.
        /// </summary>
        /// <param name="e">The exception that cause the failure (if present).</param>
        protected void SetTaskFailed(Exception e)
        {
            Exception = e;
            TaskState = DispatcherTaskState.Failed;
            manualResetEvent.Set();
        }

        /// <summary>
        ///     Dispose of this connection.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        ///     Dispose of this connection.
        /// </summary>
        /// <param name="disposing">Are we disposing?</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                manualResetEvent.Close();
            }
        }
    }
}
