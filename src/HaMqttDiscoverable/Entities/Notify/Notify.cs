namespace HaMqttDiscoverable.Entities;

/// <summary>A notification target Home Assistant can send messages to (e.g. via the `notify.send_message` action).</summary>
public sealed class Notify : HaEntity<NotifyConfig>
{
    /// <inheritdoc />
    protected override bool SupportsCommands => true;

    /// <inheritdoc />
    protected override bool HasStateTopic => false;

    /// <summary>Raised when Home Assistant sends a message to this target.</summary>
    public event Func<string, Task>? MessageReceived;

    /// <summary>Creates a notify target from the given config, optionally subscribing `onMessage` to `MessageReceived`.</summary>
    public Notify(HaMqttConnection connection, NotifyConfig config, Func<string, Task>? onMessage = null)
        : base(connection, config)
    {
        if (onMessage is not null)
        {
            MessageReceived += onMessage;
        }
    }

    /// <inheritdoc />
    protected override async Task OnCommandReceivedAsync(string payload)
    {
        if (MessageReceived is not null)
        {
            await MessageReceived.Invoke(payload).ConfigureAwait(false);
        }
    }
}
