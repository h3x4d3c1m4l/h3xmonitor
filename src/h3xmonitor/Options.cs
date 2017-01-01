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
using CommandLine;

namespace h3xmonitor
{
    class Options
    {
        [Option('i', "input", HelpText = "Input file to be processed (if omitted standard input will be used).")]
        public string InputFile { get; set; }

        [Option('o', "output", HelpText = "Output file for results (if omitted standard output will be used).")]
        public string OutputFile { get; set; }

        [Option('v', "verbose", HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        [Option('s', "checkinputsyntaxonly", HelpText = "Only check if the input file has valid syntax, no real monitoring.")]
        public bool NoRealMonitoring { get; set; }
    }
}