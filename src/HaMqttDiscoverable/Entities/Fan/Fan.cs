using System.Globalization;
using System.Text.Json.Nodes;

namespace HaMqttDiscoverable.Entities;

/// <summary>
/// A fan that can be turned on/off, and optionally has a speed percentage and/or named preset
/// modes. Oscillation and direction (forward/reverse) control aren't exposed by this type yet -
/// they'd follow the same auxiliary-topic pattern as <see cref="FanConfig.SupportsPercentage"/>
/// support; see <c>HaEntity.RegisterAuxiliaryCommandTopic</c> if you need to add them.
/// </summary>
public sealed class Fan : HaEntity<FanConfig>
{
    private readonly string? _percentageCommandTopic;
    private readonly string? _presetModeCommandTopic;

    /// <inheritdoc />
    protected override bool SupportsCommands => true;

    /// <summary>The topic this fan accepts a speed percentage on, or null if <see cref="FanConfig.SupportsPercentage"/> is false.</summary>
    public string? PercentageCommandTopic => _percentageCommandTopic;

    /// <summary>The topic this fan publishes its speed percentage to, or null if <see cref="FanConfig.SupportsPercentage"/> is false.</summary>
    public string? PercentageStateTopic { get; }

    /// <summary>The topic this fan accepts a preset mode on, or null if <see cref="FanConfig.PresetModes"/> is empty.</summary>
    public string? PresetModeCommandTopic => _presetModeCommandTopic;

    /// <summary>The topic this fan publishes its preset mode to, or null if <see cref="FanConfig.PresetModes"/> is empty.</summary>
    public string? PresetModeStateTopic { get; }

    /// <summary>Raised when Home Assistant sends a command to turn the fan on or off.</summary>
    public event Func<bool, Task>? CommandReceived;

    /// <summary>Raised when Home Assistant sends a new speed percentage (0-100). Only raised when <see cref="FanConfig.SupportsPercentage"/> is true.</summary>
    public event Func<int, Task>? PercentageCommandReceived;

    /// <summary>Raised when Home Assistant sends a new preset mode. Only raised when <see cref="FanConfig.PresetModes"/> is non-empty.</summary>
    public event Func<string, Task>? PresetModeCommandReceived;

    /// <summary>Creates a fan from the given config.</summary>
    public Fan(HaMqttConnection connection, FanConfig config)
        : base(connection, config)
    {
        if (config.SupportsPercentage)
        {
            _percentageCommandTopic = $"{BaseTopic}/percentage/set";
            PercentageStateTopic = $"{BaseTopic}/percentage/state";
            RegisterAuxiliaryCommandTopic(_percentageCommandTopic, OnPercentageCommandReceivedAsync);
        }

        if (config.PresetModes.Count > 0)
        {
            _presetModeCommandTopic = $"{BaseTopic}/preset_mode/set";
            PresetModeStateTopic = $"{BaseTopic}/preset_mode/state";
            RegisterAuxiliaryCommandTopic(_presetModeCommandTopic, OnPresetModeCommandReceivedAsync);
        }
    }

    /// <summary>Publishes the fan's current on/off state.</summary>
    public Task PublishStateAsync(bool isOn, bool retain = true, CancellationToken cancellationToken = default)
        => PublishStateAsync(isOn ? Config.PayloadOn : Config.PayloadOff, retain, cancellationToken);

    /// <summary>Publishes the fan's current speed (0-100). Requires <see cref="FanConfig.SupportsPercentage"/>.</summary>
    public Task PublishPercentageAsync(int percentage, bool retain = true, CancellationToken cancellationToken = default)
    {
        if (PercentageStateTopic is null)
        {
            throw new InvalidOperationException("This fan was not created with FanConfig.SupportsPercentage.");
        }

        return Connection.PublishAsync(PercentageStateTopic, percentage.ToString(CultureInfo.InvariantCulture), retain, Config.Qos, cancellationToken);
    }

    /// <summary>Publishes the fan's current preset mode. Requires <see cref="FanConfig.PresetModes"/> to be non-empty.</summary>
    public Task PublishPresetModeAsync(string presetMode, bool retain = true, CancellationToken cancellationToken = default)
    {
        if (PresetModeStateTopic is null)
        {
            throw new InvalidOperationException("This fan was not created with any FanConfig.PresetModes.");
        }

        return Connection.PublishAsync(PresetModeStateTopic, presetMode, retain, Config.Qos, cancellationToken);
    }

    /// <inheritdoc />
    protected override void EnrichDiscoveryPayload(JsonObject payload)
    {
        if (_percentageCommandTopic is not null)
        {
            payload["percentage_command_topic"] = _percentageCommandTopic;
            payload["percentage_state_topic"] = PercentageStateTopic;
        }

        if (_presetModeCommandTopic is not null)
        {
            payload["preset_mode_command_topic"] = _presetModeCommandTopic;
            payload["preset_mode_state_topic"] = PresetModeStateTopic;
            payload["preset_modes"] = new JsonArray(Config.PresetModes.Select(m => (JsonNode)m).ToArray());
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

    private async Task OnPercentageCommandReceivedAsync(string payload)
    {
        if (PercentageCommandReceived is not null && int.TryParse(payload, NumberStyles.Integer, CultureInfo.InvariantCulture, out var percentage))
        {
            await PercentageCommandReceived.Invoke(percentage).ConfigureAwait(false);
        }
    }

    private async Task OnPresetModeCommandReceivedAsync(string payload)
    {
        if (PresetModeCommandReceived is not null)
        {
            await PresetModeCommandReceived.Invoke(payload).ConfigureAwait(false);
        }
    }
}
