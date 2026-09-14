using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>
/// Configuration for a <see cref="Climate"/> entity: an HVAC mode plus a target temperature,
/// with optional fan mode, swing mode, preset mode, humidity, a separate power toggle, a
/// heating/cooling range instead of a single target, and a read-only "current action" report.
/// </summary>
public sealed class ClimateConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "climate";

    /// <summary>The HVAC modes this device supports. Defaults to "auto", "off", "cool", "heat", "dry", "fan_only".</summary>
    [JsonPropertyName("modes")]
    public List<string> Modes { get; set; } = new() { "auto", "off", "cool", "heat", "dry", "fan_only" };

    /// <summary>The step between selectable target temperatures. Defaults to 1.0.</summary>
    [JsonPropertyName("temp_step")]
    public double TempStep { get; set; } = 1.0;

    /// <summary>The minimum target temperature.</summary>
    [JsonPropertyName("min_temp")]
    public double? TempMin { get; set; }

    /// <summary>The maximum target temperature.</summary>
    [JsonPropertyName("max_temp")]
    public double? TempMax { get; set; }

    /// <summary>The target temperature to suggest when the entity is first added.</summary>
    [JsonPropertyName("initial")]
    public double? TempInitial { get; set; }

    /// <summary>
    /// When true, this device is set to a heating/cooling range (two target temperatures) via
    /// <see cref="Climate.TargetTemperatureRangeCommandReceived"/> instead of a single target.
    /// </summary>
    [JsonIgnore]
    public bool SupportsTemperatureRange { get; set; }

    /// <summary>When true, adds a read-only report of what the equipment is actually doing right now - see <see cref="Climate.PublishActionAsync"/>.</summary>
    [JsonIgnore]
    public bool SupportsAction { get; set; }

    /// <summary>When true, adds fan mode control - see <see cref="Climate.FanModeCommandReceived"/>.</summary>
    [JsonIgnore]
    public bool SupportsFanMode { get; set; }

    /// <summary>The fan modes this device supports, if <see cref="SupportsFanMode"/> is true. Defaults to "auto", "low", "medium", "high".</summary>
    [JsonPropertyName("fan_modes")]
    public List<string> FanModes { get; set; } = new() { "auto", "low", "medium", "high" };

    /// <summary>When true, adds swing mode control - see <see cref="Climate.SwingModeCommandReceived"/>.</summary>
    [JsonIgnore]
    public bool SupportsSwingMode { get; set; }

    /// <summary>The swing modes this device supports, if <see cref="SupportsSwingMode"/> is true. Defaults to "on", "off".</summary>
    [JsonPropertyName("swing_modes")]
    public List<string> SwingModes { get; set; } = new() { "on", "off" };

    /// <summary>When true, adds named preset control (e.g. "eco", "away") - see <see cref="Climate.PresetModeCommandReceived"/>.</summary>
    [JsonIgnore]
    public bool SupportsPresetMode { get; set; }

    /// <summary>The presets this device supports. Required (non-empty) if <see cref="SupportsPresetMode"/> is true.</summary>
    [JsonPropertyName("preset_modes")]
    public List<string> PresetModes { get; set; } = new();

    /// <summary>When true, adds target/current humidity - see <see cref="Climate.TargetHumidityCommandReceived"/>.</summary>
    [JsonIgnore]
    public bool SupportsHumidity { get; set; }

    /// <summary>The minimum target humidity, 0-100, if <see cref="SupportsHumidity"/> is true. Defaults to 0.</summary>
    [JsonPropertyName("min_humidity")]
    public double HumidityMin { get; set; }

    /// <summary>The maximum target humidity, 0-100, if <see cref="SupportsHumidity"/> is true. Defaults to 100.</summary>
    [JsonPropertyName("max_humidity")]
    public double HumidityMax { get; set; } = 100;

    /// <summary>
    /// When true, adds a separate on/off power toggle independent of the HVAC mode - see
    /// <see cref="Climate.PowerCommandReceived"/>.
    /// </summary>
    [JsonIgnore]
    public bool SupportsPower { get; set; }

    /// <summary>The payload sent/expected to mean "on", if <see cref="SupportsPower"/> is true. Defaults to "ON".</summary>
    [JsonPropertyName("payload_on")]
    public string PayloadOn { get; set; } = "ON";

    /// <summary>The payload sent/expected to mean "off", if <see cref="SupportsPower"/> is true. Defaults to "OFF".</summary>
    [JsonPropertyName("payload_off")]
    public string PayloadOff { get; set; } = "OFF";
}
