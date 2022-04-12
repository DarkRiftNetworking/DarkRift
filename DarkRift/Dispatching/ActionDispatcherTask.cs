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
    ///     A <see cref="DispatcherTask"/> that has no return value.
    /// </summary>
    public class ActionDispatcherTask : DispatcherTask
    {
        /// <summary>
        ///     The action to execute.
        /// </summary>
        private Action action;

        /// <summary>
        ///     The callback to invoke once the task has been executed.
        /// </summary>
        private ActionDispatchCompleteCallback callback;

        /// <summary>
        ///     Whether this task is currently in an object pool waiting or not.
        /// </summary>
        private volatile bool isCurrentlyLoungingInAPool;

        /// <summary>
        ///     Creates an ActionDispatcherTask.
        /// </summary>
        internal ActionDispatcherTask() : base()
        {

        }

        /// <summary>
        ///     Creates a new action dispatcher task.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        internal static ActionDispatcherTask Create(Action action)
        {
            return Create(action, null);
        }

        /// <summary>
        ///     Creates a new action dispatcher task.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="callback">The callback to run when this task has been executed.</param>
        internal static ActionDispatcherTask Create(Action action, ActionDispatchCompleteCallback callback)
        {
            ActionDispatcherTask task = ObjectCache.GetActionDispatcherTask();

            task.isCurrentlyLoungingInAPool = false;

            task.action = action;
            task.callback = callback;

            return task;
        }

        /// <summary>
        ///     Executes the action.
        /// </summary>
        /// <param name="synchronous">Was this called synchronously?</param>
        internal override void Execute(bool synchronous)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                SetTaskFailed(e);
                throw new DispatcherException("An exception occurred whilst running a dispatcher task. See inner exception for more details.", e);
            }

            SetTaskComplete(synchronous);

            if (callback != null)
            {
                void RunCallback(object _)
                {
                    callback(this);
                }

                if (synchronous)
                    RunCallback(null);
                else
                    ThreadPool.QueueUserWorkItem(RunCallback);
            }
        }
        
        /// <summary>
        ///     Actually disposes of the instance rather than recycling it.
        /// </summary>
        internal void ActuallyDispose()
        {
            base.Dispose(true);
        }

        /// <summary>
        ///     Returns the instance to the object cache.
        /// </summary>
        /// <param name="disposing">If the object is being disposed.</param>
        protected override void Dispose(bool disposing)
        {
            //Intercepts default dispose to recycle the instance!

            ObjectCache.ReturnActionDispatcherTask(this);
            isCurrentlyLoungingInAPool = true;
        }

        /// <summary>
        ///     Finalizer so we can inform the cache system we were not recycled correctly.
        /// </summary>
        ~ActionDispatcherTask()
        {
            if (!isCurrentlyLoungingInAPool)
                ObjectCacheHelper.ActionDispatcherTaskWasFinalized();
        }
    }
}
