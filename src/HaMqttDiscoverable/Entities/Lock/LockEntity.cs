namespace HaMqttDiscoverable.Entities;

/// <summary>A lock entity that can be locked/unlocked (and optionally opened) from Home Assistant.</summary>
public sealed class LockEntity : HaEntity<LockConfig>
{
    /// <inheritdoc />
    protected override bool SupportsCommands => true;

    /// <summary>Raised when Home Assistant sends a lock/unlock/open command.</summary>
    public event Func<LockCommand, Task>? CommandReceived;

    /// <summary>Creates a lock entity from the given config, optionally subscribing `onCommand` to `CommandReceived`.</summary>
    public LockEntity(HaMqttConnection connection, LockConfig config, Func<LockCommand, Task>? onCommand = null)
        : base(connection, config)
    {
        if (onCommand is not null)
        {
            CommandReceived += onCommand;
        }
    }

    /// <summary>Publishes the lock's current state.</summary>
    public Task PublishStateAsync(LockState state, bool retain = true, CancellationToken cancellationToken = default)
        => PublishStateAsync(state switch
        {
            LockState.Locked => Config.StateLocked,
            LockState.Unlocked => Config.StateUnlocked,
            LockState.Locking => Config.StateLocking,
            LockState.Unlocking => Config.StateUnlocking,
            LockState.Jammed => Config.StateJammed,
            LockState.Open => Config.StateOpen,
            LockState.Opening => Config.StateOpening,
            _ => throw new ArgumentOutOfRangeException(nameof(state)),
        }, retain, cancellationToken);

    /// <inheritdoc />
    protected override async Task OnCommandReceivedAsync(string payload)
    {
        if (CommandReceived is null)
        {
            return;
        }

        LockCommand? command = payload switch
        {
            _ when payload == Config.PayloadLock => LockCommand.Lock,
            _ when payload == Config.PayloadUnlock => LockCommand.Unlock,
            _ when Config.PayloadOpen is not null && payload == Config.PayloadOpen => LockCommand.Open,
            _ => null,
        };

        if (command is not null)
        {
            await CommandReceived.Invoke(command.Value).ConfigureAwait(false);
        }
    }
}
