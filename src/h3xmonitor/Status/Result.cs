using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace h3xmonitor.Status
{
    public class Result
    {
        /// <summary>
        /// Timestamp.
        /// </summary>
        [JsonProperty("date")]
        public DateTime Date { get; set; }

        /// <summary>
        /// Status of all checked servers.
        /// </summary>
        [JsonProperty("servers")]
        public List<ServerStatus> Servers { get; set; }
    }
}
