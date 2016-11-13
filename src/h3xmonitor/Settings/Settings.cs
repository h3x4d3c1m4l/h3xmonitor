using System.Collections.Generic;
using Newtonsoft.Json;

namespace h3xmonitor.Settings
{
    public class Settings
    {
        /// <summary>
        /// Servers that will be checked.
        /// </summary>
        [JsonProperty("servers")]
        public List<Server> Servers { get; set; }
    }
}
