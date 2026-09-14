namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for a <see cref="LawnMower"/> entity: start/pause/dock controls plus an activity report.</summary>
public sealed class LawnMowerConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "lawn_mower";
}
