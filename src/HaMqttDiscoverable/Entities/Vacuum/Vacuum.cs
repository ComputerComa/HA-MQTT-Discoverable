using System.Text.Json;
using System.Text.Json.Nodes;
using HaMqttDiscoverable.Json;

namespace HaMqttDiscoverable.Entities;

/// <summary>A robot vacuum with start/pause/stop/return-to-base/clean-spot/locate controls and an optional fan speed.</summary>
public sealed class Vacuum : HaEntity<VacuumConfig>
{
    private readonly string? _setFanSpeedTopic;

    /// <inheritdoc />
    protected override bool SupportsCommands => true;

    /// <summary>The topic this vacuum accepts a fan speed on, or null if <see cref="VacuumConfig.FanSpeedList"/> is empty.</summary>
    public string? SetFanSpeedTopic => _setFanSpeedTopic;

    /// <summary>Raised when Home Assistant sends a start/pause/stop/return/clean-spot/locate command.</summary>
    public event Func<VacuumCommand, Task>? CommandReceived;

    /// <summary>Raised when Home Assistant sends a new fan speed. Only raised when <see cref="VacuumConfig.FanSpeedList"/> is non-empty.</summary>
    public event Func<string, Task>? FanSpeedCommandReceived;

    /// <summary>Creates a vacuum from the given config.</summary>
    public Vacuum(HaMqttConnection connection, VacuumConfig config)
        : base(connection, config)
    {
        if (config.FanSpeedList.Count > 0)
        {
            _setFanSpeedTopic = $"{BaseTopic}/fan_speed/set";
            RegisterAuxiliaryCommandTopic(_setFanSpeedTopic, OnFanSpeedCommandReceivedAsync);
        }
    }

    /// <summary>
    /// Publishes the vacuum's current activity, and optionally its fan speed and any extra
    /// attributes (e.g. battery_level), as a single JSON payload.
    /// </summary>
    public Task PublishStateAsync(VacuumActivity activity, string? fanSpeed = null, IReadOnlyDictionary<string, object?>? extraAttributes = null, bool retain = true, CancellationToken cancellationToken = default)
    {
        var node = new JsonObject
        {
            ["state"] = activity switch
            {
                VacuumActivity.Idle => "idle",
                VacuumActivity.Docked => "docked",
                VacuumActivity.Error => "error",
                VacuumActivity.Paused => "paused",
                VacuumActivity.Returning => "returning",
                VacuumActivity.Cleaning => "cleaning",
                _ => throw new ArgumentOutOfRangeException(nameof(activity)),
            },
        };

        if (fanSpeed is not null)
        {
            node["fan_speed"] = fanSpeed;
        }

        if (extraAttributes is not null)
        {
            foreach (var (key, value) in extraAttributes)
            {
                node[key] = JsonSerializer.SerializeToNode(value, HaJsonOptions.Default);
            }
        }

        return PublishStateAsync(node.ToJsonString(), retain, cancellationToken);
    }

    /// <inheritdoc />
    protected override void EnrichDiscoveryPayload(JsonObject payload)
    {
        if (_setFanSpeedTopic is not null)
        {
            payload["set_fan_speed_topic"] = _setFanSpeedTopic;
        }
    }

    /// <inheritdoc />
    protected override async Task OnCommandReceivedAsync(string payload)
    {
        if (CommandReceived is null)
        {
            return;
        }

        VacuumCommand? command = payload switch
        {
            _ when payload == Config.PayloadStart => VacuumCommand.Start,
            _ when payload == Config.PayloadPause => VacuumCommand.Pause,
            _ when payload == Config.PayloadStop => VacuumCommand.Stop,
            _ when payload == Config.PayloadReturnToBase => VacuumCommand.ReturnToBase,
            _ when payload == Config.PayloadCleanSpot => VacuumCommand.CleanSpot,
            _ when payload == Config.PayloadLocate => VacuumCommand.Locate,
            _ => null,
        };

        if (command is not null)
        {
            await CommandReceived.Invoke(command.Value).ConfigureAwait(false);
        }
    }

    private async Task OnFanSpeedCommandReceivedAsync(string payload)
    {
        if (FanSpeedCommandReceived is not null)
        {
            await FanSpeedCommandReceived.Invoke(payload).ConfigureAwait(false);
        }
    }
}
