using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for an <see cref="Image"/> entity: publishes a still image (e.g. a snapshot or a generated chart) for Home Assistant to display.</summary>
public sealed class ImageConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "image";

    /// <summary>The MIME type of the published image, e.g. "image/jpeg" (the default) or "image/png".</summary>
    [JsonPropertyName("content_type")]
    public string ContentType { get; set; } = "image/jpeg";
}
