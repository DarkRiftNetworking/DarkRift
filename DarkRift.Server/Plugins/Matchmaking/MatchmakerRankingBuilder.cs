/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;

#if PRO
namespace DarkRift.Server.Plugins.Matchmaking
{
    /// <summary>
    ///     Helper class for generating the rankings used by the <see cref="RankingMatchmaker{T}"/>.
    /// </summary>
    /// <remarks>
    ///     This class can be used to help build up a matchmaking ranking for complex objects.
    ///     
    ///     For example, imagine we have 3 properties class, level, and highscore and we would 
    ///     like level to be the largest influence on ranking and prefer entities with different 
    ///     classes to be matched together. We can use a MinimiseDifference on both level and 
    ///     highscore to prefer entities with similar values and a NotEqual on our class type. 
    ///     To ensure level has the largest influence we might split our weighting values to 
    ///     0.25, 0.5, 0.25 respectively.
    ///     
    ///     It is important that the weighting of all comparisons totals 1. Without this the 
    ///     ranking value may exceed the 0-1 range that it should fall within and this will 
    ///     cause mistakes in the generated rankings.
    ///     
    ///     This is not thread safe.
    ///     
    ///     <c>Pro only.</c>
    /// </remarks>
    public class MatchmakerRankingBuilder
    {
        /// <summary>
        ///     The ranking between the two entities.
        /// </summary>
        public float Ranking
        {
            get
            {
                if (Failed)
                    return 0;
                else
                    return ranking;
            }
        }

        /// <summary>
        ///     The calculated ranking.
        /// </summary>
        private float ranking;

        /// <summary>
        ///     Indicates that <see cref="Fail"/> was called.
        /// </summary>
        public bool Failed { get; private set; }

        /// <summary>
        ///     Creates a new <see cref="MatchmakerRankingBuilder"/>.
        /// </summary>
        public MatchmakerRankingBuilder()
        {

        }

        /// <summary>
        ///     Instructs that a better ranking would have less defference between the two given value.
        /// </summary>
        /// <param name="a">The first entity's value.</param>
        /// <param name="b">The second entity's value.</param>
        /// <param name="maxDifference">The maximum difference possibel between the values. E.g. if the scale is -100 to +100, the maximum difference is 200.</param>
        /// <param name="weight">The weighting to give this comparison.</param>
        public void MinimiseDifferenceLinear(float a, float b, float maxDifference, float weight)
        {
            ranking += (1 - Math.Abs(a - b) / maxDifference) * weight;
        }

        /// <summary>
        ///     Instructs that a better ranking would have less defference between the two given value.
        /// </summary>
        /// <param name="a">The first entity's value.</param>
        /// <param name="b">The second entity's value.</param>
        /// <param name="maxDifference">The maximum difference possibel between the values. E.g. if the scale is -100 to +100, the maximum difference is 200.</param>
        /// <param name="weight">The weighting to give this comparison.</param>
        public void MinimiseDifferenceLinear(double a, double b, double maxDifference, float weight)
        {
            ranking += (float)((1 - Math.Abs(a - b) / maxDifference) * weight);
        }

        /// <summary>
        ///     Instructs that a better ranking would have less defference between the two given value.
        /// </summary>
        /// <param name="a">The first entity's value.</param>
        /// <param name="b">The second entity's value.</param>
        /// <param name="maxDifference">The maximum difference possibel between the values. E.g. if the scale is -100 to +100, the maximum difference is 200.</param>
        /// <param name="weight">The weighting to give this comparison.</param>
        public void MinimiseDifferenceLinear(int a, int b, int maxDifference, float weight)
        {
            ranking += (1 - (float)Math.Abs(a - b) / maxDifference) * weight;
        }

        /// <summary>
        ///     Instructs that a better ranking would have less defference between the two given value.
        /// </summary>
        /// <param name="a">The first entity's value.</param>
        /// <param name="b">The second entity's value.</param>
        /// <param name="maxDifference">The maximum difference possibel between the values. E.g. if the scale is -100 to +100, the maximum difference is 200.</param>
        /// <param name="weight">The weighting to give this comparison.</param>
        public void MinimiseDifferenceLinear(long a, long b, long maxDifference, float weight)
        {
            ranking += (1 - (float)Math.Abs(a - b) / maxDifference) * weight;
        }

        /// <summary>
        ///     Instructs that a better ranking would have more defference between the two given value.
        /// </summary>
        /// <param name="a">The first entity's value.</param>
        /// <param name="b">The second entity's value.</param>
        /// <param name="maxDifference">The maximum difference possibel between the values. E.g. if the scale is -100 to +100, the maximum difference is 200.</param>
        /// <param name="weight">The weighting to give this comparison.</param>
        public void MaximiseDifferenceLinear(float a, float b, float maxDifference, float weight)
        {
            ranking += Math.Abs(a - b) / maxDifference * weight;
        }

        /// <summary>
        ///     Instructs that a better ranking would have more defference between the two given value.
        /// </summary>
        /// <param name="a">The first entity's value.</param>
        /// <param name="b">The second entity's value.</param>
        /// <param name="maxDifference">The maximum difference possibel between the values. E.g. if the scale is -100 to +100, the maximum difference is 200.</param>
        /// <param name="weight">The weighting to give this comparison.</param>
        public void MaximiseDifferenceLinear(double a, double b, double maxDifference, float weight)
        {
            ranking += (float)(Math.Abs(a - b) / maxDifference * weight);
        }

        /// <summary>
        ///     Instructs that a better ranking would have more defference between the two given value.
        /// </summary>
        /// <param name="a">The first entity's value.</param>
        /// <param name="b">The second entity's value.</param>
        /// <param name="maxDifference">The maximum difference possibel between the values. E.g. if the scale is -100 to +100, the maximum difference is 200.</param>
        /// <param name="weight">The weighting to give this comparison.</param>
        public void MaximiseDifferenceLinear(int a, int b, int maxDifference, float weight)
        {
            ranking += (float)Math.Abs(a - b) / maxDifference * weight;
        }

        /// <summary>
        ///     Instructs that a better ranking would have more defference between the two given value.
        /// </summary>
        /// <param name="a">The first entity's value.</param>
        /// <param name="b">The second entity's value.</param>
        /// <param name="maxDifference">The maximum difference possibel between the values. E.g. if the scale is -100 to +100, the maximum difference is 200.</param>
        /// <param name="weight">The weighting to give this comparison.</param>
        public void MaximiseDifferenceLinear(long a, long b, long maxDifference, float weight)
        {
            ranking += (float)Math.Abs(a - b) / maxDifference * weight;
        }

        /// <summary>
        ///     Instructs that a better ranking would have the same value between the two given value.
        /// </summary>
        /// <param name="a">The first entity's value.</param>
        /// <param name="b">The second entity's value.</param>
        /// <param name="weight">The weighting to give this comparison.</param>
        public void Equal(float a, float b, float weight)
        {
            if (a == b)
                ranking += weight;
        }

        /// <summary>
        ///     Instructs that a better ranking would have the same value between the two given value.
        /// </summary>
        /// <param name="a">The first entity's value.</param>
        /// <param name="b">The second entity's value.</param>
        /// <param name="weight">The weighting to give this comparison.</param>
        public void Equal(double a, double b, float weight)
        {
            if (a == b)
                ranking += weight;
        }

        /// <summary>
        ///     Instructs that a better ranking would have the same value between the two given value.
        /// </summary>
        /// <param name="a">The first entity's value.</param>
        /// <param name="b">The second entity's value.</param>
        /// <param name="weight">The weighting to give this comparison.</param>
        public void Equal(int a, int b, float weight)
        {
            if (a == b)
                ranking += weight;
        }

        /// <summary>
        ///     Instructs that a better ranking would have the same value between the two given value.
        /// </summary>
        /// <param name="a">The first entity's value.</param>
        /// <param name="b">The second entity's value.</param>
        /// <param name="weight">The weighting to give this comparison.</param>
        public void Equal(long a, long b, float weight)
        {
            if (a == b)
                ranking += weight;
        }

        /// <summary>
        ///     Instructs that a better ranking would have the same value between the two given value.
        /// </summary>
        /// <param name="a">The first entity's value.</param>
        /// <param name="b">The second entity's value.</param>
        /// <param name="weight">The weighting to give this comparison.</param>
        public void Equal<T>(IEquatable<T> a, IEquatable<T> b, float weight)
        {
            if (a.Equals(b))
                ranking += weight;
        }

        /// <summary>
        ///     Instructs that a better ranking would have the difference values between the two given value.
        /// </summary>
        /// <param name="a">The first entity's value.</param>
        /// <param name="b">The second entity's value.</param>
        /// <param name="weight">The weighting to give this comparison.</param>
        public void NotEqual(float a, float b, float weight)
        {
            if (a != b)
                ranking += weight;
        }

        /// <summary>
        ///     Instructs that a better ranking would have the difference values between the two given value.
        /// </summary>
        /// <param name="a">The first entity's value.</param>
        /// <param name="b">The second entity's value.</param>
        /// <param name="weight">The weighting to give this comparison.</param>
        public void NotEqual(double a, double b, float weight)
        {
            if (a != b)
                ranking += weight;
        }

        /// <summary>
        ///     Instructs that a better ranking would have the difference values between the two given value.
        /// </summary>
        /// <param name="a">The first entity's value.</param>
        /// <param name="b">The second entity's value.</param>
        /// <param name="weight">The weighting to give this comparison.</param>
        public void NotEqual(int a, int b, float weight)
        {
            if (a != b)
                ranking += weight;
        }

        /// <summary>
        ///     Instructs that a better ranking would have the difference values between the two given value.
        /// </summary>
        /// <param name="a">The first entity's value.</param>
        /// <param name="b">The second entity's value.</param>
        /// <param name="weight">The weighting to give this comparison.</param>
        public void NotEqual(long a, long b, float weight)
        {
            if (a != b)
                ranking += weight;
        }

        /// <summary>
        ///     Instructs that a better ranking would have the same value between the two given value.
        /// </summary>
        /// <param name="a">The first entity's value.</param>
        /// <param name="b">The second entity's value.</param>
        /// <param name="weight">The weighting to give this comparison.</param>
        public void NotEqual<T>(IEquatable<T> a, IEquatable<T> b, float weight)
        {
            if (!a.Equals(b))
                ranking += weight;
        }

        /// <summary>
        ///     Instructs that a better ranking would hold this condition.
        /// </summary>
        /// <param name="b">The outcome of the condition.</param>
        /// <param name="weight">The weighting to give this comparison.</param>
        public void IsTrue(bool b, float weight)
        {
            if (b)
                ranking += weight;
        }

        /// <summary>
        ///     Instructs that a better ranking would not hold this condition.
        /// </summary>
        /// <param name="b">The outcome of the condition.</param>
        /// <param name="weight">The weighting to give this comparison.</param>
        public void IsFalse(bool b, float weight)
        {
            if (!b)
                ranking += weight;
        }
        
        /// <summary>
        ///     Instructs that this pair should not be ranked together.
        /// </summary>
        /// <remarks>
        ///     This does not early out the ranking algorithm and instead just forces <see cref="Ranking"/>
        ///     to return 0. Therefore it is desirable that you should immediately return from the ranking 
        ///     after calling this, if possible, to avoid any further unecessary computation.
        /// </remarks>
        public void Fail()
        {
            Failed = true;
        }

        /// <summary>
        ///     Clears the ranking and resets the builder.
        /// </summary>
        public void Clear()
        {
            ranking = 0;
            Failed = false;
        }
    }
}
#endif
