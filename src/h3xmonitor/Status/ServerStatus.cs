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
using System.Net.NetworkInformation;
using Newtonsoft.Json;

namespace h3xmonitor.Status
{
    public class ServerStatus
    {
        /// <summary>
        /// Server name.
        /// </summary>
        [JsonProperty("friendlyName")]
        public string FriendlyName { get; set; }

        /// <summary>
        /// Server hostname.
        /// </summary>
        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        /// <summary>
        /// Server IP.
        /// </summary>
        [JsonProperty("ip")]
        public string IP { get; set; }

        /// <summary>
        /// Server OS.
        /// </summary>
        [JsonProperty("os")]
        public ServerOS OS { get; set; }

        /// <summary>
        /// Server OS version.
        /// </summary>
        [JsonProperty("osVersion")]
        public string OSVersion { get; set; }

        /// <summary>
        /// List of physical disks.
        /// </summary>
        [JsonProperty("harddisks")]
        public List<HarddiskStatus> Harddisks { get; set; }

        /// <summary>
        /// List of filesystems.
        /// </summary>
        [JsonProperty("filesystems")]
        public List<FilesystemStatus> Filesystems { get; set; }

        /// <summary>
        /// List of services.
        /// </summary>
        [JsonProperty("services")]
        public List<ServiceStatus> Services { get; set; }

        /// <summary>
        /// Whether the monitoring has failed.
        /// </summary>
        [JsonProperty("monitoringFailed")]
        public bool MonitoringFailed { get; set; }

        /// <summary>
        /// Error if the monitoring failed.
        /// </summary>
        [JsonProperty("monitoringError")]
        public string MonitoringError { get; set; }

        /// <summary>
        /// Object that is copied from the server settings.
        /// </summary>
        [JsonProperty("reference")]
        public object Reference { get; set; }

        /// <summary>
        /// Round trip time.
        /// </summary>
        [JsonProperty("ping")]
        public double? Ping { get; set; }
    }
}
