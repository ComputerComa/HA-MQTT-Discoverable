using System.Text.Json.Nodes;

namespace HaMqttDiscoverable.Entities;

/// <summary>A robot lawn mower with start/pause/dock controls and an activity report.</summary>
public sealed class LawnMower : HaEntity<LawnMowerConfig>
{
    /// <inheritdoc />
    protected override bool HasStateTopic => false;

    /// <summary>The topic this mower publishes its activity to.</summary>
    public string ActivityStateTopic => StateTopic;

    /// <summary>The topic this mower accepts a "start mowing" command on.</summary>
    public string StartMowingCommandTopic { get; }

    /// <summary>The topic this mower accepts a "dock" command on.</summary>
    public string DockCommandTopic { get; }

    /// <summary>The topic this mower accepts a "pause" command on.</summary>
    public string PauseCommandTopic { get; }

    /// <summary>Raised when Home Assistant requests mowing to start (or resume).</summary>
    public event Func<Task>? StartMowingRequested;

    /// <summary>Raised when Home Assistant requests the mower dock.</summary>
    public event Func<Task>? DockRequested;

    /// <summary>Raised when Home Assistant requests the mower pause.</summary>
    public event Func<Task>? PauseRequested;

    /// <summary>Creates a lawn mower from the given config.</summary>
    public LawnMower(HaMqttConnection connection, LawnMowerConfig config)
        : base(connection, config)
    {
        StartMowingCommandTopic = $"{BaseTopic}/start_mowing";
        DockCommandTopic = $"{BaseTopic}/dock";
        PauseCommandTopic = $"{BaseTopic}/pause";

        RegisterAuxiliaryCommandTopic(StartMowingCommandTopic, _ => InvokeAsync(StartMowingRequested));
        RegisterAuxiliaryCommandTopic(DockCommandTopic, _ => InvokeAsync(DockRequested));
        RegisterAuxiliaryCommandTopic(PauseCommandTopic, _ => InvokeAsync(PauseRequested));
    }

    /// <summary>Publishes the mower's current activity.</summary>
    public Task PublishStateAsync(LawnMowerActivity activity, bool retain = true, CancellationToken cancellationToken = default)
        => PublishStateAsync(activity switch
        {
            LawnMowerActivity.Error => "error",
            LawnMowerActivity.Paused => "paused",
            LawnMowerActivity.Mowing => "mowing",
            LawnMowerActivity.Docked => "docked",
            LawnMowerActivity.Returning => "returning",
            LawnMowerActivity.Idle => "idle",
            _ => throw new ArgumentOutOfRangeException(nameof(activity)),
        }, retain, cancellationToken);

    /// <inheritdoc />
    protected override void EnrichDiscoveryPayload(JsonObject payload)
    {
        payload["activity_state_topic"] = ActivityStateTopic;
        payload["start_mowing_command_topic"] = StartMowingCommandTopic;
        payload["dock_command_topic"] = DockCommandTopic;
        payload["pause_command_topic"] = PauseCommandTopic;
    }

    private static Task InvokeAsync(Func<Task>? handler) => handler?.Invoke() ?? Task.CompletedTask;
}
