using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for a <see cref="Scene"/> entity: a stateless trigger, like <see cref="Button"/> but grouped under Home Assistant's "scene" domain.</summary>
public sealed class SceneConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "scene";

    /// <summary>The payload sent when the scene is activated. Defaults to "ON".</summary>
    [JsonPropertyName("payload_on")]
    public string PayloadOn { get; set; } = "ON";
}
