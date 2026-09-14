namespace HaMqttDiscoverable.Entities;

/// <summary>The states a <see cref="Cover"/> can publish.</summary>
public enum CoverState
{
    /// <summary>Fully open.</summary>
    Open,
    /// <summary>Fully closed.</summary>
    Closed,
    /// <summary>In the process of opening.</summary>
    Opening,
    /// <summary>In the process of closing.</summary>
    Closing,
    /// <summary>Stopped partway.</summary>
    Stopped,
}
