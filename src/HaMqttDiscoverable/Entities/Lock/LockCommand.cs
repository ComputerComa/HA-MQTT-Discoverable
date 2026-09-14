namespace HaMqttDiscoverable.Entities;

/// <summary>The commands Home Assistant can send to a <see cref="LockEntity"/>.</summary>
public enum LockCommand
{
    /// <summary>Lock.</summary>
    Lock,
    /// <summary>Unlock.</summary>
    Unlock,

    /// <summary>Only sent if <see cref="LockConfig.PayloadOpen"/> was configured.</summary>
    Open,
}
