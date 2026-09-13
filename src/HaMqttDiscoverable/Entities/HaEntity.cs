using System.Text.Json;
using System.Text.Json.Nodes;
using HaMqttDiscoverable.Json;

namespace HaMqttDiscoverable.Entities;

/// <summary>
/// Base class for every discoverable entity. Handles building the MQTT topics, publishing the
/// Home Assistant discovery payload, publishing state, and (for entities that accept commands)
/// subscribing to the command topic.
/// </summary>
public abstract class HaEntity<TConfig> : IAsyncDisposable
    where TConfig : EntityConfig
{
    private bool _subscribed;

    /// <summary>The MQTT connection this entity publishes through.</summary>
    protected HaMqttConnection Connection { get; }

    /// <summary>The configuration this entity was created with.</summary>
    public TConfig Config { get; }

    /// <summary>The retained topic Home Assistant reads the discovery payload from.</summary>
    public string DiscoveryTopic { get; }

    /// <summary>The topic this entity publishes its state to.</summary>
    public string StateTopic { get; }

    /// <summary>The topic this entity listens on for commands from Home Assistant, or null if it doesn't accept commands.</summary>
    public string? CommandTopic { get; }

    /// <summary>Whether this entity type accepts commands from Home Assistant (e.g. switches, numbers). Sensors are read-only and return false.</summary>
    protected virtual bool SupportsCommands => false;

    /// <summary>Whether this entity type reports a state at all. Stateless entities like <see cref="Button"/> override this to false.</summary>
    protected virtual bool HasStateTopic => true;

    /// <summary>Computes this entity's MQTT topics from its config. Call <see cref="PublishDiscoveryAsync"/> to make it visible to Home Assistant.</summary>
    protected HaEntity(HaMqttConnection connection, TConfig config)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        Config = config ?? throw new ArgumentNullException(nameof(config));

        if (string.IsNullOrWhiteSpace(config.UniqueId))
        {
            throw new ArgumentException("EntityConfig.UniqueId is required.", nameof(config));
        }

        if (config.Device is null || config.Device.Identifiers.Count == 0)
        {
            throw new ArgumentException("EntityConfig.Device with at least one identifier is required.", nameof(config));
        }

        var deviceId = Slug.Create(config.Device.Identifiers[0]);
        var objectId = Slug.Create(config.UniqueId);
        var baseTopic = $"{connection.DiscoveryPrefix}/{config.Component}/{deviceId}/{objectId}";

        DiscoveryTopic = $"{baseTopic}/config";
        StateTopic = $"{baseTopic}/state";
        CommandTopic = SupportsCommands ? $"{baseTopic}/set" : null;
    }

    /// <summary>
    /// Publishes the Home Assistant MQTT discovery payload for this entity (retained), and
    /// subscribes to its command topic if it accepts commands. Call this once after
    /// constructing the entity and connecting to the broker; call it again any time you change
    /// <see cref="Config"/> to update Home Assistant's copy.
    /// </summary>
    public virtual async Task PublishDiscoveryAsync(CancellationToken cancellationToken = default)
    {
        var node = JsonSerializer.SerializeToNode(Config, HaJsonOptions.Default)?.AsObject()
            ?? throw new InvalidOperationException("Failed to serialize entity configuration.");

        if (HasStateTopic)
        {
            node["state_topic"] = StateTopic;
        }

        if (CommandTopic is not null)
        {
            node["command_topic"] = CommandTopic;
        }

        ApplyAvailability(node);
        EnrichDiscoveryPayload(node);

        var json = node.ToJsonString(HaJsonOptions.Default);
        await Connection.PublishAsync(DiscoveryTopic, json, retain: true, qos: Config.Qos, cancellationToken).ConfigureAwait(false);

        if (SupportsCommands && CommandTopic is not null && !_subscribed)
        {
            await Connection.SubscribeAsync(CommandTopic, OnMessageReceivedAsync, Config.Qos, cancellationToken).ConfigureAwait(false);
            _subscribed = true;
        }
    }

    /// <summary>Publishes a raw state payload to <see cref="StateTopic"/>.</summary>
    public Task PublishStateAsync(string state, bool retain = true, CancellationToken cancellationToken = default)
        => Connection.PublishAsync(StateTopic, state, retain, Config.Qos, cancellationToken);

    /// <summary>Serializes <paramref name="state"/> to JSON and publishes it to <see cref="StateTopic"/>. Useful for entities with a JSON state schema.</summary>
    public Task PublishStateAsync<TState>(TState state, bool retain = true, CancellationToken cancellationToken = default)
        => PublishStateAsync(JsonSerializer.Serialize(state, HaJsonOptions.Default), retain, cancellationToken);

    /// <summary>
    /// Removes this entity from Home Assistant by publishing an empty retained payload to its
    /// discovery topic, and unsubscribes from its command topic.
    /// </summary>
    public virtual async Task RemoveAsync(CancellationToken cancellationToken = default)
    {
        await Connection.PublishAsync(DiscoveryTopic, string.Empty, retain: true, qos: Config.Qos, cancellationToken).ConfigureAwait(false);

        if (_subscribed && CommandTopic is not null)
        {
            await Connection.UnsubscribeAsync(CommandTopic, OnMessageReceivedAsync, cancellationToken).ConfigureAwait(false);
            _subscribed = false;
        }
    }

    /// <summary>Allows subclasses to add fields to the discovery payload that aren't part of <see cref="EntityConfig"/> directly.</summary>
    protected virtual void EnrichDiscoveryPayload(JsonObject payload)
    {
    }

    /// <summary>Called with the raw payload every time a message arrives on <see cref="CommandTopic"/>. Overridden by controllable entity types.</summary>
    protected virtual Task OnCommandReceivedAsync(string payload) => Task.CompletedTask;

    private Task OnMessageReceivedAsync(string payload) => OnCommandReceivedAsync(payload);

    private void ApplyAvailability(JsonObject node)
    {
        var availability = Config.Availability;
        if (availability is not null)
        {
            node["availability_topic"] = availability.Topic;
            if (!string.Equals(availability.PayloadAvailable, "online", StringComparison.Ordinal))
            {
                node["payload_available"] = availability.PayloadAvailable;
            }

            if (!string.Equals(availability.PayloadNotAvailable, "offline", StringComparison.Ordinal))
            {
                node["payload_not_available"] = availability.PayloadNotAvailable;
            }

            if (availability.ValueTemplate is not null)
            {
                node["availability_template"] = availability.ValueTemplate;
            }
        }
        else if (Connection.AvailabilityTopic is { Length: > 0 })
        {
            node["availability_topic"] = Connection.AvailabilityTopic;
        }
    }

    /// <summary>Unsubscribes from the command topic, if subscribed. Does not remove the entity from Home Assistant - call <see cref="RemoveAsync"/> for that.</summary>
    public virtual async ValueTask DisposeAsync()
    {
        if (_subscribed && CommandTopic is not null)
        {
            await Connection.UnsubscribeAsync(CommandTopic, OnMessageReceivedAsync).ConfigureAwait(false);
            _subscribed = false;
        }
    }
}
