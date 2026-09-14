namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for a <see cref="Date"/> entity: a calendar date that can be read and set.</summary>
public sealed class DateConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "date";
}
