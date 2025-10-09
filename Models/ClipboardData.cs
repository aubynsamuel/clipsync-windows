
using Newtonsoft.Json;

namespace ClipSyncWindows.Models
{
    public class ClipboardData
    {
        [JsonProperty("clip")]
        public string Clip { get; set; } = string.Empty;

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; } = string.Empty;
    }
}
