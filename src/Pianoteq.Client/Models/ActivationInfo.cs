using System.Text.Json.Serialization;

namespace Pianoteq.Client.Models;

public class ActivationInfo
{
    [JsonPropertyName("serial")]
    public string? Serial { get; set; }

    [JsonPropertyName("device_name")]
    public string? DeviceName { get; set; }

    [JsonPropertyName("activated")]
    public bool? Activated { get; set; }
}
