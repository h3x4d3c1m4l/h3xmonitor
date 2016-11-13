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
        /// Object that will be copied to the output.
        /// </summary>
        [JsonProperty("reference")]
        public object Reference { get; set; }
    }
}
