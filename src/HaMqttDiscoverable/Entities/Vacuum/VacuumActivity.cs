namespace HaMqttDiscoverable.Entities;

/// <summary>The activity values Home Assistant's vacuum component understands, for use with <see cref="Vacuum.PublishStateAsync"/>.</summary>
public enum VacuumActivity
{
    /// <summary>On, but not currently cleaning.</summary>
    Idle,
    /// <summary>Docked.</summary>
    Docked,
    /// <summary>In an error state, needs assistance.</summary>
    Error,
    /// <summary>Paused during cleaning.</summary>
    Paused,
    /// <summary>Returning to the dock.</summary>
    Returning,
    /// <summary>Cleaning.</summary>
    Cleaning,
}
