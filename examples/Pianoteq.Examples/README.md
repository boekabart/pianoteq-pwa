# Pianoteq Client Example

## Quick Start

1. Make sure Pianoteq is running with JSON-RPC enabled on port 8081
2. Run the example:
   ```bash
   cd examples/Pianoteq.Examples
   dotnet run
   ```

## To change the server address

Edit [Program.cs](Program.cs) and modify this line:
```csharp
const string serverUrl = "http://retropie:8081";
```

## What the example does

- ✅ Connects to Pianoteq
- ✅ Gets version and current preset info
- ✅ Shows CPU usage and voice count
- ✅ Lists first 10 presets
- ✅ Displays selected parameters (Condition, HammerHardness, Volume)
- ✅ Shows metronome settings
- ✅ Displays audio device configuration

The example is read-only by default (won't change your Pianoteq state). To enable preset loading and parameter changes, uncomment the marked sections in Program.cs.
