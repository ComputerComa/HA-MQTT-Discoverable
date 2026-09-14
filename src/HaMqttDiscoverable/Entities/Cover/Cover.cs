using System.Globalization;
using System.Text.Json.Nodes;

namespace HaMqttDiscoverable.Entities;

/// <summary>
/// A cover (blind, garage door, shade, awning, ...) that can be opened/closed/stopped, and
/// optionally reports/accepts a 0-100 position and, independently, a 0-100 tilt.
/// </summary>
/// <example>
/// <code>
/// var blind = new Cover(connection, new CoverConfig
/// {
///     Name = "Blind",
///     UniqueId = "living-room-blind",
///     Device = device,
///     DeviceClass = "blind",
///     SupportsPosition = true,
/// });
/// blind.PositionCommandReceived += async position =>
/// {
///     MoveTo(position);
///     await blind.PublishPositionAsync(position);
/// };
/// await blind.PublishDiscoveryAsync();
/// </code>
/// </example>
public sealed class Cover : HaEntity<CoverConfig>
{
    private readonly string? _setPositionTopic;
    private readonly string? _tiltCommandTopic;

    /// <inheritdoc />
    protected override bool SupportsCommands => true;

    /// <summary>The topic this cover accepts a target position on, or null if <see cref="CoverConfig.SupportsPosition"/> is false.</summary>
    public string? SetPositionTopic => _setPositionTopic;

    /// <summary>The topic this cover accepts tilt commands on, or null if <see cref="CoverConfig.SupportsTilt"/> is false.</summary>
    public string? TiltCommandTopic => _tiltCommandTopic;

    /// <summary>The topic this cover publishes its tilt status to, or null if <see cref="CoverConfig.SupportsTilt"/> is false.</summary>
    public string? TiltStatusTopic { get; }

    /// <summary>Raised when Home Assistant sends an open/close/stop command.</summary>
    public event Func<CoverCommand, Task>? CommandReceived;

    /// <summary>Raised when Home Assistant sends a new target position (0-100). Only raised when <see cref="CoverConfig.SupportsPosition"/> is true.</summary>
    public event Func<int, Task>? PositionCommandReceived;

    /// <summary>Raised when Home Assistant sends a new target tilt (0-100). Only raised when <see cref="CoverConfig.SupportsTilt"/> is true.</summary>
    public event Func<int, Task>? TiltCommandReceived;

    /// <summary>Creates a cover from the given config.</summary>
    public Cover(HaMqttConnection connection, CoverConfig config)
        : base(connection, config)
    {
        if (config.SupportsPosition)
        {
            _setPositionTopic = $"{BaseTopic}/set_position";
            RegisterAuxiliaryCommandTopic(_setPositionTopic, OnPositionCommandReceivedAsync);
        }

        if (config.SupportsTilt)
        {
            _tiltCommandTopic = $"{BaseTopic}/tilt/set";
            TiltStatusTopic = $"{BaseTopic}/tilt/state";
            RegisterAuxiliaryCommandTopic(_tiltCommandTopic, OnTiltCommandReceivedAsync);
        }
    }

    /// <summary>Publishes open/closed/opening/closing/stopped state.</summary>
    public Task PublishStateAsync(CoverState state, bool retain = true, CancellationToken cancellationToken = default)
        => PublishStateAsync(ToStateString(state), retain, cancellationToken);

    /// <summary>Publishes the cover's current position (0-100), and optionally its state, as a single JSON payload. Requires <see cref="CoverConfig.SupportsPosition"/>.</summary>
    public Task PublishPositionAsync(int position, CoverState? state = null, bool retain = true, CancellationToken cancellationToken = default)
    {
        var node = new JsonObject { ["position"] = position };
        if (state is not null)
        {
            node["state"] = ToStateString(state.Value);
        }

        return PublishStateAsync(node.ToJsonString(), retain, cancellationToken);
    }

    /// <summary>Publishes the cover's current tilt (0-100). Requires <see cref="CoverConfig.SupportsTilt"/>.</summary>
    public Task PublishTiltAsync(int tilt, bool retain = true, CancellationToken cancellationToken = default)
    {
        if (TiltStatusTopic is null)
        {
            throw new InvalidOperationException("This cover was not created with CoverConfig.SupportsTilt.");
        }

        return Connection.PublishAsync(TiltStatusTopic, tilt.ToString(CultureInfo.InvariantCulture), retain, Config.Qos, cancellationToken);
    }

    /// <inheritdoc />
    protected override void EnrichDiscoveryPayload(JsonObject payload)
    {
        if (_setPositionTopic is not null)
        {
            payload["set_position_topic"] = _setPositionTopic;
        }

        if (_tiltCommandTopic is not null)
        {
            payload["tilt_command_topic"] = _tiltCommandTopic;
            payload["tilt_status_topic"] = TiltStatusTopic;
        }
    }

    /// <inheritdoc />
    protected override async Task OnCommandReceivedAsync(string payload)
    {
        if (CommandReceived is null)
        {
            return;
        }

        CoverCommand? command = payload switch
        {
            _ when payload == Config.PayloadOpen => CoverCommand.Open,
            _ when payload == Config.PayloadClose => CoverCommand.Close,
            _ when payload == Config.PayloadStop => CoverCommand.Stop,
            _ => null,
        };

        if (command is not null)
        {
            await CommandReceived.Invoke(command.Value).ConfigureAwait(false);
        }
    }

    private async Task OnPositionCommandReceivedAsync(string payload)
    {
        if (PositionCommandReceived is not null && int.TryParse(payload, NumberStyles.Integer, CultureInfo.InvariantCulture, out var position))
        {
            await PositionCommandReceived.Invoke(position).ConfigureAwait(false);
        }
    }

    private async Task OnTiltCommandReceivedAsync(string payload)
    {
        if (TiltCommandReceived is not null && int.TryParse(payload, NumberStyles.Integer, CultureInfo.InvariantCulture, out var tilt))
        {
            await TiltCommandReceived.Invoke(tilt).ConfigureAwait(false);
        }
    }

    private string ToStateString(CoverState state) => state switch
    {
        CoverState.Open => Config.StateOpen,
        CoverState.Closed => Config.StateClosed,
        CoverState.Opening => Config.StateOpening,
        CoverState.Closing => Config.StateClosing,
        CoverState.Stopped => Config.StateStopped,
        _ => throw new ArgumentOutOfRangeException(nameof(state)),
    };
}
