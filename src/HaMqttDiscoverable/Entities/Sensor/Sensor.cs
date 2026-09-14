namespace HaMqttDiscoverable.Entities;

/// <summary>
/// A read-only value reported to Home Assistant, e.g. a temperature reading or a counter.
/// </summary>
/// <example>
/// <code>
/// var sensor = new Sensor(connection, new SensorConfig
/// {
///     Name = "Temperature",
///     UniqueId = "weather-station-temperature",
///     Device = device,
///     UnitOfMeasurement = "°C",
///     DeviceClass = "temperature",
///     StateClass = "measurement",
/// });
/// await sensor.PublishDiscoveryAsync();
/// await sensor.PublishStateAsync("21.5");
/// </code>
/// </example>
public sealed class Sensor : HaEntity<SensorConfig>
{
    /// <summary>Creates a sensor from the given config.</summary>
    public Sensor(HaMqttConnection connection, SensorConfig config)
        : base(connection, config)
    {
    }
}
