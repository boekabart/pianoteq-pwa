using Pianoteq.Client;
using Pianoteq.Client.Exceptions;

namespace Pianoteq.Examples;

/// <summary>
/// Example console application demonstrating the Pianoteq.Client library
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Replace with your Pianoteq server address
        const string serverUrl = "http://192.168.86.30:8081";

        using var client = new PianoteqClient(serverUrl);

        try
        {
            Console.WriteLine("=== Pianoteq Client Example ===\n");

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
            Console.WriteLine($"Sample Rate: {audioDevice.SampleRate} Hz");
            Console.WriteLine($"Buffer Size: {audioDevice.BufferSize} samples");
            Console.WriteLine($"Channels: {audioDevice.InputChannels} in / {audioDevice.OutputChannels} out\n");

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
    }
}
