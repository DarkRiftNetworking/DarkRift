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
    public class FunctionDispatcherTask<T> : DispatcherTask
    {
        /// <summary>
        ///     The function to execute.
        /// </summary>
        private readonly Func<T> function;

        /// <summary>
        ///     The callback to invoke once the task has been executed.
        /// </summary>
        private readonly FunctionDispatchCompleteCallback<T> callback;

        /// <summary>
        ///     The value returned from the function.
        /// </summary>
        public T Result { get; set; }

        internal FunctionDispatcherTask(Func<T> function) : this(function, null)
        {
        }

        internal FunctionDispatcherTask(Func<T> function, FunctionDispatchCompleteCallback<T> callback) : base()
        {
            this.function = function;
            this.callback = callback;
        }

        /// <summary>
        ///     Executes the function.
        /// </summary>
        /// <param name="synchronous">Was this called synchronously?</param>
        internal override void Execute(bool synchronous)
        {
            try
            {
                Result = function();
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
    }
}
