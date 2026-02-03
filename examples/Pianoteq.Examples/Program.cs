using Pianoteq.Client;
using Pianoteq.Client.Exceptions;
using System.Net.Http.Json;
using System.Text.Json;

// Replace with your Pianoteq server address
const string serverUrl = "http://retropie:8081";

Console.WriteLine("=== RAW JSON Response Dump ===\n");

// Use HttpClient directly to see raw responses - mimicking what PianoteqClient does
using var httpClient = new HttpClient();

// Test 1: getInfo
Console.WriteLine("=== RAW getInfo Response ===");
var getInfoRequestJson = "{\"jsonrpc\":\"2.0\",\"method\":\"getInfo\",\"params\":[],\"id\":1}";
var getInfoContent = new StringContent(getInfoRequestJson, System.Text.Encoding.UTF8, "application/json");
var getInfoResponse = await httpClient.PostAsync($"{serverUrl}/jsonrpc", getInfoContent);
Console.WriteLine($"Status: {getInfoResponse.StatusCode}");
var getInfoRaw = await getInfoResponse.Content.ReadAsStringAsync();
if (string.IsNullOrEmpty(getInfoRaw))
{
    Console.WriteLine("ERROR: Empty response!");
}
else
{
    // Pretty print
    var doc = JsonDocument.Parse(getInfoRaw);
    Console.WriteLine(JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true }));
}
Console.WriteLine("\n" + new string('=', 80) + "\n");

// Test 2: getListOfPresets (just first 5 for brevity)
Console.WriteLine("=== RAW getListOfPresets Response (first 5 presets) ===");
var getPresetsRequestJson = "{\"jsonrpc\":\"2.0\",\"method\":\"getListOfPresets\",\"params\":[],\"id\":2}";
var getPresetsContent = new StringContent(getPresetsRequestJson, System.Text.Encoding.UTF8, "application/json");
var getPresetsResponse = await httpClient.PostAsync($"{serverUrl}/jsonrpc", getPresetsContent);
Console.WriteLine($"Status: {getPresetsResponse.StatusCode}");
var getPresetsRaw = await getPresetsResponse.Content.ReadAsStringAsync();
if (string.IsNullOrEmpty(getPresetsRaw))
{
    Console.WriteLine("ERROR: Empty response!");
}
else
{
    // Pretty print just the first 5 presets
    var presetsDoc = JsonDocument.Parse(getPresetsRaw);
    Console.WriteLine("DEBUG: Full raw response:");
    Console.WriteLine(getPresetsRaw.Substring(0, Math.Min(500, getPresetsRaw.Length)));
    Console.WriteLine("...(truncated)");
    Console.WriteLine();
    
    if (presetsDoc.RootElement.TryGetProperty("result", out var resultArray))
    {
        if (resultArray.ValueKind == JsonValueKind.Array && resultArray.GetArrayLength() > 0)
        {
            var presets = resultArray[0]; // Get the first element which is the array of presets
            if (presets.ValueKind == JsonValueKind.Array)
            {
                Console.WriteLine($"First 5 presets:");
                foreach (var preset in presets.EnumerateArray().Take(5))
                {
                    Console.WriteLine(JsonSerializer.Serialize(preset, new JsonSerializerOptions { WriteIndented = true }));
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine($"First result element is not an array: {presets.ValueKind}");
            }
        }
        else
        {
            Console.WriteLine($"Result is not an array or is empty. Kind: {resultArray.ValueKind}, Length: {(resultArray.ValueKind == JsonValueKind.Array ? resultArray.GetArrayLength() : 0)}");
        }
    }
    else
    {
        Console.WriteLine("Full response:");
        Console.WriteLine(JsonSerializer.Serialize(presetsDoc, new JsonSerializerOptions { WriteIndented = true }));
    }
}
Console.WriteLine("\n" + new string('=', 80) + "\n");

// Now use the typed client
Console.WriteLine("=== Using Typed Client ===\n");

using var client = new PianoteqClient(serverUrl);

try
{

    // Get basic info
    var info = await client.GetInfoAsync();
    Console.WriteLine($"Pianoteq Version: {info.Version}");
    Console.WriteLine($"Current Preset: {info.CurrentPreset?.Name ?? "None"}");
    Console.WriteLine($"Bank: {info.CurrentPreset?.Bank ?? "N/A"}\n");

    // Dump current preset from GetInfo
    Console.WriteLine("=== Current Preset from GetInfo ===");
    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(info.CurrentPreset, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    Console.WriteLine();

    // Get performance info
    var perfInfo = await client.GetPerfInfoAsync();
    Console.WriteLine($"CPU Usage: {perfInfo.CpuUsage:F1}%");
    Console.WriteLine($"Active Voices: {perfInfo.Voices}/{perfInfo.MaxVoices}\n");

    // List available presets (first 10)
    Console.WriteLine("=== First 10 Presets from GetListOfPresets ===");
    var presets = await client.GetListOfPresetsAsync();
    foreach (var preset in presets.Take(10))
    {
        var favMarker = preset.Favourite == true ? "★" : " ";
        Console.WriteLine($"{favMarker} {preset.Name} [{preset.Bank}] - Instrument: {preset.Instrument}");
    }
    Console.WriteLine($"Total presets: {presets.Count}\n");

    // Dump first preset with all fields
    Console.WriteLine("=== First Preset Full JSON ===");
    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(presets.First(), new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    Console.WriteLine();

    // Find the current preset in the list to compare
    var currentPresetInList = presets.FirstOrDefault(p => p.Name == info.CurrentPreset?.Name);
    if (currentPresetInList != null)
    {
        Console.WriteLine("=== Current Preset from List (compare with GetInfo) ===");
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(currentPresetInList, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine();
    }

    // Get current parameters
    Console.WriteLine("=== Selected Parameters ===");
    var parameters = await client.GetParametersAsync();
    var interestingParams = parameters
        .Where(p => p.Id == "Condition" || p.Id == "HammerHardness" || p.Id == "Volume")
        .ToList();

    foreach (var param in interestingParams)
    {
        Console.WriteLine($"{param.Name}: {param.Text ?? param.NormalizedValue.ToString("F2")}");
    }
    Console.WriteLine();

    // Test favorites functionality
    Console.WriteLine("=== Favorites Management ===");
    var favorites = await client.GetFavoritePresetsAsync();
    Console.WriteLine($"Total favorite presets: {favorites.Count}");
    
    if (favorites.Any())
    {
        Console.WriteLine("\nAll favorite presets:");
        foreach (var fav in favorites)
        {
            var tags = fav.Tags != null && fav.Tags.Any() 
                ? $" [Tags: {string.Join(", ", fav.Tags)}]" 
                : "";
            Console.WriteLine($"  ★ {fav.Name}{tags}");
        }
        
        // Test navigation
        Console.WriteLine("\n--- Testing favorite navigation ---");
        var currentInfo = await client.GetInfoAsync();
        Console.WriteLine($"Current preset: {currentInfo.CurrentPreset?.Name}");
        
        Console.WriteLine("\nNavigating to next favorite...");
        await client.NextFavouritePresetAsync();
        currentInfo = await client.GetInfoAsync();
        var isFavorite = await client.IsCurrentPresetFavoriteAsync();
        Console.WriteLine($"After next: {currentInfo.CurrentPreset?.Name} (Is favorite: {isFavorite})");
        
        Console.WriteLine("\nNavigating to previous favorite...");
        await client.PrevFavouritePresetAsync();
        currentInfo = await client.GetInfoAsync();
        isFavorite = await client.IsCurrentPresetFavoriteAsync();
        Console.WriteLine($"After prev: {currentInfo.CurrentPreset?.Name} (Is favorite: {isFavorite})");
    }
    else
    {
        Console.WriteLine("\n💡 No favorites found. You can mark presets as favorites in the Pianoteq UI.");
        Console.WriteLine("   (Favorites cannot be set via the JSON-RPC API)");
    }
    Console.WriteLine();

    // Metronome info
    var metronome = await client.GetMetronomeAsync();
    Console.WriteLine("=== Metronome ===");
    Console.WriteLine($"Enabled: {metronome.Enabled}");
    Console.WriteLine($"BPM: {metronome.Bpm}");
    Console.WriteLine($"Time Signature: {metronome.TimeSig}\n");

    // Audio device info
    var audioDevice = await client.GetAudioDeviceInfoAsync();
    Console.WriteLine("=== Audio Device ===");
    Console.WriteLine($"Device: {audioDevice.Name}");
    Console.WriteLine($"Sample Rate: {audioDevice.SampleRate}");
    Console.WriteLine($"Buffer Size: {audioDevice.BufferSize}");
    Console.WriteLine($"Device Type: {audioDevice.DeviceType}");
    Console.WriteLine($"Channels: {audioDevice.Channels}\n");

    // Example: Load a preset (commented out to avoid changing state)
    // Console.WriteLine("Loading preset 'YC5 Basic'...");
    // await client.LoadPresetAsync("YC5 Basic");
    // Console.WriteLine("Preset loaded!\n");

    // Example: Modify a parameter (commented out)
    // Console.WriteLine("Setting Condition to 50%...");
    // await client.SetParameterAsync("Condition", 0.5);
    // Console.WriteLine("Parameter updated!\n");

    Console.WriteLine("=== Example completed successfully ===");
}
catch (PianoteqException ex)
{
    Console.WriteLine($"Pianoteq Error: {ex.Message}");
    if (ex.ErrorCode.HasValue)
    {
        Console.WriteLine($"Error Code: {ex.ErrorCode}");
    }
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Connection Error: {ex.Message}");
    Console.WriteLine($"Make sure Pianoteq is running at {serverUrl}");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected Error: {ex.Message}");
}
