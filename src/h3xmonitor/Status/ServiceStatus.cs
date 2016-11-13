using Newtonsoft.Json;

namespace h3xmonitor.Status
{
    public class ServiceStatus
    {
        /// <summary>
        /// TCP port number.
        /// </summary>
        [JsonProperty("port")]
        public int Port { get; set; }

        /// <summary>
        /// Whether the connection test has succeeded.
        /// </summary>
        [JsonProperty("isOpen")]
        public bool IsOpen { get; set; }
    }
}
