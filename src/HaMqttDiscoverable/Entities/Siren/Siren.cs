namespace HaMqttDiscoverable.Entities;

/// <summary>
/// An on/off alarm sounder. <see cref="SirenConfig.AvailableTones"/>, <see cref="SirenConfig.SupportDuration"/>
/// and <see cref="SirenConfig.SupportVolumeSet"/> only affect what Home Assistant's UI shows -
/// without a `command_template` (which this library doesn't expose, matching its other entities),
/// Home Assistant sends only the plain on/off payload over MQTT, so tone/duration/volume choices
/// made in the UI aren't transmitted. If you need them, read them from the service call on the
/// Home Assistant side via an automation, or use the raw <see cref="HaMqttConnection"/> APIs.
/// </summary>
public sealed class Siren : HaEntity<SirenConfig>
{
    /// <inheritdoc />
    protected override bool SupportsCommands => true;

    /// <summary>Raised when Home Assistant sends a command to turn the siren on or off.</summary>
    public event Func<bool, Task>? CommandReceived;

    /// <summary>Creates a siren from the given config, optionally subscribing `onCommand` to `CommandReceived`.</summary>
    public Siren(HaMqttConnection connection, SirenConfig config, Func<bool, Task>? onCommand = null)
        : base(connection, config)
    {
        if (onCommand is not null)
        {
            CommandReceived += onCommand;
        }
    }

    /// <summary>Publishes the siren's current on/off state.</summary>
    public Task PublishStateAsync(bool isOn, bool retain = true, CancellationToken cancellationToken = default)
        => PublishStateAsync(isOn ? (Config.StateOn ?? Config.PayloadOn) : (Config.StateOff ?? Config.PayloadOff), retain, cancellationToken);

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
