/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace DarkRift.Dispatching
{
    /// <summary>
    ///     Delegate used when a dispatch call has completed.
    /// </summary>
    /// <param name="task">The task that was completed.</param>
    public delegate void ActionDispatchCompleteCallback(ActionDispatcherTask task);
}
