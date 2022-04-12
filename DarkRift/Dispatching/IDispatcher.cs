/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkRift.Dispatching
{
    /// <summary>
    ///     Interface for the Dispatcher.
    /// </summary>
    public interface IDispatcher
    {
        /// <summary>
        ///     The number of items waiting to be dispatched.
        /// </summary>
        int Count { get; }

        /// <summary>
        ///     Queues the operation for execution on the main thread and waits until it has completed.
        /// </summary>
        /// <param name="action">The operation to execute.</param>
        void InvokeWait(Action action);

        /// <summary>
        ///     Queues the operation for execution on the main thread and waits until it has completed
        /// </summary>
        /// <typeparam name="T">The result of the function</typeparam>
        /// <param name="function">The function to execute.</param>
        /// <returns>The result of the function executed.</returns>
        T InvokeWait<T>(Func<T> function);

        /// <summary>
        ///     Queues the operation for execution on the main thread.
        /// </summary>
        /// <param name="action">The operation to execute.</param>
        /// <returns>A DispatcherTask for this operation.</returns>
        /// <remarks>This returns an IDisposable object, it is your responsibility to dispose of it when you're done!</remarks>
        ActionDispatcherTask InvokeAsync(Action action);

        /// <summary>
        ///     Queues the operation for execution on the main thread.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="function">The function to execute.</param>
        /// <returns>A DispatcherTask for this operation.</returns>
        /// <remarks>This returns an IDisposable object, it is your responsibility to dispose of it when you're done!</remarks>
        FunctionDispatcherTask<T> InvokeAsync<T>(Func<T> function);

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
        ActionDispatcherTask InvokeAsync(Action action, ActionDispatchCompleteCallback callback);

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
        FunctionDispatcherTask<T> InvokeAsync<T>(Func<T> function, FunctionDispatchCompleteCallback<T> callback);
    }
}
