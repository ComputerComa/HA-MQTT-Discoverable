using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for a <see cref="Button"/> entity: a stateless, momentary action.</summary>
public sealed class ButtonConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "button";

    /// <summary>The payload sent when the button is pressed. Defaults to "PRESS".</summary>
    [JsonPropertyName("payload_press")]
    public string PayloadPress { get; set; } = "PRESS";
}
