﻿/*
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace h3xmonitor.Settings
{
    public class ServiceTest
    {
        /// <summary>
        /// The service's TCP-port.
        /// </summary>
        [JsonProperty("port")]
        public uint Port { get; set; }

        /// <summary>
        /// The type of test that will be done.
        /// </summary>
        [JsonProperty("test")]
        public TCPTests Test { get; set; }

        /// <summary>
        /// Object that will be copied to the output.
        /// </summary>
        [JsonProperty("reference")]
        public object Reference { get; set; }
    }
}
