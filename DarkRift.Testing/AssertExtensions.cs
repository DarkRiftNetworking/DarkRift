/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DarkRift.Testing
{
    internal static class AssertExtensions
    {
        public static void AreEqualAndNotShorter<T>(T[] expected, T[] actual) where T : IEquatable<T>
        {
            if (actual.Length < expected.Length)
                throw new AssertFailedException("Actual array was too short.");

            for (int i = 0; i < expected.Length; i++)
            {
                if (!actual[i].Equals(expected[i]))
                    throw new AssertFailedException($"Element {i} was incorrect. Exepected: '{expected[i]}', actual: '{actual[i]}'");
            }
        }

        public static void AreEqualAndSameLength<T>(T[] expected, T[] actual) where T : IEquatable<T>
        {
            if (actual.Length < expected.Length)
                throw new AssertFailedException("Actual array was too short.");

            if (actual.Length > expected.Length)
                throw new AssertFailedException("Actual array was too long.");

            for (int i = 0; i < expected.Length; i++)
            {
                if (!actual[i].Equals(expected[i]))
                    throw new AssertFailedException($"Element {i} was incorrect. Exepected: '{expected[i]}', actual: '{actual[i]}'");
            }
        }
    }
}
