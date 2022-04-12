/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Text.RegularExpressions;

namespace DarkRift.Server.Metrics
{
#if PRO
    /// <summary>
    /// Builder for tagged metrics.
    /// </summary>
    /// <typeparam name="T">The type of metric being produced.</typeparam>
    /// <remarks>
    ///     Pro only.
    /// </remarks>
    public class TaggedMetricBuilder<T>
    {
        /// <summary>
        /// The number of tags expected to be provided.
        /// </summary>
        private readonly int noTagsExpected;

        /// <summary>
        /// A function that will produce a metric from the given tags.
        /// </summary>
        private readonly Func<string[], T> producer;

        /// <summary>
        /// Creates a new TaggedMetricBuilder.
        /// </summary>
        /// <param name="noTagsExpected">The number of tags expected to be provided.</param>
        /// <param name="producer">A function that will produce a metric from the given values.</param>
        public TaggedMetricBuilder(int noTagsExpected, Func<string[], T> producer)
        {
            this.noTagsExpected = noTagsExpected;
            this.producer = producer;
        }

        /// <summary>
        /// Creates a metric with the given tag values.
        /// </summary>
        /// <param name="values">The values of the tags to set.</param>
        /// <returns>The created metric.</returns>
        /// <exception cref="ArgumentException">The the number of values does not match the number the metric was created with.</exception>
        /// <remarks>
        /// The number of values passed in here must match the number of arguments the metric was initialized
        /// with else an <see cref="ArgumentException"/> will be thrown.
        /// </remarks>
        public T WithTags(params string[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values", "Tag values must not be null, use an empty array instead.");

            foreach (string tag in values)
            {
                if (!Regex.IsMatch(tag, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
                    throw new ArgumentException($@"Tag value '{tag}' must match regex '^[a-zA-Z_][a-zA-Z0-9_]*'. For example: 'tag_value_here'.");
            }

            if (values.Length != noTagsExpected)
                throw new ArgumentException($"Not enough tag values were provided to match the number of tags initialized. Expected {noTagsExpected} but only {values.Length} were provided.");

            return producer.Invoke(values);
        }
    }
#endif
}
