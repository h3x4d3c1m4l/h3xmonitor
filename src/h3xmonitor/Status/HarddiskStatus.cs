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
    public class HarddiskStatus
    {
        /// <summary>
        /// Name of the physical disk.
        /// </summary>
        [JsonProperty("model")]
        public string Model { get; set; }

        /// <summary>
        /// Disk address (e.g. /dev/sda).
        /// </summary>
        [JsonProperty("address")]
        public string Address { get; set; }

        /// <summary>
        /// Whether the disk seems to be healthy.
        /// </summary>
        [JsonProperty("isHealthy")]
        public bool IsHealthy { get; set; }

        /// <summary>
        /// SMART diagnostic data.
        /// </summary>
        [JsonProperty("smartData")]
        public string SMARTData { get; set; }
    }
}
