using System.Globalization;
using System.Text.Json.Nodes;

namespace HaMqttDiscoverable.Entities;

/// <summary>A water heater with an operating mode plus a target/current temperature.</summary>
public sealed class WaterHeater : HaEntity<WaterHeaterConfig>
{
    private readonly string? _powerCommandTopic;

    /// <inheritdoc />
    protected override bool HasStateTopic => false;

    /// <summary>The topic this water heater accepts a mode on.</summary>
    public string ModeCommandTopic { get; }

    /// <summary>The topic this water heater publishes its mode to.</summary>
    public string ModeStateTopic { get; }

    /// <summary>The topic this water heater accepts a target temperature on.</summary>
    public string TargetTemperatureCommandTopic { get; }

    /// <summary>The topic this water heater publishes its target temperature to.</summary>
    public string TargetTemperatureStateTopic { get; }

    /// <summary>The topic this water heater publishes the currently measured temperature to.</summary>
    public string CurrentTemperatureTopic { get; }

    /// <summary>The topic this water heater accepts an on/off power command on, or null if <see cref="WaterHeaterConfig.SupportsPower"/> is false.</summary>
    public string? PowerCommandTopic => _powerCommandTopic;

    /// <summary>Raised when Home Assistant sends a new mode (one of <see cref="WaterHeaterConfig.Modes"/>).</summary>
    public event Func<string, Task>? ModeCommandReceived;

    /// <summary>Raised when Home Assistant sends a new target temperature.</summary>
    public event Func<double, Task>? TargetTemperatureCommandReceived;

    /// <summary>Raised when Home Assistant sends an on/off power command. Only raised when <see cref="WaterHeaterConfig.SupportsPower"/> is true.</summary>
    public event Func<bool, Task>? PowerCommandReceived;

    /// <summary>Creates a water heater from the given config.</summary>
    public WaterHeater(HaMqttConnection connection, WaterHeaterConfig config)
        : base(connection, config)
    {
        ModeCommandTopic = $"{BaseTopic}/mode/set";
        ModeStateTopic = $"{BaseTopic}/mode/state";
        TargetTemperatureCommandTopic = $"{BaseTopic}/temperature/set";
        TargetTemperatureStateTopic = $"{BaseTopic}/temperature/state";
        CurrentTemperatureTopic = $"{BaseTopic}/current_temperature";

        RegisterAuxiliaryCommandTopic(ModeCommandTopic, OnModeCommandReceivedAsync);
        RegisterAuxiliaryCommandTopic(TargetTemperatureCommandTopic, OnTargetTemperatureCommandReceivedAsync);

        if (config.SupportsPower)
        {
            _powerCommandTopic = $"{BaseTopic}/power/set";
            RegisterAuxiliaryCommandTopic(_powerCommandTopic, OnPowerCommandReceivedAsync);
        }
    }

    /// <summary>Publishes the water heater's current mode.</summary>
    public Task PublishModeAsync(string mode, bool retain = true, CancellationToken cancellationToken = default)
        => Connection.PublishAsync(ModeStateTopic, mode, retain, Config.Qos, cancellationToken);

    /// <summary>Publishes the water heater's current target temperature.</summary>
    public Task PublishTargetTemperatureAsync(double temperature, bool retain = true, CancellationToken cancellationToken = default)
        => Connection.PublishAsync(TargetTemperatureStateTopic, temperature.ToString(CultureInfo.InvariantCulture), retain, Config.Qos, cancellationToken);

    /// <summary>Publishes the currently measured water temperature.</summary>
    public Task PublishCurrentTemperatureAsync(double temperature, bool retain = true, CancellationToken cancellationToken = default)
        => Connection.PublishAsync(CurrentTemperatureTopic, temperature.ToString(CultureInfo.InvariantCulture), retain, Config.Qos, cancellationToken);

    /// <inheritdoc />
    protected override void EnrichDiscoveryPayload(JsonObject payload)
    {
        payload["mode_command_topic"] = ModeCommandTopic;
        payload["mode_state_topic"] = ModeStateTopic;
        payload["temperature_command_topic"] = TargetTemperatureCommandTopic;
        payload["temperature_state_topic"] = TargetTemperatureStateTopic;
        payload["current_temperature_topic"] = CurrentTemperatureTopic;

        if (_powerCommandTopic is not null)
        {
            payload["power_command_topic"] = _powerCommandTopic;
        }
    }

    private async Task OnModeCommandReceivedAsync(string payload)
    {
        if (ModeCommandReceived is not null)
        {
            await ModeCommandReceived.Invoke(payload).ConfigureAwait(false);
        }
    }

    private async Task OnTargetTemperatureCommandReceivedAsync(string payload)
    {
        if (TargetTemperatureCommandReceived is not null && double.TryParse(payload, NumberStyles.Float, CultureInfo.InvariantCulture, out var temperature))
        {
            await TargetTemperatureCommandReceived.Invoke(temperature).ConfigureAwait(false);
        }
    }

    private async Task OnPowerCommandReceivedAsync(string payload)
    {
        if (PowerCommandReceived is not null)
        {
            await PowerCommandReceived.Invoke(string.Equals(payload, Config.PayloadOn, StringComparison.Ordinal)).ConfigureAwait(false);
        }
    }
}
