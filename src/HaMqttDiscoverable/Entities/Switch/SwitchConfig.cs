using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for a <see cref="Switch"/> entity: a simple on/off control.</summary>
public sealed class SwitchConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "switch";

    /// <summary>The payload sent/expected to mean "on". Defaults to "ON".</summary>
    [JsonPropertyName("payload_on")]
    public string PayloadOn { get; set; } = "ON";

    /// <summary>The payload sent/expected to mean "off". Defaults to "OFF".</summary>
    [JsonPropertyName("payload_off")]
    public string PayloadOff { get; set; } = "OFF";

    /// <summary>
    /// When true, Home Assistant assumes the command succeeded and updates the UI immediately
    /// instead of waiting for a state update. Only use this if you don't call
    /// <see cref="Switch.PublishStateAsync(bool, bool, System.Threading.CancellationToken)"/> after handling a command.
    /// </summary>
    [JsonPropertyName("optimistic")]
    public bool? Optimistic { get; set; }
}
