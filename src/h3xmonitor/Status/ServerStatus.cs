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
