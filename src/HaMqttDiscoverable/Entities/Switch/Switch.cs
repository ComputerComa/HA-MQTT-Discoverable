namespace HaMqttDiscoverable.Entities;

/// <summary>A simple on/off control, e.g. a relay or a smart plug.</summary>
/// <example>
/// <code>
/// var mySwitch = new Switch(connection, new SwitchConfig
/// {
///     Name = "Relay",
///     UniqueId = "garage-relay",
///     Device = device,
/// });
/// mySwitch.CommandReceived += async isOn =>
/// {
///     SetRelay(isOn);
///     await mySwitch.PublishStateAsync(isOn);
/// };
/// await mySwitch.PublishDiscoveryAsync();
/// </code>
/// </example>
public sealed class Switch : HaEntity<SwitchConfig>
{
    /// <inheritdoc />
    protected override bool SupportsCommands => true;

    /// <summary>Raised when Home Assistant sends a command to turn this switch on or off.</summary>
    public event Func<bool, Task>? CommandReceived;

    /// <summary>Creates a switch from the given config, optionally subscribing <paramref name="onCommand"/> to <see cref="CommandReceived"/>.</summary>
    public Switch(HaMqttConnection connection, SwitchConfig config, Func<bool, Task>? onCommand = null)
        : base(connection, config)
    {
        if (onCommand is not null)
        {
            CommandReceived += onCommand;
        }
    }

    /// <summary>Publishes the switch's current on/off state.</summary>
    public Task PublishStateAsync(bool isOn, bool retain = true, CancellationToken cancellationToken = default)
        => PublishStateAsync(isOn ? Config.PayloadOn : Config.PayloadOff, retain, cancellationToken);

    /// <inheritdoc />
    protected override async Task OnCommandReceivedAsync(string payload)
    {
        if (CommandReceived is null)
        {
            return;
        }

        var isOn = string.Equals(payload, Config.PayloadOn, StringComparison.Ordinal);
        await CommandReceived.Invoke(isOn).ConfigureAwait(false);
    }
}
