namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for a <see cref="Time"/> entity: a time-of-day value that can be read and set.</summary>
public sealed class TimeConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "time";
}
