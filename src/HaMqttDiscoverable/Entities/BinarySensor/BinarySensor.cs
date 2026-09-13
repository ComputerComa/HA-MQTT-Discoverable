namespace HaMqttDiscoverable.Entities;

/// <summary>A read-only two-state value reported to Home Assistant, e.g. a door sensor or a motion detector.</summary>
public sealed class BinarySensor : HaEntity<BinarySensorConfig>
{
    /// <summary>Creates a binary sensor from the given config.</summary>
    public BinarySensor(HaMqttConnection connection, BinarySensorConfig config)
        : base(connection, config)
    {
    }

    /// <summary>Publishes the sensor's "on"/"off" state.</summary>
    public Task PublishStateAsync(bool isOn, bool retain = true, CancellationToken cancellationToken = default)
        => PublishStateAsync(isOn ? Config.PayloadOn : Config.PayloadOff, retain, cancellationToken);
}
