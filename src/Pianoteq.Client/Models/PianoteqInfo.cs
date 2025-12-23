using System.Text.Json.Serialization;

namespace Pianoteq.Client.Models;

public class PianoteqInfo
{
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("current_preset")]
    public PresetInfo? CurrentPreset { get; set; }

    [JsonPropertyName("licence_property_1")]
    public string? LicenceProperty1 { get; set; }

    [JsonPropertyName("licence_property_2")]
    public string? LicenceProperty2 { get; set; }
}
