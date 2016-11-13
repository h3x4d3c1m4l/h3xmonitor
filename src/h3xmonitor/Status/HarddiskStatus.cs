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
