/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DarkRift.DataStructures
{
    /// <summary>
    ///     A simple moving average filter for smoothing changing values.
    /// </summary>
    internal class MovingAverageFilter
    {
        /// <summary>
        ///     The average of the samples.
        /// </summary>
        public float Average { get; private set; }

        /// <summary>
        ///     The samples recorded.
        /// </summary>
        private float[] samples;

        /// <summary>
        ///     The next sample to overwrite.
        /// </summary>
        private int head = 0;

        /// <summary>
        ///     Creates a new moving average filter.
        /// </summary>
        /// <param name="size">The number of past samples to keep.</param>
        public MovingAverageFilter(int size)
        {
            samples = new float[size];
        }

        /// <summary>
        ///     Adds a new sample to the filter.
        /// </summary>
        /// <param name="sample">The new sample value.</param>
        public void Add(float sample)
        {
            lock (samples)
            {
                sample = sample / samples.Length;

                Average = Average - samples[head] + sample;

                samples[head] = sample;

                head = (head + 1) % samples.Length;
            }
        }

        /// <summary>
        ///     Resets the filter and clears all history.
        /// </summary>
        public void Reset()
        {
            lock (samples)
            {
                for (int i = 0; i < samples.Length; i++)
                    samples[i] = 0;

                head = 0;
                Average = 0;
            }
        }
    }
}
