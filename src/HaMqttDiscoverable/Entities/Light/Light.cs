using System.Text.Json;
using System.Text.Json.Nodes;
using HaMqttDiscoverable.Json;

namespace HaMqttDiscoverable.Entities;

/// <summary>A light that can be turned on/off and, optionally, dimmed.</summary>
/// <example>
/// <code>
/// var light = new Light(connection, new LightConfig
/// {
///     Name = "Lamp",
///     UniqueId = "living-room-lamp",
///     Device = device,
///     SupportsBrightness = true,
/// });
/// light.CommandReceived += async state =>
/// {
///     ApplyToHardware(state.State == "ON", state.Brightness);
///     await light.PublishStateAsync(state);
/// };
/// await light.PublishDiscoveryAsync();
/// </code>
/// </example>
public sealed class Light : HaEntity<LightConfig>
{
    /// <inheritdoc />
    protected override bool SupportsCommands => true;

    /// <summary>Raised when Home Assistant sends a new desired state.</summary>
    public event Func<LightState, Task>? CommandReceived;

    /// <summary>Creates a light from the given config, optionally subscribing <paramref name="onCommand"/> to <see cref="CommandReceived"/>.</summary>
    public Light(HaMqttConnection connection, LightConfig config, Func<LightState, Task>? onCommand = null)
        : base(connection, config)
    {
        if (onCommand is not null)
        {
            CommandReceived += onCommand;
        }
    }

    /// <summary>Publishes the light's current state.</summary>
    public Task PublishStateAsync(LightState state, bool retain = true, CancellationToken cancellationToken = default)
        => PublishStateAsync<LightState>(state, retain, cancellationToken);

    /// <summary>Publishes the light's current on/off state and, optionally, brightness.</summary>
    public Task PublishStateAsync(bool isOn, int? brightness = null, bool retain = true, CancellationToken cancellationToken = default)
        => PublishStateAsync(new LightState { State = isOn ? "ON" : "OFF", Brightness = brightness }, retain, cancellationToken);

    /// <inheritdoc />
    protected override void EnrichDiscoveryPayload(JsonObject payload)
    {
        payload["schema"] = "json";
    }

    /// <inheritdoc />
    protected override async Task OnCommandReceivedAsync(string payload)
    {
        if (CommandReceived is null)
        {
            return;
        }

        LightState? state;
        try
        {
            state = JsonSerializer.Deserialize<LightState>(payload, HaJsonOptions.Default);
        }
        catch (JsonException)
        {
            return;
        }

        if (state is not null)
        {
            await CommandReceived.Invoke(state).ConfigureAwait(false);
        }
    }
}
