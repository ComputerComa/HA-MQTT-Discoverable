namespace HaMqttDiscoverable.Entities;

/// <summary>A stateless scene Home Assistant can activate, e.g. "Movie Night".</summary>
public sealed class Scene : HaEntity<SceneConfig>
{
    /// <inheritdoc />
    protected override bool SupportsCommands => true;

    /// <inheritdoc />
    protected override bool HasStateTopic => false;

    /// <summary>Raised whenever Home Assistant activates this scene.</summary>
    public event Func<Task>? Activated;

    /// <summary>Creates a scene from the given config, optionally subscribing `onActivated` to `Activated`.</summary>
    public Scene(HaMqttConnection connection, SceneConfig config, Func<Task>? onActivated = null)
        : base(connection, config)
    {
        if (onActivated is not null)
        {
            Activated += onActivated;
        }
    }

    /// <inheritdoc />
    protected override async Task OnCommandReceivedAsync(string payload)
    {
        if (Activated is not null)
        {
            await Activated.Invoke().ConfigureAwait(false);
        }
    }
}
