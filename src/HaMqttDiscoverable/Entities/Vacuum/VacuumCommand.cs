namespace HaMqttDiscoverable.Entities;

/// <summary>The commands Home Assistant can send to a <see cref="Vacuum"/>.</summary>
public enum VacuumCommand
{
    /// <summary>Start (or resume) cleaning.</summary>
    Start,
    /// <summary>Pause cleaning.</summary>
    Pause,
    /// <summary>Stop cleaning.</summary>
    Stop,
    /// <summary>Return to the dock.</summary>
    ReturnToBase,
    /// <summary>Clean the current spot.</summary>
    CleanSpot,
    /// <summary>Locate (e.g. play a sound).</summary>
    Locate,
}
