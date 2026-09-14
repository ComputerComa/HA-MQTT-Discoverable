namespace HaMqttDiscoverable.Entities;

/// <summary>A value picked from a fixed list of options, e.g. an operating mode.</summary>
public sealed class Select : HaEntity<SelectConfig>
{
    /// <inheritdoc />
    protected override bool SupportsCommands => true;

    /// <summary>Raised when Home Assistant sends a newly selected option.</summary>
    public event Func<string, Task>? CommandReceived;

    /// <summary>Creates a select from the given config, optionally subscribing <paramref name="onCommand"/> to <see cref="CommandReceived"/>.</summary>
    public Select(HaMqttConnection connection, SelectConfig config, Func<string, Task>? onCommand = null)
        : base(connection, config)
    {
        if (config.Options is null || config.Options.Count == 0)
        {
            throw new ArgumentException("SelectConfig.Options must contain at least one option.", nameof(config));
        }

        if (onCommand is not null)
        {
            CommandReceived += onCommand;
        }
    }

    /// <inheritdoc />
    protected override async Task OnCommandReceivedAsync(string payload)
    {
        if (CommandReceived is not null)
        {
            await CommandReceived.Invoke(payload).ConfigureAwait(false);
        }
    }
}
