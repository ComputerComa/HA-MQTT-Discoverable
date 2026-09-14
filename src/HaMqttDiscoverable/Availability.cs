using System.Text.Json.Serialization;

namespace HaMqttDiscoverable;

/// <summary>
/// Describes an MQTT topic Home Assistant should watch to know whether an entity (or device)
/// is currently online. When omitted, entities default to using the connection's own
/// last-will/birth mechanism managed by <see cref="HaMqttConnection"/>.
/// </summary>
public sealed class Availability
{
    /// <summary>The MQTT topic Home Assistant subscribes to for availability updates.</summary>
    [JsonPropertyName("topic")]
    public string Topic { get; set; } = string.Empty;

    /// <summary>The payload that indicates the entity/device is available. Defaults to "online".</summary>
    [JsonPropertyName("payload_available")]
    public string PayloadAvailable { get; set; } = "online";

    /// <summary>The payload that indicates the entity/device is unavailable. Defaults to "offline".</summary>
    [JsonPropertyName("payload_not_available")]
    public string PayloadNotAvailable { get; set; } = "offline";

    /// <summary>Defines a [JSONPath](https://goessner.net/articles/JsonPath/) to extract the availability value from a JSON payload.</summary>
    [JsonPropertyName("value_template")]
    public string? ValueTemplate { get; set; }

    /// <summary>Creates an empty availability config. Set <see cref="Topic"/> before use.</summary>
    public Availability() { }

    /// <summary>Creates an availability config for the given topic, using Home Assistant's default "online"/"offline" payloads.</summary>
    public Availability(string topic)
    {
        Topic = topic;
    }
}
