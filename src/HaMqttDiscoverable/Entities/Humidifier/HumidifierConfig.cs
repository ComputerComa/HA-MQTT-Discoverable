using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for a <see cref="Humidifier"/> entity: on/off with a target humidity, optionally with named modes.</summary>
public sealed class HumidifierConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "humidifier";

    /// <summary>The payload sent/expected to mean "on". Defaults to "ON".</summary>
    [JsonPropertyName("payload_on")]
    public string PayloadOn { get; set; } = "ON";

    /// <summary>The payload sent/expected to mean "off". Defaults to "OFF".</summary>
    [JsonPropertyName("payload_off")]
    public string PayloadOff { get; set; } = "OFF";

    /// <summary>The minimum target humidity, 0-100. Defaults to 0.</summary>
    [JsonPropertyName("min_humidity")]
    public double TargetHumidityMin { get; set; }

    /// <summary>The maximum target humidity, 0-100. Defaults to 100.</summary>
    [JsonPropertyName("max_humidity")]
    public double TargetHumidityMax { get; set; } = 100;

    /// <summary>
    /// The list of named modes this humidifier supports (e.g. "auto", "sleep", "away"). When
    /// non-empty, a mode command/state topic pair is added - see <see cref="Humidifier.ModeCommandReceived"/>.
    /// </summary>
    [JsonIgnore]
    public List<string> Modes { get; set; } = new();
}
