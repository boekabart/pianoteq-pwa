using System.Text.Json.Serialization;

namespace Pianoteq.Client.Models;

public class PresetInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("bank")]
    public string Bank { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("favourite")]
    public bool? Favourite { get; set; }
}
