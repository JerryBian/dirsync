using System.Text.Json.Serialization;

namespace DirSync.Model
{
    public class SyncConfig
    {
        [JsonPropertyName("src")] public string Source { get; set; }

        [JsonPropertyName("target")] public string Target { get; set; }
    }
}