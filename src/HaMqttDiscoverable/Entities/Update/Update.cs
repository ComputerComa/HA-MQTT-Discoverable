namespace HaMqttDiscoverable.Entities;

/// <summary>Reports available firmware/software updates for a device, with an optional "Install" action.</summary>
public sealed class Update : HaEntity<UpdateConfig>
{
    /// <inheritdoc />
    protected override bool SupportsCommands => true;

    /// <summary>Raised when the user clicks "Install" in Home Assistant.</summary>
    public event Func<Task>? InstallRequested;

    /// <summary>Creates an update entity from the given config, optionally subscribing `onInstallRequested` to `InstallRequested`.</summary>
    public Update(HaMqttConnection connection, UpdateConfig config, Func<Task>? onInstallRequested = null)
        : base(connection, config)
    {
        if (onInstallRequested is not null)
        {
            InstallRequested += onInstallRequested;
        }
    }

    /// <summary>Publishes the current/available version info.</summary>
    public Task PublishStateAsync(UpdateState state, bool retain = true, CancellationToken cancellationToken = default)
        => PublishStateAsync<UpdateState>(state, retain, cancellationToken);

    /// <inheritdoc />
    protected override async Task OnCommandReceivedAsync(string payload)
    {
        if (InstallRequested is not null)
        {
            await InstallRequested.Invoke().ConfigureAwait(false);
        }
    }
}
