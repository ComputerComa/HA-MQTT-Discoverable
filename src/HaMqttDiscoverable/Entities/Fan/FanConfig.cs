using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>
/// Configuration for a <see cref="Fan"/> entity: on/off, optionally with speed
/// (<see cref="SupportsPercentage"/>) and/or named presets (<see cref="PresetModes"/>).
/// Oscillation and direction control aren't exposed yet - see <see cref="Fan"/>'s remarks.
/// </summary>
public sealed class FanConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "fan";

    /// <summary>The payload sent/expected to mean "on". Defaults to "ON".</summary>
    [JsonPropertyName("payload_on")]
    public string PayloadOn { get; set; } = "ON";

    /// <summary>The payload sent/expected to mean "off". Defaults to "OFF".</summary>
    [JsonPropertyName("payload_off")]
    public string PayloadOff { get; set; } = "OFF";

    /// <summary>When true, this fan reports/accepts a speed percentage in addition to on/off.</summary>
    [JsonIgnore]
    public bool SupportsPercentage { get; set; }

    /// <summary>The lowest non-zero speed step, e.g. 1 (the default) for a percentage-based fan, or the number of discrete speeds otherwise.</summary>
    [JsonPropertyName("speed_range_min")]
    public int SpeedRangeMin { get; set; } = 1;

    /// <summary>The highest speed step. Defaults to 100.</summary>
    [JsonPropertyName("speed_range_max")]
    public int SpeedRangeMax { get; set; } = 100;

    /// <summary>
    /// The list of named presets this fan supports (e.g. "eco", "sleep", "auto"). When non-empty,
    /// a preset mode command/state topic pair is added - see <see cref="Fan.PresetModeCommandReceived"/>.
    /// </summary>
    [JsonIgnore]
    public List<string> PresetModes { get; set; } = new();
}
