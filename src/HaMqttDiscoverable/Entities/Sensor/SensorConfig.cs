using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for a read-only <see cref="Sensor"/> entity.</summary>
public sealed class SensorConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "sensor";

    /// <summary>The unit of measurement of the sensor's value, e.g. "°C" or "kWh".</summary>
    [JsonPropertyName("unit_of_measurement")]
    public string? UnitOfMeasurement { get; set; }

    /// <summary>
    /// Tells Home Assistant how to interpret readings over time, e.g. "measurement",
    /// "total", or "total_increasing" (for ever-increasing counters like energy usage).
    /// </summary>
    [JsonPropertyName("state_class")]
    public string? StateClass { get; set; }

    /// <summary>
    /// If set, the sensor becomes "unknown" when no update is received within this many
    /// seconds - useful for detecting a stalled or offline sensor.
    /// </summary>
    [JsonPropertyName("expire_after")]
    public int? ExpireAfter { get; set; }

    /// <summary>Number of decimals to display for numeric values.</summary>
    [JsonPropertyName("suggested_display_precision")]
    public int? SuggestedDisplayPrecision { get; set; }
}
