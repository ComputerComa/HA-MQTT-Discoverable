using System.Globalization;
using System.Text.Json.Nodes;

namespace HaMqttDiscoverable.Entities;

/// <summary>
/// A thermostat/HVAC device: an operating mode (one of <see cref="ClimateConfig.Modes"/>) plus a
/// target temperature, with optional fan mode, swing mode, preset mode, humidity, a heating/cooling
/// range, a separate power toggle, and a read-only "current action" report - enable the ones you
/// need via the corresponding <see cref="ClimateConfig"/> flags.
/// </summary>
public sealed class Climate : HaEntity<ClimateConfig>
{
    private readonly string? _fanModeCommandTopic;
    private readonly string? _swingModeCommandTopic;
    private readonly string? _presetModeCommandTopic;
    private readonly string? _humidityCommandTopic;
    private readonly string? _powerCommandTopic;

    /// <inheritdoc />
    protected override bool HasStateTopic => false;

    /// <summary>The topic this device accepts a mode on.</summary>
    public string ModeCommandTopic { get; }

    /// <summary>The topic this device publishes its mode to.</summary>
    public string ModeStateTopic { get; }

    /// <summary>The topic this device accepts a target temperature on. Null when <see cref="ClimateConfig.SupportsTemperatureRange"/> is true - see <see cref="TargetTemperatureLowCommandTopic"/>/<see cref="TargetTemperatureHighCommandTopic"/> instead.</summary>
    public string? TargetTemperatureCommandTopic { get; }

    /// <summary>The topic this device publishes its target temperature to. Null when <see cref="ClimateConfig.SupportsTemperatureRange"/> is true.</summary>
    public string? TargetTemperatureStateTopic { get; }

    /// <summary>The topic this device accepts the low end of a target range on. Only set when <see cref="ClimateConfig.SupportsTemperatureRange"/> is true.</summary>
    public string? TargetTemperatureLowCommandTopic { get; }

    /// <summary>The topic this device publishes the low end of its target range to. Only set when <see cref="ClimateConfig.SupportsTemperatureRange"/> is true.</summary>
    public string? TargetTemperatureLowStateTopic { get; }

    /// <summary>The topic this device accepts the high end of a target range on. Only set when <see cref="ClimateConfig.SupportsTemperatureRange"/> is true.</summary>
    public string? TargetTemperatureHighCommandTopic { get; }

    /// <summary>The topic this device publishes the high end of its target range to. Only set when <see cref="ClimateConfig.SupportsTemperatureRange"/> is true.</summary>
    public string? TargetTemperatureHighStateTopic { get; }

    /// <summary>The topic this device publishes the currently measured temperature to.</summary>
    public string CurrentTemperatureTopic { get; }

    /// <summary>The topic this device publishes its current action to, or null if <see cref="ClimateConfig.SupportsAction"/> is false.</summary>
    public string? ActionTopic { get; }

    /// <summary>The topic this device accepts a fan mode on, or null if <see cref="ClimateConfig.SupportsFanMode"/> is false.</summary>
    public string? FanModeCommandTopic => _fanModeCommandTopic;

    /// <summary>The topic this device publishes its fan mode to, or null if <see cref="ClimateConfig.SupportsFanMode"/> is false.</summary>
    public string? FanModeStateTopic { get; }

    /// <summary>The topic this device accepts a swing mode on, or null if <see cref="ClimateConfig.SupportsSwingMode"/> is false.</summary>
    public string? SwingModeCommandTopic => _swingModeCommandTopic;

    /// <summary>The topic this device publishes its swing mode to, or null if <see cref="ClimateConfig.SupportsSwingMode"/> is false.</summary>
    public string? SwingModeStateTopic { get; }

    /// <summary>The topic this device accepts a preset mode on, or null if <see cref="ClimateConfig.SupportsPresetMode"/> is false.</summary>
    public string? PresetModeCommandTopic => _presetModeCommandTopic;

    /// <summary>The topic this device publishes its preset mode to, or null if <see cref="ClimateConfig.SupportsPresetMode"/> is false.</summary>
    public string? PresetModeStateTopic { get; }

    /// <summary>The topic this device accepts a target humidity on, or null if <see cref="ClimateConfig.SupportsHumidity"/> is false.</summary>
    public string? TargetHumidityCommandTopic => _humidityCommandTopic;

    /// <summary>The topic this device publishes its target humidity to, or null if <see cref="ClimateConfig.SupportsHumidity"/> is false.</summary>
    public string? TargetHumidityStateTopic { get; }

    /// <summary>The topic this device publishes the currently measured humidity to, or null if <see cref="ClimateConfig.SupportsHumidity"/> is false.</summary>
    public string? CurrentHumidityTopic { get; }

    /// <summary>The topic this device accepts an on/off power command on, or null if <see cref="ClimateConfig.SupportsPower"/> is false.</summary>
    public string? PowerCommandTopic => _powerCommandTopic;

    /// <summary>Raised when Home Assistant sends a new mode (one of <see cref="ClimateConfig.Modes"/>).</summary>
    public event Func<string, Task>? ModeCommandReceived;

    /// <summary>Raised when Home Assistant sends a new target temperature. Only raised when <see cref="ClimateConfig.SupportsTemperatureRange"/> is false.</summary>
    public event Func<double, Task>? TargetTemperatureCommandReceived;

    /// <summary>Raised when Home Assistant sends a new target temperature range (low, high). Only raised when <see cref="ClimateConfig.SupportsTemperatureRange"/> is true.</summary>
    public event Func<double, double, Task>? TargetTemperatureRangeCommandReceived;

    /// <summary>Raised when Home Assistant sends a new fan mode. Only raised when <see cref="ClimateConfig.SupportsFanMode"/> is true.</summary>
    public event Func<string, Task>? FanModeCommandReceived;

    /// <summary>Raised when Home Assistant sends a new swing mode. Only raised when <see cref="ClimateConfig.SupportsSwingMode"/> is true.</summary>
    public event Func<string, Task>? SwingModeCommandReceived;

    /// <summary>Raised when Home Assistant sends a new preset mode. Only raised when <see cref="ClimateConfig.SupportsPresetMode"/> is true.</summary>
    public event Func<string, Task>? PresetModeCommandReceived;

    /// <summary>Raised when Home Assistant sends a new target humidity (0-100). Only raised when <see cref="ClimateConfig.SupportsHumidity"/> is true.</summary>
    public event Func<double, Task>? TargetHumidityCommandReceived;

    /// <summary>Raised when Home Assistant sends an on/off power command. Only raised when <see cref="ClimateConfig.SupportsPower"/> is true.</summary>
    public event Func<bool, Task>? PowerCommandReceived;

    private double? _pendingRangeLow;
    private double? _pendingRangeHigh;

    /// <summary>Creates a climate device from the given config.</summary>
    public Climate(HaMqttConnection connection, ClimateConfig config)
        : base(connection, config)
    {
        if (config.SupportsPresetMode && config.PresetModes.Count == 0)
        {
            throw new ArgumentException("ClimateConfig.PresetModes must be non-empty when SupportsPresetMode is true.", nameof(config));
        }

        ModeCommandTopic = $"{BaseTopic}/mode/set";
        ModeStateTopic = $"{BaseTopic}/mode/state";
        CurrentTemperatureTopic = $"{BaseTopic}/current_temperature";
        RegisterAuxiliaryCommandTopic(ModeCommandTopic, OnModeCommandReceivedAsync);

        if (config.SupportsTemperatureRange)
        {
            TargetTemperatureLowCommandTopic = $"{BaseTopic}/temperature_low/set";
            TargetTemperatureLowStateTopic = $"{BaseTopic}/temperature_low/state";
            TargetTemperatureHighCommandTopic = $"{BaseTopic}/temperature_high/set";
            TargetTemperatureHighStateTopic = $"{BaseTopic}/temperature_high/state";
            RegisterAuxiliaryCommandTopic(TargetTemperatureLowCommandTopic, p => OnTemperatureRangeCommandReceivedAsync(p, isLow: true));
            RegisterAuxiliaryCommandTopic(TargetTemperatureHighCommandTopic, p => OnTemperatureRangeCommandReceivedAsync(p, isLow: false));
        }
        else
        {
            TargetTemperatureCommandTopic = $"{BaseTopic}/temperature/set";
            TargetTemperatureStateTopic = $"{BaseTopic}/temperature/state";
            RegisterAuxiliaryCommandTopic(TargetTemperatureCommandTopic, OnTargetTemperatureCommandReceivedAsync);
        }

        if (config.SupportsAction)
        {
            ActionTopic = $"{BaseTopic}/action";
        }

        if (config.SupportsFanMode)
        {
            _fanModeCommandTopic = $"{BaseTopic}/fan_mode/set";
            FanModeStateTopic = $"{BaseTopic}/fan_mode/state";
            RegisterAuxiliaryCommandTopic(_fanModeCommandTopic, p => InvokeStringAsync(FanModeCommandReceived, p));
        }

        if (config.SupportsSwingMode)
        {
            _swingModeCommandTopic = $"{BaseTopic}/swing_mode/set";
            SwingModeStateTopic = $"{BaseTopic}/swing_mode/state";
            RegisterAuxiliaryCommandTopic(_swingModeCommandTopic, p => InvokeStringAsync(SwingModeCommandReceived, p));
        }

        if (config.SupportsPresetMode)
        {
            _presetModeCommandTopic = $"{BaseTopic}/preset_mode/set";
            PresetModeStateTopic = $"{BaseTopic}/preset_mode/state";
            RegisterAuxiliaryCommandTopic(_presetModeCommandTopic, p => InvokeStringAsync(PresetModeCommandReceived, p));
        }

        if (config.SupportsHumidity)
        {
            _humidityCommandTopic = $"{BaseTopic}/humidity/set";
            TargetHumidityStateTopic = $"{BaseTopic}/humidity/state";
            CurrentHumidityTopic = $"{BaseTopic}/current_humidity";
            RegisterAuxiliaryCommandTopic(_humidityCommandTopic, OnTargetHumidityCommandReceivedAsync);
        }

        if (config.SupportsPower)
        {
            _powerCommandTopic = $"{BaseTopic}/power/set";
            RegisterAuxiliaryCommandTopic(_powerCommandTopic, OnPowerCommandReceivedAsync);
        }
    }

    /// <summary>Publishes the device's current mode.</summary>
    public Task PublishModeAsync(string mode, bool retain = true, CancellationToken cancellationToken = default)
        => Connection.PublishAsync(ModeStateTopic, mode, retain, Config.Qos, cancellationToken);

    /// <summary>Publishes the device's current target temperature. Requires <see cref="ClimateConfig.SupportsTemperatureRange"/> to be false.</summary>
    public Task PublishTargetTemperatureAsync(double temperature, bool retain = true, CancellationToken cancellationToken = default)
    {
        if (TargetTemperatureStateTopic is null)
        {
            throw new InvalidOperationException("This climate entity uses a temperature range - call PublishTargetTemperatureRangeAsync instead.");
        }

        return Connection.PublishAsync(TargetTemperatureStateTopic, temperature.ToString(CultureInfo.InvariantCulture), retain, Config.Qos, cancellationToken);
    }

    /// <summary>Publishes the device's current target temperature range. Requires <see cref="ClimateConfig.SupportsTemperatureRange"/> to be true.</summary>
    public async Task PublishTargetTemperatureRangeAsync(double low, double high, bool retain = true, CancellationToken cancellationToken = default)
    {
        if (TargetTemperatureLowStateTopic is null || TargetTemperatureHighStateTopic is null)
        {
            throw new InvalidOperationException("This climate entity was not created with ClimateConfig.SupportsTemperatureRange.");
        }

        await Connection.PublishAsync(TargetTemperatureLowStateTopic, low.ToString(CultureInfo.InvariantCulture), retain, Config.Qos, cancellationToken).ConfigureAwait(false);
        await Connection.PublishAsync(TargetTemperatureHighStateTopic, high.ToString(CultureInfo.InvariantCulture), retain, Config.Qos, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Publishes the currently measured temperature.</summary>
    public Task PublishCurrentTemperatureAsync(double temperature, bool retain = true, CancellationToken cancellationToken = default)
        => Connection.PublishAsync(CurrentTemperatureTopic, temperature.ToString(CultureInfo.InvariantCulture), retain, Config.Qos, cancellationToken);

    /// <summary>Publishes what the equipment is actually doing right now. Requires <see cref="ClimateConfig.SupportsAction"/>.</summary>
    public Task PublishActionAsync(HvacAction action, bool retain = true, CancellationToken cancellationToken = default)
    {
        if (ActionTopic is null)
        {
            throw new InvalidOperationException("This climate entity was not created with ClimateConfig.SupportsAction.");
        }

        var value = action switch
        {
            HvacAction.Cooling => "cooling",
            HvacAction.Defrosting => "defrosting",
            HvacAction.Drying => "drying",
            HvacAction.Fan => "fan",
            HvacAction.Heating => "heating",
            HvacAction.Idle => "idle",
            HvacAction.Off => "off",
            HvacAction.Preheating => "preheating",
            _ => throw new ArgumentOutOfRangeException(nameof(action)),
        };

        return Connection.PublishAsync(ActionTopic, value, retain, Config.Qos, cancellationToken);
    }

    /// <summary>Publishes the device's current fan mode. Requires <see cref="ClimateConfig.SupportsFanMode"/>.</summary>
    public Task PublishFanModeAsync(string fanMode, bool retain = true, CancellationToken cancellationToken = default)
        => PublishAuxiliaryStateAsync(FanModeStateTopic, fanMode, nameof(ClimateConfig.SupportsFanMode), retain, cancellationToken);

    /// <summary>Publishes the device's current swing mode. Requires <see cref="ClimateConfig.SupportsSwingMode"/>.</summary>
    public Task PublishSwingModeAsync(string swingMode, bool retain = true, CancellationToken cancellationToken = default)
        => PublishAuxiliaryStateAsync(SwingModeStateTopic, swingMode, nameof(ClimateConfig.SupportsSwingMode), retain, cancellationToken);

    /// <summary>Publishes the device's current preset mode. Requires <see cref="ClimateConfig.SupportsPresetMode"/>.</summary>
    public Task PublishPresetModeAsync(string presetMode, bool retain = true, CancellationToken cancellationToken = default)
        => PublishAuxiliaryStateAsync(PresetModeStateTopic, presetMode, nameof(ClimateConfig.SupportsPresetMode), retain, cancellationToken);

    /// <summary>Publishes the device's current target humidity (0-100). Requires <see cref="ClimateConfig.SupportsHumidity"/>.</summary>
    public Task PublishTargetHumidityAsync(double humidity, bool retain = true, CancellationToken cancellationToken = default)
        => PublishAuxiliaryStateAsync(TargetHumidityStateTopic, humidity.ToString(CultureInfo.InvariantCulture), nameof(ClimateConfig.SupportsHumidity), retain, cancellationToken);

    /// <summary>Publishes the currently measured humidity (0-100). Requires <see cref="ClimateConfig.SupportsHumidity"/>.</summary>
    public Task PublishCurrentHumidityAsync(double humidity, bool retain = true, CancellationToken cancellationToken = default)
        => PublishAuxiliaryStateAsync(CurrentHumidityTopic, humidity.ToString(CultureInfo.InvariantCulture), nameof(ClimateConfig.SupportsHumidity), retain, cancellationToken);

    private Task PublishAuxiliaryStateAsync(string? topic, string value, string requiredFlagName, bool retain, CancellationToken cancellationToken)
    {
        if (topic is null)
        {
            throw new InvalidOperationException($"This climate entity was not created with ClimateConfig.{requiredFlagName}.");
        }

        return Connection.PublishAsync(topic, value, retain, Config.Qos, cancellationToken);
    }

    /// <inheritdoc />
    protected override void EnrichDiscoveryPayload(JsonObject payload)
    {
        payload["mode_command_topic"] = ModeCommandTopic;
        payload["mode_state_topic"] = ModeStateTopic;
        payload["current_temperature_topic"] = CurrentTemperatureTopic;

        if (TargetTemperatureCommandTopic is not null)
        {
            payload["temperature_command_topic"] = TargetTemperatureCommandTopic;
            payload["temperature_state_topic"] = TargetTemperatureStateTopic;
        }
        else
        {
            payload["temperature_low_command_topic"] = TargetTemperatureLowCommandTopic;
            payload["temperature_low_state_topic"] = TargetTemperatureLowStateTopic;
            payload["temperature_high_command_topic"] = TargetTemperatureHighCommandTopic;
            payload["temperature_high_state_topic"] = TargetTemperatureHighStateTopic;
        }

        if (ActionTopic is not null)
        {
            payload["action_topic"] = ActionTopic;
        }

        if (_fanModeCommandTopic is not null)
        {
            payload["fan_mode_command_topic"] = _fanModeCommandTopic;
            payload["fan_mode_state_topic"] = FanModeStateTopic;
        }

        if (_swingModeCommandTopic is not null)
        {
            payload["swing_mode_command_topic"] = _swingModeCommandTopic;
            payload["swing_mode_state_topic"] = SwingModeStateTopic;
        }

        if (_presetModeCommandTopic is not null)
        {
            payload["preset_mode_command_topic"] = _presetModeCommandTopic;
            payload["preset_mode_state_topic"] = PresetModeStateTopic;
        }

        if (_humidityCommandTopic is not null)
        {
            payload["target_humidity_command_topic"] = _humidityCommandTopic;
            payload["target_humidity_state_topic"] = TargetHumidityStateTopic;
            payload["current_humidity_topic"] = CurrentHumidityTopic;
        }

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

    private async Task OnTemperatureRangeCommandReceivedAsync(string payload, bool isLow)
    {
        if (!double.TryParse(payload, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            return;
        }

        if (isLow)
        {
            _pendingRangeLow = value;
        }
        else
        {
            _pendingRangeHigh = value;
        }

        if (TargetTemperatureRangeCommandReceived is not null && _pendingRangeLow is { } low && _pendingRangeHigh is { } high)
        {
            await TargetTemperatureRangeCommandReceived.Invoke(low, high).ConfigureAwait(false);
        }
    }

    private async Task OnTargetHumidityCommandReceivedAsync(string payload)
    {
        if (TargetHumidityCommandReceived is not null && double.TryParse(payload, NumberStyles.Float, CultureInfo.InvariantCulture, out var humidity))
        {
            await TargetHumidityCommandReceived.Invoke(humidity).ConfigureAwait(false);
        }
    }

    private async Task OnPowerCommandReceivedAsync(string payload)
    {
        if (PowerCommandReceived is not null)
        {
            await PowerCommandReceived.Invoke(string.Equals(payload, Config.PayloadOn, StringComparison.Ordinal)).ConfigureAwait(false);
        }
    }

    private static async Task InvokeStringAsync(Func<string, Task>? handler, string payload)
    {
        if (handler is not null)
        {
            await handler.Invoke(payload).ConfigureAwait(false);
        }
    }
}
