using System.Text.Json.Serialization;

namespace Pianoteq.Client.Models;

public class MetronomeInfo
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("bpm")]
    public double Bpm { get; set; }

    [JsonPropertyName("volume_db")]
    public double VolumeDb { get; set; }

    [JsonPropertyName("timesig")]
    public string TimeSig { get; set; } = "4/4";

    [JsonPropertyName("accentuate")]
    public bool Accentuate { get; set; }
}
