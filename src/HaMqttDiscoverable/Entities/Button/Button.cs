namespace HaMqttDiscoverable.Entities;

/// <summary>A stateless, momentary action exposed to Home Assistant, e.g. "Restart" or "Identify".</summary>
/// <example>
/// <code>
/// var restart = new Button(connection, new ButtonConfig
/// {
///     Name = "Restart",
///     UniqueId = "device-restart",
///     Device = device,
/// });
/// restart.Pressed += async () => await RestartAsync();
/// await restart.PublishDiscoveryAsync();
/// </code>
/// </example>
public sealed class Button : HaEntity<ButtonConfig>
{
    /// <inheritdoc />
    protected override bool SupportsCommands => true;

    /// <inheritdoc />
    protected override bool HasStateTopic => false;

    /// <summary>Raised whenever Home Assistant reports the button was pressed.</summary>
    public event Func<Task>? Pressed;

    /// <summary>Creates a button from the given config, optionally subscribing <paramref name="onPressed"/> to <see cref="Pressed"/>.</summary>
    public Button(HaMqttConnection connection, ButtonConfig config, Func<Task>? onPressed = null)
        : base(connection, config)
    {
        if (onPressed is not null)
        {
            Pressed += onPressed;
        }
    }

    /// <inheritdoc />
    protected override async Task OnCommandReceivedAsync(string payload)
    {
        if (Pressed is not null)
        {
            await Pressed.Invoke().ConfigureAwait(false);
        }
    }
}
