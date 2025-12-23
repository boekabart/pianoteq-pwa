using System.Text.Json.Serialization;

namespace Pianoteq.Client.JsonRpc;

public class JsonRpcRequest
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";
    
    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;
    
    [JsonPropertyName("params")]
    public object? Params { get; set; }
    
    [JsonPropertyName("id")]
    public int Id { get; set; }
}
