namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for a <see cref="Camera"/> entity: a live-updating image feed, distinct from <see cref="Image"/> which is for a single still image.</summary>
public sealed class CameraConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "camera";
}
