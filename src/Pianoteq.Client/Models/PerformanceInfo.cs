using System.Text.Json.Serialization;

namespace Pianoteq.Client.Models;

public class PerformanceInfo
{
    [JsonPropertyName("cpu_usage")]
    public double? CpuUsage { get; set; }

    [JsonPropertyName("voices")]
    public int? Voices { get; set; }

    [JsonPropertyName("max_voices")]
    public int? MaxVoices { get; set; }

    [JsonPropertyName("audio_buffer")]
    public List<int>? AudioBuffer { get; set; }

    [JsonPropertyName("audio_load")]
    public List<double>? AudioLoad { get; set; }
}
