# Pianoteq.Client

A fully typed, async C# client library for the Pianoteq 9 JSON-RPC API, with a Progressive Web App (PWA) for remote control.

## 🎹 Projects

### Pianoteq.Client
C# client library for the Pianoteq JSON-RPC API.

### Pianoteq.Pwa
Progressive Web App for controlling Pianoteq remotely. Features:
- 📱 **Installable** - Add to home screen on mobile/desktop
- ⚡ **Offline-capable** - Service worker caches app assets
- ⭐ **Favorites management** - Local storage for favorite presets
- 🎵 **Browse & load presets** - 3-tier navigation (favorites/instrument/preset)
- 🎚️ **Parameter control** - Adjust Condition and other parameters
- 🔒 **Licensed filter** - Show only owned presets
- 🎨 **Modern UI** - Material Design with MudBlazor

## 🚀 Quick Start

### Running Locally
```bash
cd src/Pianoteq.Pwa
dotnet run
```
Open http://localhost:5000 (or port shown in console).

### Docker Deployment
```bash
# Using docker-compose (recommended)
docker-compose up -d

# Or build and run manually
docker build -t pianoteq-pwa .
docker run -p 8080:80 pianoteq-pwa
```

Access at http://localhost:8080.

**⚡ Performance**: The Docker image uses optimized layering - app updates download only ~200 KB instead of 18 MB. See [Docker Optimization Guide](docs/DOCKER-OPTIMIZATION.md).

### Using PowerShell Scripts
```powershell
# Build
.\build.ps1 -Version 1.0.0

# Build Docker image
.\build.ps1 -Docker -Version 1.0.0

# Deploy
.\deploy.ps1 start -Port 8080

# View logs
.\deploy.ps1 logs

# Stop
.\deploy.ps1 stop
```

### Prerequisites
- Pianoteq 9 running with JSON-RPC enabled (Preferences → Remote control)
- Update server URL in [src/Pianoteq.Pwa/Program.cs](src/Pianoteq.Pwa/Program.cs#L15) if not at `http://192.168.86.66:8081`

### Installing as PWA
1. Open the app in Chrome/Edge
2. Click the install icon in the address bar (⊕ or ⬇️)
3. Or use browser menu: "Install Pianoteq Remote..."
4. The app will be added to your home screen/apps

**iOS**: Safari → Share → Add to Home Screen

## 📖 Documentation

- [PWA Features & Architecture](docs/PWA.md) - Detailed PWA documentation
- [API Reference](#api-coverage) - Complete API method list
- [JSON-RPC Spec](docs/jsonrpc.html) - Pianoteq API documentation

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

// Work with favorites
var favorites = await client.GetFavoritePresetsAsync();
Console.WriteLine($"You have {favorites.Count} favorite presets");

foreach (var fav in favorites)
{
    Console.WriteLine($"★ {fav.Name}");
    if (fav.Tags != null && fav.Tags.Any())
    {
        Console.WriteLine($"  Tags: {string.Join(", ", fav.Tags)}");
    }
}

// Navigate through favorites only
await client.NextFavouritePresetAsync();
await client.PrevFavouritePresetAsync();

// Check if current preset is a favorite
bool isFavorite = await client.IsCurrentPresetFavoriteAsync();
Console.WriteLine($"Current preset is favorite: {isFavorite}");

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
- `GetFavoritePresetsAsync()` - Get only favorite presets
- `LoadPresetAsync()` - Load a preset
- `SavePresetAsync()` - Save current preset
- `DeletePresetAsync()` - Delete a preset
- `ResetPresetAsync()` - Reset to saved preset
- `NextPresetAsync()` / `PrevPresetAsync()` - Navigate presets
- `NextFavouritePresetAsync()` / `PrevFavouritePresetAsync()` - Navigate favourites
- `NextInstrumentAsync()` / `PrevInstrumentAsync()` - Navigate instruments
- `IsCurrentPresetFavoriteAsync()` - Check if current preset is a favorite

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
    await client.LoadPresetAsync("NonExistentPr

## Important Notes

### Favorites Management
The client can **read** and **navigate** favorite presets, but **cannot set/unset** favorites programmatically. Favorites must be managed through the Pianoteq UI. The API provides:
- ✅ `GetFavoritePresetsAsync()` - Get list of favorite presets
- ✅ `NextFavouritePresetAsync()` / `PrevFavouritePresetAsync()` - Navigate favorites
- ✅ `IsCurrentPresetFavoriteAsync()` - Check if current preset is favorited
- ❌ No API method to mark/unmark presets as favorites (use Pianoteq UI)

### Preset Tags
Similarly, preset tags are read-only via the API and must be managed in the Pianoteq UI.eset");
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
