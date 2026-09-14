namespace HaMqttDiscoverable.Entities;

/// <summary>A security alarm panel that can be armed/disarmed/triggered from Home Assistant.</summary>
/// <example>
/// <code>
/// var alarm = new AlarmControlPanel(connection, new AlarmControlPanelConfig
/// {
///     Name = "Alarm",
///     UniqueId = "house-alarm",
///     Device = device,
/// });
/// alarm.CommandReceived += async command =>
/// {
///     ApplyToHardware(command);
///     await alarm.PublishStateAsync(AlarmControlPanelState.ArmedAway);
/// };
/// await alarm.PublishDiscoveryAsync();
/// </code>
/// </example>
public sealed class AlarmControlPanel : HaEntity<AlarmControlPanelConfig>
{
    /// <inheritdoc />
    protected override bool SupportsCommands => true;

    /// <summary>Raised when Home Assistant sends an arm/disarm/trigger command.</summary>
    public event Func<AlarmControlPanelCommand, Task>? CommandReceived;

    /// <summary>Creates an alarm control panel from the given config, optionally subscribing `onCommand` to `CommandReceived`.</summary>
    public AlarmControlPanel(HaMqttConnection connection, AlarmControlPanelConfig config, Func<AlarmControlPanelCommand, Task>? onCommand = null)
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
        if (CommandReceived is null)
        {
            return;
        }

        AlarmControlPanelCommand? command = payload switch
        {
            _ when payload == Config.PayloadArmAway => AlarmControlPanelCommand.ArmAway,
            _ when payload == Config.PayloadArmHome => AlarmControlPanelCommand.ArmHome,
            _ when payload == Config.PayloadArmNight => AlarmControlPanelCommand.ArmNight,
            _ when payload == Config.PayloadArmVacation => AlarmControlPanelCommand.ArmVacation,
            _ when payload == Config.PayloadArmCustomBypass => AlarmControlPanelCommand.ArmCustomBypass,
            _ when payload == Config.PayloadDisarm => AlarmControlPanelCommand.Disarm,
            _ when payload == Config.PayloadTrigger => AlarmControlPanelCommand.Trigger,
            _ => null,
        };

        if (command is not null)
        {
            await CommandReceived.Invoke(command.Value).ConfigureAwait(false);
        }
    }
}
