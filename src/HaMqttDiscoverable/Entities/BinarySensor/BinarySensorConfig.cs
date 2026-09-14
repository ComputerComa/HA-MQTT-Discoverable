using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for a read-only <see cref="BinarySensor"/> entity, reporting one of two states.</summary>
public sealed class BinarySensorConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "binary_sensor";

    /// <summary>The payload that means "on"/"detected"/"true". Defaults to "ON".</summary>
    [JsonPropertyName("payload_on")]
    public string PayloadOn { get; set; } = "ON";

    /// <summary>The payload that means "off"/"clear"/"false". Defaults to "OFF".</summary>
    [JsonPropertyName("payload_off")]
    public string PayloadOff { get; set; } = "OFF";

    /// <summary>
    /// If set, the sensor becomes "unknown" when no update is received within this many
    /// seconds.
    /// </summary>
    [JsonPropertyName("expire_after")]
    public int? ExpireAfter { get; set; }
}
