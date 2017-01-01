/*
    This file is part of h3xmonitor.

    h3xmonitor is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    h3xmonitor is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace h3xmonitor.Logging
{
    public static class Log
    {
        /// <summary>
        /// Collection to collect lines that haven't been written yet.
        /// </summary>
        private static readonly BlockingCollection<Logline> LogList;

        /// <summary>
        /// Autostarts the logging thread.
        /// </summary>
        static Log()
        {
            LogList = new BlockingCollection<Logline>(new ConcurrentQueue<Logline>());
            new Thread(Mainloop) { Name = "Log" }.Start();
        }

        /// <summary>
        /// Loop that constantly dumps new log lines (to console/file, whatever is desired).
        /// </summary>
        private static void Mainloop()
        {
            foreach (var l in LogList.GetConsumingEnumerable())
            {
                // set console color
                Console.ResetColor();
                switch (l.Level)
                {
                    case LoglineLevel.Fatal:
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case LoglineLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LoglineLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LoglineLevel.Info:
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        break;
                    case LoglineLevel.Debug:
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // dump to console
                Console.Error.WriteLine(l.Text);
            }
        }

        /// <summary>
        /// Write a line to the log.
        /// </summary>
        /// <param name="pLevel">Level of the line (e.g. error, warning)</param>
        /// <param name="pText">Text of the line</param>
        public static void Write(LoglineLevel pLevel, string pText)
        {
            LogList.Add(new Logline { Level = pLevel, Text = pText });
        }
    }
}
