using System.Text.Json.Nodes;

namespace HaMqttDiscoverable.Entities;

/// <summary>
/// A still image published for Home Assistant to display, e.g. a camera snapshot or a
/// generated chart. Images are always published base64-encoded, so this works over the same
/// string-based MQTT publish path as every other entity in this library.
/// </summary>
public sealed class Image : HaEntity<ImageConfig>
{
    /// <inheritdoc />
    protected override bool HasStateTopic => false;

    /// <summary>Creates an image entity from the given config.</summary>
    public Image(HaMqttConnection connection, ImageConfig config)
        : base(connection, config)
    {
    }

    /// <summary>Publishes new image bytes (e.g. the contents of a .jpg/.png file).</summary>
    public Task PublishImageAsync(byte[] imageBytes, bool retain = true, CancellationToken cancellationToken = default)
        => PublishStateAsync(Convert.ToBase64String(imageBytes), retain, cancellationToken);

    /// <inheritdoc />
    protected override void EnrichDiscoveryPayload(JsonObject payload)
    {
        payload["image_topic"] = StateTopic;
        payload["image_encoding"] = "b64";
    }
}
