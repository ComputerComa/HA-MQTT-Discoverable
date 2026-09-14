using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>The JSON body used for both state updates and commands under Home Assistant's MQTT JSON light schema.</summary>
public sealed class LightState
{
    /// <summary>"ON" or "OFF".</summary>
    [JsonPropertyName("state")]
    public string State { get; set; } = "OFF";

    /// <summary>Brightness, from 0 to <see cref="LightConfig.BrightnessScale"/> (255 by default). Null when not reported/changed.</summary>
    [JsonPropertyName("brightness")]
    public int? Brightness { get; set; }
}
