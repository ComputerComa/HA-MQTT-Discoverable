namespace HaMqttDiscoverable.Entities;

/// <summary>A free-form string value that can be read and set from Home Assistant.</summary>
public sealed class Text : HaEntity<TextConfig>
{
    /// <inheritdoc />
    protected override bool SupportsCommands => true;

    /// <summary>Raised when Home Assistant sends a new value.</summary>
    public event Func<string, Task>? CommandReceived;

    /// <summary>Creates a text entity from the given config, optionally subscribing <paramref name="onCommand"/> to <see cref="CommandReceived"/>.</summary>
    public Text(HaMqttConnection connection, TextConfig config, Func<string, Task>? onCommand = null)
        : base(connection, config)
    {
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
