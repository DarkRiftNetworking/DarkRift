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
    ///     The possible states that a <see cref="DispatcherTask"/>can be in.
    /// </summary>
    public enum DispatcherTaskState
    {
        /// <summary>
        ///     The task is queued for execution in the dispatcher but not yet run.
        /// </summary>
        Queued,

        /// <summary>
        ///     The task was queued and has been completed.
        /// </summary>
        CompletedQueued,

        /// <summary>
        ///     The task was completed without being queued (i.e. you were already on the execution thread)
        /// </summary>
        CompletedImmediate,

        /// <summary>
        ///     The task was run but threw an exception.
        /// </summary>
        Failed
    }
}
