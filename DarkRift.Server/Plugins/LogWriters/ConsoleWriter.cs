/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if INLINE_CONSOLE_METHODS
using System.Runtime.CompilerServices;
#endif
using System.Text;

namespace DarkRift.Server.Plugins.LogWriters
{
    /// <summary>
    ///     Log writer that outputs to the console.
    /// </summary>
    public sealed class ConsoleWriter : LogWriter
    {
        /// <inheritdoc/>
        public override Version Version => new Version(1, 0, 0);

        //TODO 3 expose colours to settings

        private const string ANSI_RESET_COLOR_CODE = "\u001b[0m";

        /// <summary>
        ///     The lookup table for the foreground colors to print with.
        /// </summary>
        private readonly ConsoleColor[] foregroundColours = new ConsoleColor[] { ConsoleColor.Gray, ConsoleColor.Gray, ConsoleColor.Yellow, ConsoleColor.Red, ConsoleColor.Red };

        /// <summary>
        ///     The lookup table for the background colours to print with.
        /// </summary>
        private readonly ConsoleColor[] backgroundColours = new ConsoleColor[] { ConsoleColor.Black, ConsoleColor.Black, ConsoleColor.Black, ConsoleColor.Black, ConsoleColor.Gray };

        /// <summary>
        ///     The lookup table for the ANSI colour codes to print with.
        /// </summary>
        private readonly string[] ansiColours = new string[] { "", "", "\u001b[33m", "\u001b[31m", "\u001b[31m\u001b[47m" };

        /// <summary>
        ///     Lock for console writes.
        /// </summary>
        /// <remarks>
        ///     Technically, Console is thread safe but if we don't have this then colours get mixed up with fast writes.
        /// </remarks>
        private readonly object consoleLock = new object();

        /// <summary>
        ///     Whether the faster ANSI color code coloring should be used for rendering or not.
        /// </summary>
        private readonly bool useFastAnsiColoring;

        /// <summary>
        ///     Creates a new console writer with the plugins load data.
        /// </summary>
        /// <param name="logWriterLoadData">The data to load the logwriter with.</param>
        public ConsoleWriter(LogWriterLoadData logWriterLoadData) : base(logWriterLoadData)
        {
            // Get fast ansi coloring setting, defaulting to true.
            if (logWriterLoadData.Settings["useFastAnsiColoring"] != null)
            {
                if (!bool.TryParse(logWriterLoadData.Settings["useFastAnsiColoring"], out useFastAnsiColoring))
                    throw new ArgumentException("useFastAnsiColoring setting on ConsoleWriter was not a boolean!");
            }
            else
            {
                useFastAnsiColoring = true;
            }


            if (useFastAnsiColoring)
            {
                if (!ColorsOnWindows.Enable())
                {
                    Console.WriteLine($"Could not enable ANSI coloring on Windows. Consider adding useFastAnsiColoring = false to the {nameof(ConsoleWriter)}'s settings (https://www.darkriftnetworking.com/DarkRift2/Docs/2.9.0/advanced/internal_plugins/console_writer.html):\n" +
                        $"\n" +
                        $"\t<logWriter name=\"{Name}\" type=\"{nameof(ConsoleWriter)}\" levels=\"...\">\n" +
                        $"\t\t<settings useFastAnsiColoring=\"false\" />\n" +
                        $"\t</logWriter>.\n" +
                        $"\n" +
                        $"Last error code was '{ColorsOnWindows.GetLastError()}'. Will continue logging to console with standard coloring methods.");
                    useFastAnsiColoring = false;
                }
            }
        }

        /// <inheritdoc/>
        public override void WriteEvent(WriteEventArgs args)
        {
            if (useFastAnsiColoring)
                WriteWithAnsiColorCodes(args);
            else
                WriteWithConsoleColor(args);
        }

        /// <summary>
        ///     Renders the output using ANSI color codes.
        /// </summary>
        /// <param name="args">The args passed to the event.</param>
#if INLINE_CONSOLE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void WriteWithAnsiColorCodes(WriteEventArgs args)
        {
            // Add colour codes
            string coloredMessage = ansiColours[(int)args.LogType] + args.FormattedMessage + ANSI_RESET_COLOR_CODE;

            // Output
            if (args.LogType == LogType.Error)
                Console.Error.WriteLine(coloredMessage);
            else
                Console.WriteLine(coloredMessage);

        }

        /// <summary>
        ///     Renders the output using the console coloring methods.
        /// </summary>
        /// <param name="args">The args passed to the event.</param>
#if INLINE_CONSOLE_METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void WriteWithConsoleColor(WriteEventArgs args)
        {
            lock (consoleLock)
            {
                // Set colours
                Console.ForegroundColor = foregroundColours[(int)args.LogType];
                Console.BackgroundColor = backgroundColours[(int)args.LogType];

                // Output
                if (args.LogType == LogType.Error)
                    Console.Error.WriteLine(args.FormattedMessage);
                else
                    Console.WriteLine(args.FormattedMessage);

                Console.ResetColor();
            }
        }
    }
}
