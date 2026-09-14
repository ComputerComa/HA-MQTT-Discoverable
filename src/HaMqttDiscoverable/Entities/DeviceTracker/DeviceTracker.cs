namespace HaMqttDiscoverable.Entities;

/// <summary>
/// A read-only presence report - "home", "away", or a custom zone name. For full GPS tracking
/// (latitude/longitude/accuracy), publish JSON with a <c>json_attributes_topic</c> yourself via
/// the underlying <see cref="HaMqttConnection"/>; this entity covers the common home/away case.
/// </summary>
public sealed class DeviceTracker : HaEntity<DeviceTrackerConfig>
{
    /// <summary>Creates a device tracker from the given config.</summary>
    public DeviceTracker(HaMqttConnection connection, DeviceTrackerConfig config)
        : base(connection, config)
    {
    }

    /// <summary>Publishes "home" or "away".</summary>
    public Task PublishStateAsync(bool isHome, bool retain = true, CancellationToken cancellationToken = default)
        => PublishStateAsync(isHome ? Config.PayloadHome : Config.PayloadNotHome, retain, cancellationToken);

    /// <summary>Publishes a custom zone name (anything other than the configured home/away payloads).</summary>
    public Task PublishZoneAsync(string zoneName, bool retain = true, CancellationToken cancellationToken = default)
        => PublishStateAsync(zoneName, retain, cancellationToken);
}
