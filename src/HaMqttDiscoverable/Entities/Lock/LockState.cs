namespace HaMqttDiscoverable.Entities;

/// <summary>The possible states of a <see cref="LockEntity"/>, for use with <see cref="LockEntity.PublishStateAsync(LockState, bool, CancellationToken)"/>.</summary>
public enum LockState
{
    /// <summary>Locked.</summary>
    Locked,
    /// <summary>Unlocked.</summary>
    Unlocked,
    /// <summary>In the process of locking.</summary>
    Locking,
    /// <summary>In the process of unlocking.</summary>
    Unlocking,
    /// <summary>Jammed.</summary>
    Jammed,
    /// <summary>Open (e.g. a latch).</summary>
    Open,
    /// <summary>In the process of opening.</summary>
    Opening,
}
