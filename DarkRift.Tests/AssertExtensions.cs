/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using NUnit.Framework;

namespace DarkRift.Tests
{
    internal static class AssertExtensions
    {
        public static void AreEqualAndNotShorter<T>(T[] expected, T[] actual) where T : IEquatable<T>
        {
            if (actual.Length < expected.Length)
            {
                Assert.Fail("Actual array was too short.");
            }

            for (int i = 0; i < expected.Length; i++)
            {
                if (!actual[i].Equals(expected[i]))
                {
                    Assert.Fail($"Element {i} was incorrect. Exepected: '{expected[i]}', actual: '{actual[i]}'");
                }
            }
        }

        public static void AreEqualAndSameLength<T>(T[] expected, T[] actual) where T : IEquatable<T>
        {
            if (actual.Length < expected.Length)
            {
                Assert.Fail("Actual array was too short.");
            }

            if (actual.Length > expected.Length)
            {
                Assert.Fail("Actual array was too long.");
            }

            for (int i = 0; i < expected.Length; i++)
            {
                if (!actual[i].Equals(expected[i]))
                {
                    Assert.Fail($"Element {i} was incorrect. Exepected: '{expected[i]}', actual: '{actual[i]}'");
                }
            }
        }
    }
}
