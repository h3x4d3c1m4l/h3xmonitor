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
using Newtonsoft.Json;

namespace h3xmonitor.Status
{
    public class ServiceStatus
    {
        /// <summary>
        /// TCP port number.
        /// </summary>
        [JsonProperty("port")]
        public uint Port { get; set; }

        /// <summary>
        /// Whether the connection test has succeeded.
        /// </summary>
        [JsonProperty("isOpen")]
        public bool IsOpen { get; set; }

        /// <summary>
        /// Object that is copied from the output.
        /// </summary>
        [JsonProperty("reference")]
        public object Reference { get; set; }
    }
}
