using System.Text.Json.Nodes;

namespace HaMqttDiscoverable.Entities;

/// <summary>
/// A live-updating image feed - publish a new frame whenever one is available and Home
/// Assistant's camera card shows the latest one. Frames are always published base64-encoded,
/// so this works over the same string-based MQTT publish path as every other entity in this
/// library.
/// </summary>
public sealed class Camera : HaEntity<CameraConfig>
{
    /// <inheritdoc />
    protected override bool HasStateTopic => false;

    /// <summary>Creates a camera from the given config.</summary>
    public Camera(HaMqttConnection connection, CameraConfig config)
        : base(connection, config)
    {
    }

    /// <summary>Publishes a new frame (e.g. the contents of a .jpg snapshot).</summary>
    public Task PublishFrameAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
        => PublishStateAsync(Convert.ToBase64String(imageBytes), retain: false, cancellationToken);

    /// <inheritdoc />
    protected override void EnrichDiscoveryPayload(JsonObject payload)
    {
        payload["topic"] = StateTopic;
        payload["image_encoding"] = "b64";
    }
}
