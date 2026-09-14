using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for a <see cref="Number"/> entity: a numeric value that can be read and set.</summary>
public sealed class NumberConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "number";

    /// <summary>The minimum value. Defaults to 1.</summary>
    [JsonPropertyName("min")]
    public double? Min { get; set; }

    /// <summary>The maximum value. Defaults to 100.</summary>
    [JsonPropertyName("max")]
    public double? Max { get; set; }

    /// <summary>The step between selectable values. Defaults to 1.</summary>
    [JsonPropertyName("step")]
    public double? Step { get; set; }

    /// <summary>The unit of measurement of the value, e.g. "%".</summary>
    [JsonPropertyName("unit_of_measurement")]
    public string? UnitOfMeasurement { get; set; }

    /// <summary>How the number should be displayed: "auto" (default), "box", or "slider".</summary>
    [JsonPropertyName("mode")]
    public string? Mode { get; set; }
}
