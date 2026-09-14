using System.Globalization;

namespace HaMqttDiscoverable.Entities;

/// <summary>
/// A valve that can be opened/closed (optionally stopped mid-movement), or - when
/// <see cref="ValveConfig.ReportsPosition"/> is set - opened to a specific 0-100 position.
/// </summary>
public sealed class Valve : HaEntity<ValveConfig>
{
    /// <inheritdoc />
    protected override bool SupportsCommands => true;

    /// <summary>Raised when Home Assistant sends an open/close/stop command. Not raised for a valve with <see cref="ValveConfig.ReportsPosition"/> set - see <see cref="PositionCommandReceived"/> instead.</summary>
    public event Func<ValveCommand, Task>? CommandReceived;

    /// <summary>Raised when Home Assistant sends a new target position (0-100). Only raised for a valve with <see cref="ValveConfig.ReportsPosition"/> set.</summary>
    public event Func<int, Task>? PositionCommandReceived;

    /// <summary>Creates a valve from the given config.</summary>
    public Valve(HaMqttConnection connection, ValveConfig config)
        : base(connection, config)
    {
    }

    /// <summary>Publishes the valve's open/closed/opening/closing state. Only meaningful when <see cref="ValveConfig.ReportsPosition"/> is false.</summary>
    public Task PublishStateAsync(ValveState state, bool retain = true, CancellationToken cancellationToken = default)
        => PublishStateAsync(state switch
        {
            ValveState.Open => Config.StateOpen ?? "open",
            ValveState.Closed => Config.StateClosed ?? "closed",
            ValveState.Opening => Config.StateOpening,
            ValveState.Closing => Config.StateClosing,
            _ => throw new ArgumentOutOfRangeException(nameof(state)),
        }, retain, cancellationToken);

    /// <summary>Publishes the valve's current position (0-100). Only meaningful when <see cref="ValveConfig.ReportsPosition"/> is true.</summary>
    public Task PublishPositionAsync(int position, bool retain = true, CancellationToken cancellationToken = default)
        => PublishStateAsync(position.ToString(CultureInfo.InvariantCulture), retain, cancellationToken);

    /// <inheritdoc />
    protected override async Task OnCommandReceivedAsync(string payload)
    {
        if (Config.ReportsPosition)
        {
            if (PositionCommandReceived is not null && int.TryParse(payload, NumberStyles.Integer, CultureInfo.InvariantCulture, out var position))
            {
                await PositionCommandReceived.Invoke(position).ConfigureAwait(false);
            }

            return;
        }

        if (CommandReceived is null)
        {
            return;
        }

        ValveCommand? command = payload switch
        {
            _ when payload == Config.PayloadOpen => ValveCommand.Open,
            _ when payload == Config.PayloadClose => ValveCommand.Close,
            _ when Config.PayloadStop is not null && payload == Config.PayloadStop => ValveCommand.Stop,
            _ => null,
        };

        if (command is not null)
        {
            await CommandReceived.Invoke(command.Value).ConfigureAwait(false);
        }
    }
}
