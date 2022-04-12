/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DarkRift.SystemTesting
{
    internal static class WaitUtility
    {
        /// <summary>
        ///     Waits until the given predicate returns true, failing if it does not return true within 10 seconds.
        /// </summary>
        /// <param name="message">The failure message to assert.</param>
        /// <param name="predicate">The predicate to wait for.</param>
        internal static void WaitUntil(string message, Func<bool> predicate)
        {
            WaitUntil(message, predicate, TimeSpan.FromSeconds(10));
        }

        /// <summary>
        ///     Waits until the given predicate returns true, failing if it does not return true within the timeout.
        /// </summary>
        /// <param name="message">The failure message to assert.</param>
        /// <param name="predicate">The predicate to wait for.</param>
        /// <param name="timeout">The maximum time to wait.</param>
        internal static void WaitUntil(string message, Func<bool> predicate, TimeSpan timeout)
        {
            DateTime failAt = DateTime.Now.Add(timeout);
            do
            {
                if (predicate.Invoke())
                    return;

                Thread.Sleep(100);
            }
            while (DateTime.Now < failAt);

            Assert.Fail(message);
        }
        
        /// <summary>
        ///     Waits until the given assertion passes, failing if it does not pass within 10 seconds.
        /// </summary>
        /// <param name="message">The failure message to assert.</param>
        /// <param name="assertion">The assertion function to wait for.</param>
        internal static void WaitUntil(string message, Action assertion)
        {
            WaitUntil(message, assertion, TimeSpan.FromSeconds(10));
        }

        /// <summary>
        ///     Waits until the given assertion passes, failing if it does not pass within the timeout.
        /// </summary>
        /// <param name="message">The failure message to assert.</param>
        /// <param name="assertion">The assertion function to wait for.</param>
        /// <param name="timeout">The maximum time to wait.</param>
        internal static void WaitUntil(string message, Action assertion, TimeSpan timeout)
        {
            DateTime failAt = DateTime.Now.Add(timeout);
            Exception lastException = null;
            do
            {
                try
                {
                    assertion.Invoke();
                    return;
                }
                catch (AssertFailedException e)
                {
                    lastException = e;
                }
                    
                Thread.Sleep(100);
            }
            while (DateTime.Now < failAt);

            Assert.Fail(message + "\nLast failure was:\n" + lastException);
        }

        /// <summary>
        ///     Waits until an element is dequeued from a queue, failing if it does not return a value within 10 seconds.
        /// </summary>
        /// <typeparam name="T">The type of element to dequeue.</typeparam>
        /// <param name="queue">The queue to dequeue from.</param>
        /// <param name="message">The failure message to assert.</param>
        /// <returns>The element dequeued.</returns>
        internal static T DequeueFrom<T>(ConcurrentQueue<T> queue, string message)
        {
            return DequeueFrom(queue, message, TimeSpan.FromSeconds(10));
        }

        /// <summary>
        ///     Waits until an element is dequeued from a queue, failing if it does not return a value within the timeout.
        /// </summary>
        /// <typeparam name="T">The type of element to dequeue.</typeparam>
        /// <param name="queue">The queue to dequeue from.</param>
        /// <param name="message">The failure message to assert.</param>
        /// <param name="timeout">The maximum time to wait.</param>
        /// <returns>The element dequeued.</returns>
        internal static T DequeueFrom<T>(ConcurrentQueue<T> queue, string message, TimeSpan timeout)
        {
            T result = default;

            WaitUntil(message, () => queue.TryDequeue(out result), timeout);

            return result;
        }
    }
}
