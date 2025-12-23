using System.Text.Json.Serialization;

namespace Pianoteq.Client.Models;

public class FunctionInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("spec")]
    public string Spec { get; set; } = string.Empty;

    [JsonPropertyName("doc")]
    public string Doc { get; set; } = string.Empty;
}
