using Newtonsoft.Json;

namespace h3xmonitor.Status
{
    public class FilesystemStatus
    {
        /// <summary>
        /// Name of the filesystem.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Total amount of bytes.
        /// </summary>
        [JsonProperty("totalBytes")]
        public ulong TotalBytes { get; set; }

        /// <summary>
        /// Total amount of free bytes.
        /// </summary>
        [JsonProperty("freeBytes")]
        public ulong FreeBytes { get; set; }
    }
}
