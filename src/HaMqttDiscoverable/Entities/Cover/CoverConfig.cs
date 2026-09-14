using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for a <see cref="Cover"/> entity: a blind, garage door, shade, etc. that can be opened/closed/stopped, optionally with position and tilt.</summary>
public sealed class CoverConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "cover";

    /// <summary>The payload sent to open. Defaults to "OPEN".</summary>
    [JsonPropertyName("payload_open")]
    public string PayloadOpen { get; set; } = "OPEN";

    /// <summary>The payload sent to close. Defaults to "CLOSE".</summary>
    [JsonPropertyName("payload_close")]
    public string PayloadClose { get; set; } = "CLOSE";

    /// <summary>The payload sent to stop mid-movement. Defaults to "STOP".</summary>
    [JsonPropertyName("payload_stop")]
    public string PayloadStop { get; set; } = "STOP";

    /// <summary>The state value meaning "open". Defaults to "open".</summary>
    [JsonPropertyName("state_open")]
    public string StateOpen { get; set; } = "open";

    /// <summary>The state value meaning "closed". Defaults to "closed".</summary>
    [JsonPropertyName("state_closed")]
    public string StateClosed { get; set; } = "closed";

    /// <summary>The state value meaning "opening" (in progress). Defaults to "opening".</summary>
    [JsonPropertyName("state_opening")]
    public string StateOpening { get; set; } = "opening";

    /// <summary>The state value meaning "closing" (in progress). Defaults to "closing".</summary>
    [JsonPropertyName("state_closing")]
    public string StateClosing { get; set; } = "closing";

    /// <summary>The state value meaning "stopped" partway. Defaults to "stopped".</summary>
    [JsonPropertyName("state_stopped")]
    public string StateStopped { get; set; } = "stopped";

    /// <summary>The position value meaning fully closed. Defaults to 0.</summary>
    [JsonPropertyName("position_closed")]
    public int PositionClosed { get; set; }

    /// <summary>The position value meaning fully open. Defaults to 100.</summary>
    [JsonPropertyName("position_open")]
    public int PositionOpen { get; set; } = 100;

    /// <summary>
    /// When true, this cover reports (and, via <see cref="Cover.PositionCommandReceived"/>, accepts)
    /// a 0-100 position rather than just open/closed.
    /// </summary>
    [JsonIgnore]
    public bool SupportsPosition { get; set; }

    /// <summary>When true, this cover supports independent tilt control (e.g. venetian blinds).</summary>
    [JsonIgnore]
    public bool SupportsTilt { get; set; }

    /// <summary>The tilt value meaning fully closed. Defaults to 0.</summary>
    [JsonPropertyName("tilt_closed_value")]
    public int TiltClosedPosition { get; set; }

    /// <summary>The tilt value meaning fully open. Defaults to 100.</summary>
    [JsonPropertyName("tilt_opened_value")]
    public int TiltOpenPosition { get; set; } = 100;

    /// <summary>The minimum tilt value. Defaults to 0.</summary>
    [JsonPropertyName("tilt_min")]
    public int TiltMin { get; set; }

    /// <summary>The maximum tilt value. Defaults to 100.</summary>
    [JsonPropertyName("tilt_max")]
    public int TiltMax { get; set; } = 100;
}
