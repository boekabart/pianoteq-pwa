# Pianoteq.Client

A fully typed, async C# client library for the Pianoteq 9 JSON-RPC API.

## Installation

Reference the `Pianoteq.Client` project in your application.

## Usage

```csharp
using Pianoteq.Client;
using Pianoteq.Client.Models;

// Create a client instance
using var client = new PianoteqClient("http://192.168.86.66:8081");

// Get Pianoteq information
var info = await client.GetInfoAsync();
Console.WriteLine($"Pianoteq Version: {info.Version}");
Console.WriteLine($"Current Preset: {info.CurrentPreset?.Name}");

// Get list of all presets
var presets = await client.GetListOfPresetsAsync();
foreach (var preset in presets)
{
    Console.WriteLine($"{preset.Name} (Bank: {preset.Bank})");
}

// Load a specific preset
await client.LoadPresetAsync("YC5 Basic");

// Get and modify parameters
var parameters = await client.GetParametersAsync();
var condition = parameters.FirstOrDefault(p => p.Id == "Condition");
if (condition != null)
{
    Console.WriteLine($"Current Condition: {condition.NormalizedValue}");
}

// Set a parameter value
await client.SetParameterAsync("Condition", 0.5);

// Control the metronome
await client.SetMetronomeAsync(
    enabled: true,
    bpm: 120,
    timeSig: "4/4"
);

// Get metronome state
var metronome = await client.GetMetronomeAsync();
Console.WriteLine($"Metronome: {metronome.Bpm} BPM, {metronome.TimeSig}");

// MIDI playback control
await client.LoadMidiFileAsync(@"C:\path\to\file.mid");
await client.MidiPlayAsync();
await Task.Delay(5000);
await client.MidiStopAsync();

// Navigate presets
await client.NextPresetAsync();
await client.PrevPresetAsync();
await client.NextInstrumentAsync();

// A/B comparison
await client.AbCopyAsync();
await client.AbSwitchAsync();

// Performance info
var perfInfo = await client.GetPerfInfoAsync();
Console.WriteLine($"CPU Usage: {perfInfo.CpuUsage}%");
Console.WriteLine($"Voices: {perfInfo.Voices}/{perfInfo.MaxVoices}");
```

## Features

- ✅ Fully async/await pattern
- ✅ Strongly typed models for all API responses
- ✅ Comprehensive XML documentation
- ✅ Cancellation token support
- ✅ Custom HttpClient support for dependency injection
- ✅ Exception handling with detailed error information
- ✅ .NET 10 compatible

## API Coverage

The client implements all Pianoteq JSON-RPC methods:

### Information
- `GetInfoAsync()` - Get current state
- `GetPerfInfoAsync()` - Get CPU performance info
- `ListAsync()` - Get list of available functions

### Presets
- `GetListOfPresetsAsync()` - Get list of presets
- `LoadPresetAsync()` - Load a preset
- `SavePresetAsync()` - Save current preset
- `DeletePresetAsync()` - Delete a preset
- `ResetPresetAsync()` - Reset to saved preset
- `NextPresetAsync()` / `PrevPresetAsync()` - Navigate presets
- `NextFavouritePresetAsync()` / `PrevFavouritePresetAsync()` - Navigate favourites
- `NextInstrumentAsync()` / `PrevInstrumentAsync()` - Navigate instruments

### Parameters
- `GetParametersAsync()` - Get all parameters
- `SetParametersAsync()` - Set multiple parameters
- `SetParameterAsync()` - Set single parameter
- `RandomizeParametersAsync()` - Randomize parameters

### MIDI
- `LoadMidiFileAsync()` / `SaveMidiFileAsync()` - File operations
- `MidiSendAsync()` - Send raw MIDI bytes
- `MidiPlayAsync()` / `MidiStopAsync()` / `MidiPauseAsync()` - Playback control
- `MidiRewindAsync()` / `MidiSeekAsync()` - Position control
- `MidiRecordAsync()` - Record MIDI
- `PanicAsync()` / `MuteAsync()` - Emergency controls
- `GetSequencerInfoAsync()` - Get sequencer state

### Metronome
- `GetMetronomeAsync()` - Get metronome state
- `SetMetronomeAsync()` - Configure metronome

### Audio
- `GetAudioDeviceInfoAsync()` - Get current device info
- `GetListOfAudioDevicesAsync()` - List available devices

### Other
- `LoadFileAsync()` - Load any supported file
- `UndoAsync()` / `RedoAsync()` - Edit history
- `AbSwitchAsync()` / `AbCopyAsync()` - A/B comparison
- `ActivateAsync()` / `GetActivationInfoAsync()` - License management
- `QuitAsync()` - Quit Pianoteq

## Exception Handling

```csharp
using Pianoteq.Client.Exceptions;

try
{
    await client.LoadPresetAsync("NonExistentPreset");
}
catch (PianoteqException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Error Code: {ex.ErrorCode}");
}
```

## Dependency Injection

```csharp
services.AddHttpClient<PianoteqClient>((serviceProvider, httpClient) =>
{
    return new PianoteqClient(httpClient, "http://192.168.86.66:8081");
});
```

## Requirements

- .NET 10.0 or later
- Pianoteq 9.1.0 or later with JSON-RPC enabled
