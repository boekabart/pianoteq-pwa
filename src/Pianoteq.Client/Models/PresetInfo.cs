using System.Text.Json.Serialization;

namespace Pianoteq.Client.Models;

public class PresetInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("bank")]
    public string Bank { get; set; } = string.Empty;

    [JsonPropertyName("instrument")]
    public string? Instrument { get; set; }

    // getListOfPresets uses "instr" instead of "instrument"
    [JsonPropertyName("instr")]
    public string? Instr
    {
        get => Instrument;
        set => Instrument = value;
    }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("class")]
    public string? Class { get; set; }

    [JsonPropertyName("collection")]
    public string? Collection { get; set; }

    [JsonPropertyName("license")]
    public string? License { get; set; }

    [JsonPropertyName("license_status")]
    public string? LicenseStatus { get; set; }

    [JsonPropertyName("file")]
    public string? File { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("favourite")]
    public bool? Favourite { get; set; }
}
