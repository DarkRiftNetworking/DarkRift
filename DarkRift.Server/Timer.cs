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
#if PRO
    /// <summary>
    ///     A timer for delaying execution or one-shot or repetative tasks.
    /// </summary>
    /// <remarks>
    ///     <c>Pro only.</c> 
    /// </remarks>
    public sealed class Timer : IDisposable
    {
        /// <summary>
        ///     Whether the timer is a one-shot timer.
        /// </summary>
        public bool IsOneShot { get; }

        /// <summary>
        ///     The callback to invoke when the timer completes.
        /// </summary>
        public Action<Timer> Callback { get; }

        /// <summary>
        ///     The initial delay set on the timer.
        /// </summary>
        public int IntialDelay { get; }

        /// <summary>
        ///     The repetition period set on the timer.
        /// </summary>
        public int RepetitionPeriod{ get; }

        /// <summary>
        ///     The backing timer.
        /// </summary>
        private readonly System.Threading.Timer timer;

        /// <summary>
        ///     The thread helper to use.
        /// </summary>
        private readonly DarkRiftThreadHelper threadHelper;

        /// <summary>
        ///     Creates a new timer that will invoke the callback a single time.
        /// </summary>
        /// <param name="threadHelper">The thread helper to use.</param>
        /// <param name="delay">The delay in milliseconds before invoking the callback.</param>
        /// <param name="callback">The callback to invoke.</param>
        internal Timer(DarkRiftThreadHelper threadHelper, int delay, Action<Timer> callback)
        {
            this.threadHelper = threadHelper;
            this.IsOneShot = true;
            this.Callback = callback;
            this.IntialDelay = delay;

            timer = new System.Threading.Timer(InvokeCallback, null, delay, Timeout.Infinite);
        }

        /// <summary>
        ///     Creates a new timer that will invoke the callback repeatedly until stopped.
        /// </summary>
        /// <param name="threadHelper">The thread helper to use.</param>
        /// <param name="initialDelay">The delay in milliseconds before invoking the callback the first time.</param>
        /// <param name="repetitionPeriod">The delay in milliseconds between future invocations.</param>
        /// <param name="callback">The callback to invoke.</param>
        internal Timer(DarkRiftThreadHelper threadHelper, int initialDelay, int repetitionPeriod, Action<Timer> callback)
        {
            this.threadHelper = threadHelper;
            this.IsOneShot = false;
            this.Callback = callback;
            this.IntialDelay = initialDelay;
            this.RepetitionPeriod = repetitionPeriod;

            timer = new System.Threading.Timer(InvokeCallback, null, initialDelay, repetitionPeriod);
        }

        /// <summary>
        ///     Invokes the callback.
        /// </summary>
        /// <param name="_">Unused.</param>
        private void InvokeCallback(object _)
        {
            void DoInvoke()
            {
                Callback.Invoke(this);
            }

            threadHelper.DispatchIfNeeded(DoInvoke);
        }

    #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    timer.Dispose();
                }

                disposedValue = true;
            }
        }
        
        /// <summary>
        ///     Disposes of the timer and cancels all future invocations.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    #endregion
    }
#endif
}
