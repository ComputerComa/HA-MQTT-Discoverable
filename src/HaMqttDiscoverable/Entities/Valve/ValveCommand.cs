namespace HaMqttDiscoverable.Entities;

/// <summary>The commands Home Assistant can send to a <see cref="Valve"/> that doesn't report a position.</summary>
public enum ValveCommand
{
    /// <summary>Open the valve.</summary>
    Open,
    /// <summary>Close the valve.</summary>
    Close,

    /// <summary>Only sent if <see cref="ValveConfig.PayloadStop"/> was configured.</summary>
    Stop,
}
