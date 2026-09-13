using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>
/// Configuration for a <see cref="Light"/> entity. Uses Home Assistant's MQTT JSON light
/// schema, so state and commands are exchanged as small JSON objects rather than plain strings.
/// </summary>
public sealed class LightConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "light";

    /// <summary>Whether this light supports adjustable brightness.</summary>
    [JsonPropertyName("brightness")]
    public bool SupportsBrightness { get; set; }

    /// <summary>The maximum brightness value accepted/reported, e.g. 255 (the default) or 100.</summary>
    [JsonPropertyName("brightness_scale")]
    public int? BrightnessScale { get; set; }

    /// <summary>
    /// When true, Home Assistant assumes a command succeeded and updates the UI immediately
    /// instead of waiting for a state update.
    /// </summary>
    [JsonPropertyName("optimistic")]
    public bool? Optimistic { get; set; }
}
