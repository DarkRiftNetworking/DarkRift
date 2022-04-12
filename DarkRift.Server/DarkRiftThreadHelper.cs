/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Dispatching;
using System;
using System.Threading;

namespace DarkRift.Server
{
    /// <summary>
    ///     Thread helper class for DarkRift.
    /// </summary>
    public class DarkRiftThreadHelper
    {
        /// <summary>
        ///     Whether events are executed through the dispatcher or not.
        /// </summary>
        // TODO DR3 this should not be publicly settable!
        public bool EventsFromDispatcher { get; set; }

        /// <summary>
        ///     The dispatcher for the server.
        /// </summary>
        // TODO DR3 this should not be publicly settable!
        public IDispatcher Dispatcher { get; set; }

        /// <summary>
        ///     Creates a new thread helper with the given invocation settings.
        /// </summary>
        /// <param name="eventsFromDispatcher">Whether events should be invoked from the dispatcher.</param>
        /// <param name="dispatcher">The dispatcher to use.</param>
        public DarkRiftThreadHelper(bool eventsFromDispatcher, Dispatcher dispatcher)
        {
            EventsFromDispatcher = eventsFromDispatcher;
            Dispatcher = dispatcher;
        }

        /// <summary>
        ///     Helper method to run code from the dispatcher if <see cref="EventsFromDispatcher"/> is set.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        public void DispatchIfNeeded(Action action)
        {
            if (EventsFromDispatcher)
                Dispatcher.InvokeAsync(action);
            else
                action.Invoke();
        }

        /// <summary>
        ///     Helper method to run code from the dispatcher if <see cref="EventsFromDispatcher"/> is set.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="callback">The callback to invoke once this is complete.</param>
        /// <remarks>If the task was run synchronously then the argument to callback will be null as no task was created.</remarks>
        public void DispatchIfNeeded(Action action, ActionDispatchCompleteCallback callback)
        {
            if (EventsFromDispatcher)
            {
                Dispatcher.InvokeAsync(action, callback);
            }
            else
            {
                try
                {
                    action.Invoke();
                }
                finally
                {
                    callback.Invoke(null);
                }
            }
        }

        /// <summary>
        ///     Exponentially backs off a task
        /// </summary>
        /// <param name="action">The action to try to perform.</param>
        /// <param name="retries">The maximum number of retries to allow.</param>
        /// <param name="failureCallback">The callback to invoke if retries are exhausted, with the last exception thrown.</param>
        // TODO DR3 release publically with DR3, see if we can use Task/TaskScheduler in any way to improve the methods here
        internal void ExponentialBackoff(Action<ExponentialBackoffContext> action, int retries, Action<Exception> failureCallback)
        {
            var context = new ExponentialBackoffContext(retries, 2000);

            void Callback(object _)
            {
                context.Tries++;

                try
                {
                    action.Invoke(context);
                }
                catch (Exception e)
                {
                    if (context.Tries >= retries)
                    {
                        failureCallback.Invoke(e);
                    }
                    else
                    {
                        new System.Threading.Timer(Callback, null, context.BaseDelay * (2 ^ (context.Tries - 1)), -1);
                    }
                }
            }

            ThreadPool.QueueUserWorkItem(Callback, null);
        }

#if PRO
        /// <summary>
        ///     Creates a new timer that will invoke the callback a single time.
        /// </summary>
        /// <param name="delay">The delay in milliseconds before invoking the callback.</param>
        /// <param name="callback">The callback to invoke.</param>
        /// <returns>The new timer.</returns>
        internal Timer RunAfterDelay(int delay, Action<Timer> callback)
        {
            return new Timer(this, delay, callback);
        }

        /// <summary>
        ///     Creates a new timer that will invoke the callback repeatedly until stopped.
        /// </summary>
        /// <param name="initialDelay">The delay in milliseconds before invoking the callback the first time.</param>
        /// <param name="repetitionPeriod">The delay in milliseconds between future invocations.</param>
        /// <param name="callback">The callback to invoke.</param>
        /// <returns>The new timer.</returns>
        internal Timer CreateTimer(int initialDelay, int repetitionPeriod, Action<Timer> callback)
        {
            return new Timer(this, initialDelay, repetitionPeriod, callback);
        }
#endif

        /// <summary>
        ///     Class containing contextual information for an exponential backoff.
        /// </summary>
        internal class ExponentialBackoffContext
        {
            /// <summary>
            ///     The number of tries attempted so far.
            /// </summary>
            public int Tries { get; internal set; }

            /// <summary>
            ///     The maximum number of retries that will be attempted.
            /// </summary>
            public int MaxRetries { get; }

            /// <summary>
            ///     The delay the exponential backoff was started with.
            /// </summary>
            public int BaseDelay { get; }

            /// <summary>
            ///     Creates a new ExponentialBackoffContext.
            /// </summary>
            /// <param name="maxRetries">The maximum number of retries that will be performed.</param>
            /// <param name="baseDelay">The delay to apply of first failure.</param>
            internal ExponentialBackoffContext(int maxRetries, int baseDelay)
            {
                Tries = 0;
                MaxRetries = maxRetries;
                BaseDelay = baseDelay;
            }
        }
    }
}
