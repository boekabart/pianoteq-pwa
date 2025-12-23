using System.Text.Json.Serialization;

namespace Pianoteq.Client.Models;

public class SequencerInfo
{
    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("position")]
    public double Position { get; set; }

    [JsonPropertyName("duration")]
    public double Duration { get; set; }

    [JsonPropertyName("file")]
    public string? File { get; set; }
}
