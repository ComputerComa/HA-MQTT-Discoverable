namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for a <see cref="Notify"/> entity: a target Home Assistant can send notification messages to.</summary>
public sealed class NotifyConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "notify";
}
