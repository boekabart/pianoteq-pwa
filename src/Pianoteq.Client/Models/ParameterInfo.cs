using System.Text.Json.Serialization;

namespace Pianoteq.Client.Models;

public class ParameterInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("group")]
    public string? Group { get; set; }

    [JsonPropertyName("index")]
    public int? Index { get; set; }

    [JsonPropertyName("normalized_value")]
    public double NormalizedValue { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("value")]
    public double? Value { get; set; }

    [JsonPropertyName("range_min")]
    public double? RangeMin { get; set; }

    [JsonPropertyName("range_max")]
    public double? RangeMax { get; set; }

    [JsonPropertyName("discrete_values")]
    public List<string>? DiscreteValues { get; set; }
}
