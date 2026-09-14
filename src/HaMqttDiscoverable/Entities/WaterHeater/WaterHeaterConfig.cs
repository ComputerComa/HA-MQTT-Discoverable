using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for a <see cref="WaterHeater"/> entity: an operating mode plus a target/current temperature.</summary>
public sealed class WaterHeaterConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "water_heater";

    /// <summary>
    /// The operating modes this water heater supports. Defaults to Home Assistant's standard
    /// set: "eco", "electric", "gas", "heat_pump", "high_demand", "performance", "off".
    /// </summary>
    [JsonPropertyName("modes")]
    public List<string> Modes { get; set; } = new() { "eco", "electric", "gas", "heat_pump", "high_demand", "performance", "off" };

    /// <summary>The payload sent/expected to mean "on". Defaults to "ON".</summary>
    [JsonPropertyName("payload_on")]
    public string PayloadOn { get; set; } = "ON";

    /// <summary>The payload sent/expected to mean "off". Defaults to "OFF".</summary>
    [JsonPropertyName("payload_off")]
    public string PayloadOff { get; set; } = "OFF";

    /// <summary>When true, a separate power on/off toggle (distinct from setting the mode to "off") is added - see <see cref="WaterHeater.PowerCommandReceived"/>.</summary>
    [JsonIgnore]
    public bool SupportsPower { get; set; }

    /// <summary>The minimum target temperature.</summary>
    [JsonPropertyName("min_temp")]
    public double? TempMin { get; set; }

    /// <summary>The maximum target temperature.</summary>
    [JsonPropertyName("max_temp")]
    public double? TempMax { get; set; }

    /// <summary>The target temperature to suggest when the entity is first added.</summary>
    [JsonPropertyName("initial")]
    public double? TempInitial { get; set; }
}
