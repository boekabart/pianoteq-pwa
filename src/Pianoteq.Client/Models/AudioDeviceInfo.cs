using System.Text.Json.Serialization;

namespace Pianoteq.Client.Models;

public class AudioDeviceInfo
{
    [JsonPropertyName("audio_output_device_name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("sample_rate")]
    public string? SampleRate { get; set; }

    [JsonPropertyName("buffer_size")]
    public string? BufferSize { get; set; }

    [JsonPropertyName("channels")]
    public string? Channels { get; set; }

    [JsonPropertyName("device_type")]
    public string? DeviceType { get; set; }
}
