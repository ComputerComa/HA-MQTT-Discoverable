using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for an <see cref="Event"/> entity: a read-only stream of discrete, named events (e.g. a doorbell button's "single"/"double"/"long" presses).</summary>
public sealed class EventConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "event";

    /// <summary>The set of event types this entity can report. Required - Home Assistant rejects any event type not listed here.</summary>
    [JsonPropertyName("event_types")]
    public List<string> EventTypes { get; set; } = new();
}
