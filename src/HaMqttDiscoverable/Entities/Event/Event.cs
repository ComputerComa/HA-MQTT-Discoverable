using System.Text.Json;
using System.Text.Json.Nodes;
using HaMqttDiscoverable.Json;

namespace HaMqttDiscoverable.Entities;

/// <summary>A read-only stream of discrete, named events, e.g. a doorbell's "single_press"/"double_press"/"held".</summary>
/// <example>
/// <code>
/// var doorbell = new Event(connection, new EventConfig
/// {
///     Name = "Doorbell",
///     UniqueId = "doorbell",
///     Device = device,
///     EventTypes = new List&lt;string&gt; { "single_press", "double_press", "held" },
///     DeviceClass = "button",
/// });
/// await doorbell.PublishDiscoveryAsync();
/// await doorbell.PublishEventAsync("single_press");
/// </code>
/// </example>
public sealed class Event : HaEntity<EventConfig>
{
    /// <summary>Creates an event entity from the given config.</summary>
    public Event(HaMqttConnection connection, EventConfig config)
        : base(connection, config)
    {
    }

    /// <summary>
    /// Publishes an event. <paramref name="eventType"/> must be one of <see cref="EventConfig.EventTypes"/>.
    /// <paramref name="extraAttributes"/>, if given, are merged into the JSON payload alongside
    /// <c>event_type</c> and exposed as extra state attributes in Home Assistant.
    /// </summary>
    public Task PublishEventAsync(string eventType, IReadOnlyDictionary<string, object?>? extraAttributes = null, CancellationToken cancellationToken = default)
    {
        if (!Config.EventTypes.Contains(eventType))
        {
            throw new ArgumentException($"'{eventType}' is not one of EventConfig.EventTypes.", nameof(eventType));
        }

        var node = new JsonObject { ["event_type"] = eventType };
        if (extraAttributes is not null)
        {
            foreach (var (key, value) in extraAttributes)
            {
                node[key] = JsonSerializer.SerializeToNode(value, HaJsonOptions.Default);
            }
        }

        // Events are meant to be momentary triggers, not persisted state - don't retain them.
        return PublishStateAsync(node.ToJsonString(HaJsonOptions.Default), retain: false, cancellationToken);
    }
}
