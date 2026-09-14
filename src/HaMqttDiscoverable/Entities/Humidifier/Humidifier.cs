using System.Globalization;
using System.Text.Json.Nodes;

namespace HaMqttDiscoverable.Entities;

/// <summary>
/// A humidifier or dehumidifier: on/off with a target humidity, optionally with named modes
/// (e.g. "auto", "sleep"). Set <see cref="EntityConfig.DeviceClass"/> to "humidifier" (the
/// default Home Assistant assumes) or "dehumidifier".
/// </summary>
public sealed class Humidifier : HaEntity<HumidifierConfig>
{
    private readonly string? _modeCommandTopic;

    /// <inheritdoc />
    protected override bool SupportsCommands => true;

    /// <summary>The topic this humidifier accepts a target humidity on.</summary>
    public string TargetHumidityCommandTopic { get; }

    /// <summary>The topic this humidifier publishes its target humidity to.</summary>
    public string TargetHumidityStateTopic { get; }

    /// <summary>The topic this humidifier publishes the currently measured humidity to.</summary>
    public string CurrentHumidityTopic { get; }

    /// <summary>The topic this humidifier accepts a mode on, or null if <see cref="HumidifierConfig.Modes"/> is empty.</summary>
    public string? ModeCommandTopic => _modeCommandTopic;

    /// <summary>The topic this humidifier publishes its mode to, or null if <see cref="HumidifierConfig.Modes"/> is empty.</summary>
    public string? ModeStateTopic { get; }

    /// <summary>Raised when Home Assistant sends a command to turn the humidifier on or off.</summary>
    public event Func<bool, Task>? CommandReceived;

    /// <summary>Raised when Home Assistant sends a new target humidity (0-100).</summary>
    public event Func<double, Task>? TargetHumidityCommandReceived;

    /// <summary>Raised when Home Assistant sends a new mode. Only raised when <see cref="HumidifierConfig.Modes"/> is non-empty.</summary>
    public event Func<string, Task>? ModeCommandReceived;

    /// <summary>Creates a humidifier from the given config.</summary>
    public Humidifier(HaMqttConnection connection, HumidifierConfig config)
        : base(connection, config)
    {
        TargetHumidityCommandTopic = $"{BaseTopic}/humidity/set";
        TargetHumidityStateTopic = $"{BaseTopic}/humidity/state";
        CurrentHumidityTopic = $"{BaseTopic}/current_humidity";
        RegisterAuxiliaryCommandTopic(TargetHumidityCommandTopic, OnTargetHumidityCommandReceivedAsync);

        if (config.Modes.Count > 0)
        {
            _modeCommandTopic = $"{BaseTopic}/mode/set";
            ModeStateTopic = $"{BaseTopic}/mode/state";
            RegisterAuxiliaryCommandTopic(_modeCommandTopic, OnModeCommandReceivedAsync);
        }
    }

    /// <summary>Publishes the humidifier's current on/off state.</summary>
    public Task PublishStateAsync(bool isOn, bool retain = true, CancellationToken cancellationToken = default)
        => PublishStateAsync(isOn ? Config.PayloadOn : Config.PayloadOff, retain, cancellationToken);

    /// <summary>Publishes the humidifier's current target humidity (0-100).</summary>
    public Task PublishTargetHumidityAsync(double humidity, bool retain = true, CancellationToken cancellationToken = default)
        => Connection.PublishAsync(TargetHumidityStateTopic, humidity.ToString(CultureInfo.InvariantCulture), retain, Config.Qos, cancellationToken);

    /// <summary>Publishes the currently measured humidity (0-100).</summary>
    public Task PublishCurrentHumidityAsync(double humidity, bool retain = true, CancellationToken cancellationToken = default)
        => Connection.PublishAsync(CurrentHumidityTopic, humidity.ToString(CultureInfo.InvariantCulture), retain, Config.Qos, cancellationToken);

    /// <summary>Publishes the humidifier's current mode. Requires <see cref="HumidifierConfig.Modes"/> to be non-empty.</summary>
    public Task PublishModeAsync(string mode, bool retain = true, CancellationToken cancellationToken = default)
    {
        if (ModeStateTopic is null)
        {
            throw new InvalidOperationException("This humidifier was not created with any HumidifierConfig.Modes.");
        }

        return Connection.PublishAsync(ModeStateTopic, mode, retain, Config.Qos, cancellationToken);
    }

    /// <inheritdoc />
    protected override void EnrichDiscoveryPayload(JsonObject payload)
    {
        payload["target_humidity_command_topic"] = TargetHumidityCommandTopic;
        payload["target_humidity_state_topic"] = TargetHumidityStateTopic;
        payload["current_humidity_topic"] = CurrentHumidityTopic;

        if (_modeCommandTopic is not null)
        {
            payload["mode_command_topic"] = _modeCommandTopic;
            payload["mode_state_topic"] = ModeStateTopic;
            payload["available_modes"] = new JsonArray(Config.Modes.Select(m => (JsonNode)m).ToArray());
        }
    }

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

    private async Task OnTargetHumidityCommandReceivedAsync(string payload)
    {
        if (TargetHumidityCommandReceived is not null && double.TryParse(payload, NumberStyles.Float, CultureInfo.InvariantCulture, out var humidity))
        {
            await TargetHumidityCommandReceived.Invoke(humidity).ConfigureAwait(false);
        }
    }

    private async Task OnModeCommandReceivedAsync(string payload)
    {
        if (ModeCommandReceived is not null)
        {
            await ModeCommandReceived.Invoke(payload).ConfigureAwait(false);
        }
    }
}
