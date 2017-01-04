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
using System.Collections.Generic;
using h3xmonitor.Status;
using Newtonsoft.Json;

namespace h3xmonitor.Settings
{
    /// <summary>
    /// A server that should be checked.
    /// </summary>
    public class Server
    {
        /// <summary>
        /// Whether to skip this server or not.
        /// </summary>
        [JsonProperty("skip")]
        public bool Skip { get; set; }

        /// <summary>
        /// Server hostname or IP.
        /// </summary>
        [JsonProperty("hostnameOrIP")]
        public string HostnameOrIP { get; set; }

        /// <summary>
        /// Friendly server name.
        /// </summary>
        [JsonProperty("friendlyName")]
        public string FriendlyName { get; set; }

        /// <summary>
        /// Server username.
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; }

        /// <summary>
        /// Server password.
        /// </summary>        
        [JsonProperty("password")]
        public string Password { get; set; }

        /// <summary>
        /// Server OS.
        /// </summary>
        [JsonProperty("os")]
        public ServerOS OS { get; set; }

        /// <summary>
        /// Types of diskcontrollers this server has.
        /// </summary>
        [JsonProperty("diskControllers")]
        public DiskControllers DiskControllers { get; set; }

        /// <summary>
        /// TCP based services that shall be tested for availability.
        /// </summary>
        [JsonProperty("tcpServices")]
        public List<ServiceTest> TCPServices { get; set; }

        /// <summary>
        /// Object that will be copied to the output.
        /// </summary>
        [JsonProperty("reference")]
        public object Reference { get; set; }
    }
}
