/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.InteropServices;

namespace DarkRift.Server.Plugins.LogWriters
{
    // Taken from Crayon here https://github.com/riezebosch/crayon
    /// https://www.jerriepelser.com/blog/using-ansi-color-codes-in-net-console-apps/
    internal static class ColorsOnWindows
    {
        public static bool Enable()
        {
            if (!IsOnWindows())
            {
                return true;
            }

            var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
            return GetConsoleMode(iStdOut, out uint outConsoleMode) &&
                SetConsoleMode(iStdOut, outConsoleMode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
        }

        private static bool IsOnWindows()
        {
#if NETFRAMEWORK
            return Environment.OSVersion.Platform == PlatformID.Win32NT
                || Environment.OSVersion.Platform == PlatformID.Win32S
                || Environment.OSVersion.Platform == PlatformID.Win32Windows
                || Environment.OSVersion.Platform == PlatformID.WinCE;
#else
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif
        }

        private const int STD_OUTPUT_HANDLE = -11;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();
    }
}
