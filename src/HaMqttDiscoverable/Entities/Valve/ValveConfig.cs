using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for a <see cref="Valve"/> entity: open/close (optionally with a stop action), or an open-to-a-position valve.</summary>
public sealed class ValveConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "valve";

    /// <summary>
    /// When true, the valve reports and accepts a 0-100 position instead of just open/closed.
    /// When true, <see cref="PayloadOpen"/>/<see cref="PayloadClose"/>/<see cref="StateOpen"/>/<see cref="StateClosed"/> aren't used.
    /// </summary>
    [JsonPropertyName("reports_position")]
    public bool ReportsPosition { get; set; }

    /// <summary>The payload sent to open the valve (non-position mode). Defaults to "OPEN".</summary>
    [JsonPropertyName("payload_open")]
    public string? PayloadOpen { get; set; } = "OPEN";

    /// <summary>The payload sent to close the valve (non-position mode). Defaults to "CLOSE".</summary>
    [JsonPropertyName("payload_close")]
    public string? PayloadClose { get; set; } = "CLOSE";

    /// <summary>The payload sent to stop mid-movement, if supported. Null (the default) means "stop" isn't supported.</summary>
    [JsonPropertyName("payload_stop")]
    public string? PayloadStop { get; set; }

    /// <summary>The state value meaning "open" (non-position mode). Defaults to "open".</summary>
    [JsonPropertyName("state_open")]
    public string? StateOpen { get; set; } = "open";

    /// <summary>The state value meaning "closed" (non-position mode). Defaults to "closed".</summary>
    [JsonPropertyName("state_closed")]
    public string? StateClosed { get; set; } = "closed";

    /// <summary>The state value meaning "opening" (in progress). Defaults to "opening".</summary>
    [JsonPropertyName("state_opening")]
    public string StateOpening { get; set; } = "opening";

    /// <summary>The state value meaning "closing" (in progress). Defaults to "closing".</summary>
    [JsonPropertyName("state_closing")]
    public string StateClosing { get; set; } = "closing";

    /// <summary>The position value meaning fully closed, in position mode. Defaults to 0.</summary>
    [JsonPropertyName("position_closed")]
    public int PositionClosed { get; set; }

    /// <summary>The position value meaning fully open, in position mode. Defaults to 100.</summary>
    [JsonPropertyName("position_open")]
    public int PositionOpen { get; set; } = 100;
}
