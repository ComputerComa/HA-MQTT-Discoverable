namespace HaMqttDiscoverable.Entities;

/// <summary>The commands Home Assistant can send to a <see cref="Cover"/>.</summary>
public enum CoverCommand
{
    /// <summary>Open the cover.</summary>
    Open,
    /// <summary>Close the cover.</summary>
    Close,
    /// <summary>Stop mid-movement.</summary>
    Stop,
}
