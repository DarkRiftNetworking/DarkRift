/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Server.Metrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkRift.Server.Plugins.Performance
{
    /// <summary>
    ///     Plugin that monitors the object cache and logs of any problems.
    /// </summary>
    internal class ObjectCacheMonitor : Plugin
    {
        public override bool ThreadSafe => true;

        public override Version Version => new Version(1, 0, 0);

        internal override bool Hidden => true;

        /// <summary>
        ///     The number of miliseconds between checks.
        /// </summary>
        private readonly int period = 10000;

        /// <summary>
        ///     Timer to trigger performance checks.
        /// </summary>
        private readonly System.Threading.Timer timer;

        /// <summary>
        ///     The number of <see cref="DarkRiftReader"/> objects that were last not recycled properly.
        /// </summary>
        private int lastFinalizedDarkRiftReaders = 0;

        /// <summary>
        ///     The number of <see cref="DarkRiftWriter"/> objects that were last not recycled properly.
        /// </summary>
        private int lastFinalizedDarkRiftWriters = 0;

        /// <summary>
        ///     The number of <see cref="Message"/> objects that were last not recycled properly.
        /// </summary>
        private int lastFinalizedMessages = 0;

        /// <summary>
        ///     The number of <see cref="MessageBuffer"/> objects that were last not recycled properly.
        /// </summary>
        private int lastFinalizedMessageBuffers = 0;

        /// <summary>
        ///     The number of <see cref="MessageReceivedEventArgs"/> objects that were last not recycled properly.
        /// </summary>
        private int lastFinalizedMessageReceivedEventArgs = 0;

        /// <summary>
        ///     The number of <see cref="AutoRecyclingArray"/> objects that were last not recycled properly.
        /// </summary>
        private int lastFinalizedAutoRecyclingArrays = 0;

#if PRO
        /// <summary>
        /// Counter metric for the number of finalizations that have occured.
        /// </summary>
        private readonly TaggedMetricBuilder<ICounterMetric> finalizationsCounter;
#endif
        public ObjectCacheMonitor(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            string periodSetting = pluginLoadData.Settings["period"];
            if (periodSetting != null)
            {
                if (!int.TryParse(periodSetting, out period))
                    Logger.Error("'period' setting was not a valid number. Using the default of " + period + "ms.");
            }

            timer = new System.Threading.Timer(CheckObjectCache);

#if PRO
            finalizationsCounter = MetricsCollector.Counter("finalizations", "The number of recyclable objects that have been finalized.", "type");
#endif
        }

#if PRO
        protected
#endif
            internal override void Loaded(LoadedEventArgs args)
        {
            base.Loaded(args);

            timer.Change(period, period);
        }

        /// <summary>
        ///     Performs a check on the object cache.
        /// </summary>
        /// <param name="state">The timer state.</param>
        private void CheckObjectCache(object state)
        {
#if DEBUG
            // Collect GC in debug mode so the warnings are more up to date
            GC.Collect();
#endif
            int deltaAutoRecyclingArrays = ObjectCacheHelper.FinalizedAutoRecyclingArrays - lastFinalizedAutoRecyclingArrays;
            if (deltaAutoRecyclingArrays > 0)
                Logger.Warning(deltaAutoRecyclingArrays + " AutoRecyclingArray objects were finalized last period. This is usually a sign that you are not recycling objects correctly.");
#if PRO
            finalizationsCounter.WithTags("auto_recycling_array").Increment(deltaAutoRecyclingArrays);
#endif

            lastFinalizedAutoRecyclingArrays = ObjectCacheHelper.FinalizedAutoRecyclingArrays;

            int deltaDarkRiftReaders = ObjectCacheHelper.FinalizedDarkRiftReaders - lastFinalizedDarkRiftReaders;
            if (deltaDarkRiftReaders > 0)
                Logger.Warning(deltaDarkRiftReaders + " DarkRiftReader objects were finalized last period. This is usually a sign that you are not recycling objects correctly.");
#if PRO
            finalizationsCounter.WithTags("darkrift_reader").Increment(deltaDarkRiftReaders);
#endif

            lastFinalizedDarkRiftReaders = ObjectCacheHelper.FinalizedDarkRiftReaders;

            int deltaDarkRiftWriters = ObjectCacheHelper.FinalizedDarkRiftWriters - lastFinalizedDarkRiftWriters;
            if (deltaDarkRiftWriters > 0)
                Logger.Warning(deltaDarkRiftWriters + " DarkRiftWriter objects were finalized last period. This is usually a sign that you are not recycling objects correctly.");
#if PRO
            finalizationsCounter.WithTags("darkrift_writer").Increment(deltaDarkRiftWriters);
#endif

            lastFinalizedDarkRiftWriters = ObjectCacheHelper.FinalizedDarkRiftWriters;

            int deltaMessages = ObjectCacheHelper.FinalizedMessages - lastFinalizedMessages;
            if (deltaMessages > 0)
                Logger.Warning(deltaMessages + " Message objects were finalized last period. This is usually a sign that you are not recycling objects correctly.");
#if PRO
            finalizationsCounter.WithTags("message").Increment(deltaMessages);
#endif

            lastFinalizedMessages = ObjectCacheHelper.FinalizedMessages;

            int deltaMessageBuffers = ObjectCacheHelper.FinalizedMessageBuffers - lastFinalizedMessageBuffers;
            if (deltaMessageBuffers > 0)
                Logger.Warning(deltaMessageBuffers + " MessageBuffer objects were finalized last period. This is usually a sign that you are not recycling objects correctly.");
#if PRO
            finalizationsCounter.WithTags("message_buffer").Increment(deltaMessageBuffers);
#endif

            lastFinalizedMessageBuffers = ObjectCacheHelper.FinalizedMessageBuffers;

            int deltaMessageReceivedEventArgs = ServerObjectCacheHelper.FinalizedMessageReceivedEventArgs - lastFinalizedMessageReceivedEventArgs;
            if (deltaMessageReceivedEventArgs > 0)
                Logger.Warning(deltaMessageReceivedEventArgs + " MessageReceivedEventArgs objects were finalized last period. This is usually a sign that you are not recycling objects correctly.");
#if PRO
            finalizationsCounter.WithTags("message_received_event_args").Increment(deltaMessageReceivedEventArgs);
#endif

            lastFinalizedMessageReceivedEventArgs = ServerObjectCacheHelper.FinalizedMessageReceivedEventArgs;
        }
    }
}
