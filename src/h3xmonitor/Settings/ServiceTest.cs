﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace h3xmonitor.Settings
{
    public class ServiceTest
    {
        /// <summary>
        /// The service's TCP-port.
        /// </summary>
        [JsonProperty("port")]
        public uint Port { get; set; }

        /// <summary>
        /// The type of test that will be done.
        /// </summary>
        [JsonProperty("test")]
        public TCPTests Test { get; set; }

        /// <summary>
        /// Object that will be copied to the output.
        /// </summary>
        [JsonProperty("reference")]
        public object Reference { get; set; }
    }
}
