using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Pianoteq.Client.Exceptions;
using Pianoteq.Client.JsonRpc;
using Pianoteq.Client.Models;

namespace Pianoteq.Client;

/// <summary>
/// Async client for the Pianoteq JSON-RPC API
/// </summary>
public class PianoteqClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly bool _disposeHttpClient;
    private int _requestId = 0;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    /// <summary>
    /// Create a new Pianoteq client
    /// </summary>
    /// <param name="baseUrl">Base URL of the Pianoteq server (e.g., "http://192.168.86.66:8081")</param>
    public PianoteqClient(string baseUrl)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _httpClient = new HttpClient();
        _disposeHttpClient = true;
    }

    /// <summary>
    /// Create a new Pianoteq client with a custom HttpClient
    /// </summary>
    /// <param name="httpClient">Custom HttpClient instance</param>
    /// <param name="baseUrl">Base URL of the Pianoteq server (e.g., "http://192.168.86.66:8081")</param>
    public PianoteqClient(HttpClient httpClient, string baseUrl)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl.TrimEnd('/');
        _disposeHttpClient = false;
    }

    private async Task<T> SendRequestAsync<T>(string method, object? parameters = null, CancellationToken cancellationToken = default)
    {
        var request = new JsonRpcRequest
        {
            Method = method,
            Params = parameters ?? new object[] { },  // Default to empty array instead of null
            Id = Interlocked.Increment(ref _requestId)
        };

        var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(
            $"{_baseUrl}/jsonrpc",
            content,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            throw new PianoteqException("Server returned an empty response");
        }

        JsonRpcResponse<T>? jsonRpcResponse;
        try
        {
            jsonRpcResponse = JsonSerializer.Deserialize<JsonRpcResponse<T>>(responseContent, _jsonOptions);
        }
        catch (JsonException ex)
        {
            throw new PianoteqException(
                $"Failed to parse server response as JSON. Response was: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}",
                ex);
        }

        if (jsonRpcResponse?.Error != null)
        {
            throw new PianoteqException(
                jsonRpcResponse.Error.Message,
                jsonRpcResponse.Error.Code,
                jsonRpcResponse.Error.Data);
        }

        return jsonRpcResponse!.Result!;
    }

    private async Task SendRequestAsync(string method, object? parameters = null, CancellationToken cancellationToken = default)
    {
        await SendRequestAsync<object>(method, parameters, cancellationToken);
    }

    #region Information Methods

    /// <summary>
    /// Get various information about the current state of Pianoteq
    /// </summary>
    public async Task<PianoteqInfo> GetInfoAsync(CancellationToken cancellationToken = default)
    {
        var result = await SendRequestAsync<List<PianoteqInfo>>("getInfo", null, cancellationToken);
        return result.FirstOrDefault() ?? new PianoteqInfo();
    }

    /// <summary>
    /// Get CPU performance information
    /// </summary>
    public async Task<PerformanceInfo> GetPerfInfoAsync(CancellationToken cancellationToken = default)
    {
        var result = await SendRequestAsync<List<PerformanceInfo>>("getPerfInfo", null, cancellationToken);
        return result.FirstOrDefault() ?? new PerformanceInfo();
    }

    /// <summary>
    /// Get the list of available JSON-RPC functions
    /// </summary>
    public async Task<List<FunctionInfo>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<List<FunctionInfo>>("list", null, cancellationToken);
    }

    #endregion

    #region Preset Methods

    /// <summary>
    /// Get the list of presets
    /// </summary>
    /// <param name="presetType">Type of presets: "full", "equ", "vel", "mic", "reverb", "tuning", "effect_rack", "effect1", "effect2", "effect3"</param>
    public async Task<List<PresetInfo>> GetListOfPresetsAsync(string presetType = "full", CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<List<PresetInfo>>("getListOfPresets", new[] { presetType }, cancellationToken);
    }

    /// <summary>
    /// Load a preset
    /// </summary>
    /// <param name="name">Preset name</param>
    /// <param name="bank">Bank name (optional)</param>
    /// <param name="presetType">Type of preset to load</param>
    public async Task LoadPresetAsync(string name, string? bank = null, string presetType = "full", CancellationToken cancellationToken = default)
    {
        var parameters = new List<object?> { name };
        if (bank != null)
        {
            parameters.Add(bank);
            parameters.Add(presetType);
        }
        await SendRequestAsync("loadPreset", parameters, cancellationToken);
    }

    /// <summary>
    /// Save the current preset
    /// </summary>
    public async Task SavePresetAsync(string name, string bank, string presetType = "full", CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("savePreset", new[] { name, bank, presetType }, cancellationToken);
    }

    /// <summary>
    /// Delete a preset (removes the file from disk)
    /// </summary>
    public async Task DeletePresetAsync(string name, string bank, string presetType = "full", CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("deletePreset", new[] { name, bank, presetType }, cancellationToken);
    }

    /// <summary>
    /// Reset parameters to saved preset
    /// </summary>
    public async Task ResetPresetAsync(CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("resetPreset", null, cancellationToken);
    }

    /// <summary>
    /// Load next preset
    /// </summary>
    public async Task NextPresetAsync(CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("nextPreset", null, cancellationToken);
    }

    /// <summary>
    /// Load previous preset
    /// </summary>
    public async Task PrevPresetAsync(CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("prevPreset", null, cancellationToken);
    }

    /// <summary>
    /// Load next favourite preset
    /// </summary>
    public async Task NextFavouritePresetAsync(CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("nextFavouritePreset", null, cancellationToken);
    }

    /// <summary>
    /// Load previous favourite preset
    /// </summary>
    public async Task PrevFavouritePresetAsync(CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("prevFavouritePreset", null, cancellationToken);
    }

    /// <summary>
    /// Load next instrument
    /// </summary>
    public async Task NextInstrumentAsync(CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("nextInstrument", null, cancellationToken);
    }

    /// <summary>
    /// Load previous instrument
    /// </summary>
    public async Task PrevInstrumentAsync(CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("prevInstrument", null, cancellationToken);
    }

    #endregion

    #region A/B Comparison Methods

    /// <summary>
    /// Switch A and B presets
    /// </summary>
    public async Task AbSwitchAsync(CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("abSwitch", null, cancellationToken);
    }

    /// <summary>
    /// Copy current preset (A or B) to the other one (B or A)
    /// </summary>
    public async Task AbCopyAsync(CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("abCopy", null, cancellationToken);
    }

    #endregion

    #region Edit Methods

    /// <summary>
    /// Undo last edition
    /// </summary>
    public async Task UndoAsync(CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("undo", null, cancellationToken);
    }

    /// <summary>
    /// Redo last edition
    /// </summary>
    public async Task RedoAsync(CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("redo", null, cancellationToken);
    }

    #endregion

    #region Parameter Methods

    /// <summary>
    /// Get the list of parameters with their values
    /// </summary>
    public async Task<List<ParameterInfo>> GetParametersAsync(CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<List<ParameterInfo>>("getParameters", null, cancellationToken);
    }

    /// <summary>
    /// Set parameter values
    /// </summary>
    /// <param name="parameters">List of parameters to set. Each parameter must have 'id' and either 'normalized_value' or 'text'</param>
    public async Task SetParametersAsync(List<ParameterInfo> parameters, CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("setParameters", new { list = parameters }, cancellationToken);
    }

    /// <summary>
    /// Set a single parameter value
    /// </summary>
    public async Task SetParameterAsync(string parameterId, double normalizedValue, CancellationToken cancellationToken = default)
    {
        var parameter = new ParameterInfo
        {
            Id = parameterId,
            NormalizedValue = normalizedValue
        };
        await SetParametersAsync(new List<ParameterInfo> { parameter }, cancellationToken);
    }

    /// <summary>
    /// Randomize parameter values
    /// </summary>
    /// <param name="amount">Amount of randomization (0.0 to 1.0)</param>
    public async Task RandomizeParametersAsync(double amount = 1.0, CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("randomizeParameters", new[] { amount }, cancellationToken);
    }

    #endregion

    #region MIDI Methods

    /// <summary>
    /// Load a MIDI file or playlist
    /// </summary>
    /// <param name="path">Path to MIDI file or folder</param>
    public async Task LoadMidiFileAsync(string path, CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("loadMidiFile", new[] { path }, cancellationToken);
    }

    /// <summary>
    /// Save the currently loaded MIDI file
    /// </summary>
    public async Task SaveMidiFileAsync(string path, CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("saveMidiFile", new[] { path }, cancellationToken);
    }

    /// <summary>
    /// Send raw MIDI bytes to the engine
    /// </summary>
    /// <param name="bytes">MIDI bytes to send</param>
    public async Task MidiSendAsync(byte[][] bytes, CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("midiSend", new { bytes }, cancellationToken);
    }

    /// <summary>
    /// Play MIDI sequence
    /// </summary>
    public async Task MidiPlayAsync(CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("midiPlay", null, cancellationToken);
    }

    /// <summary>
    /// Stop MIDI sequence
    /// </summary>
    public async Task MidiStopAsync(CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("midiStop", null, cancellationToken);
    }

    /// <summary>
    /// Pause MIDI sequence
    /// </summary>
    public async Task MidiPauseAsync(CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("midiPause", null, cancellationToken);
    }

    /// <summary>
    /// Rewind MIDI sequence
    /// </summary>
    public async Task MidiRewindAsync(CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("midiRewind", null, cancellationToken);
    }

    /// <summary>
    /// Record MIDI sequence
    /// </summary>
    public async Task MidiRecordAsync(CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("midiRecord", null, cancellationToken);
    }

    /// <summary>
    /// Seek to position in MIDI sequence
    /// </summary>
    /// <param name="seconds">Position in seconds</param>
    public async Task MidiSeekAsync(double seconds, CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("midiSeek", new[] { seconds }, cancellationToken);
    }

    /// <summary>
    /// Reset all MIDI state (panic button)
    /// </summary>
    public async Task PanicAsync(CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("panic", null, cancellationToken);
    }

    /// <summary>
    /// Mute all sound
    /// </summary>
    public async Task MuteAsync(CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("mute", null, cancellationToken);
    }

    #endregion

    #region Sequencer Methods

    /// <summary>
    /// Get the MIDI sequencer state
    /// </summary>
    public async Task<SequencerInfo> GetSequencerInfoAsync(CancellationToken cancellationToken = default)
    {
        var result = await SendRequestAsync<List<SequencerInfo>>("getSequencerInfo", null, cancellationToken);
        return result.FirstOrDefault() ?? new SequencerInfo();
    }

    #endregion

    #region Metronome Methods

    /// <summary>
    /// Get the metronome state
    /// </summary>
    public async Task<MetronomeInfo> GetMetronomeAsync(CancellationToken cancellationToken = default)
    {
        var result = await SendRequestAsync<List<MetronomeInfo>>("getMetronome", null, cancellationToken);
        return result.FirstOrDefault() ?? new MetronomeInfo();
    }

    /// <summary>
    /// Set metronome properties
    /// </summary>
    public async Task SetMetronomeAsync(
        bool? enabled = null,
        int? bpm = null,
        double? volumeDb = null,
        string? timeSig = null,
        bool? accentuate = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object?>();
        if (enabled.HasValue) parameters["enabled"] = enabled.Value;
        if (bpm.HasValue) parameters["bpm"] = bpm.Value;
        if (volumeDb.HasValue) parameters["volume_db"] = volumeDb.Value;
        if (timeSig != null) parameters["timesig"] = timeSig;
        if (accentuate.HasValue) parameters["accentuate"] = accentuate.Value;

        await SendRequestAsync("setMetronome", parameters, cancellationToken);
    }

    #endregion

    #region File Methods

    /// <summary>
    /// Load any supported file (fxp/mfxp/scl/kbm/ptq/wav...)
    /// </summary>
    public async Task LoadFileAsync(string path, CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("loadFile", new[] { path }, cancellationToken);
    }

    #endregion

    #region Audio Device Methods

    /// <summary>
    /// Get information about the current audio device
    /// </summary>
    public async Task<AudioDeviceInfo> GetAudioDeviceInfoAsync(CancellationToken cancellationToken = default)
    {
        // This endpoint returns an object, not an array
        return await SendRequestAsync<AudioDeviceInfo>("getAudioDeviceInfo", null, cancellationToken);
    }

    /// <summary>
    /// Get the list of available audio devices
    /// </summary>
    public async Task<List<AudioDeviceInfo>> GetListOfAudioDevicesAsync(CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<List<AudioDeviceInfo>>("getListOfAudioDevices", null, cancellationToken);
    }

    #endregion

    #region Activation Methods

    /// <summary>
    /// Activate Pianoteq with a serial number
    /// </summary>
    public async Task ActivateAsync(string serial, string deviceName, CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("activate", new[] { serial, deviceName }, cancellationToken);
    }

    /// <summary>
    /// Get activation information
    /// </summary>
    public async Task<ActivationInfo> GetActivationInfoAsync(CancellationToken cancellationToken = default)
    {
        var result = await SendRequestAsync<List<ActivationInfo>>("getActivationInfo", null, cancellationToken);
        return result.FirstOrDefault() ?? new ActivationInfo();
    }

    #endregion

    #region Application Control

    /// <summary>
    /// Quit Pianoteq immediately
    /// </summary>
    public async Task QuitAsync(CancellationToken cancellationToken = default)
    {
        await SendRequestAsync("quit", null, cancellationToken);
    }

    #endregion

    public void Dispose()
    {
        if (_disposeHttpClient)
        {
            _httpClient?.Dispose();
        }
    }
}
