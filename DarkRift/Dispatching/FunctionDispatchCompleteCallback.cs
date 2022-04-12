/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace DarkRift.Dispatching
{
    /// <summary>
    ///     Delegate used when a dispatch call has completed with return value.
    /// </summary>
    /// <param name="task">The task that was completed.</param>
    /// <typeparam name="T">The type of the value being returned.</typeparam>
    public delegate void FunctionDispatchCompleteCallback<T>(FunctionDispatcherTask<T> task);
}
