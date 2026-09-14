namespace HaMqttDiscoverable.Entities;

/// <summary>The states a non-position-reporting <see cref="Valve"/> can publish.</summary>
public enum ValveState
{
    /// <summary>Fully open.</summary>
    Open,
    /// <summary>Fully closed.</summary>
    Closed,
    /// <summary>In the process of opening.</summary>
    Opening,
    /// <summary>In the process of closing.</summary>
    Closing,
}
