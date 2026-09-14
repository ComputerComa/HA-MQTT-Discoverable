namespace HaMqttDiscoverable.Entities;

/// <summary>The activity values Home Assistant's lawn mower component understands, for use with <see cref="LawnMower.PublishStateAsync"/>.</summary>
public enum LawnMowerActivity
{
    /// <summary>In an error state, needs assistance.</summary>
    Error,
    /// <summary>Paused during activity.</summary>
    Paused,
    /// <summary>Mowing.</summary>
    Mowing,
    /// <summary>Docked.</summary>
    Docked,
    /// <summary>Returning to the dock.</summary>
    Returning,
    /// <summary>On, but not currently mowing.</summary>
    Idle,
}
