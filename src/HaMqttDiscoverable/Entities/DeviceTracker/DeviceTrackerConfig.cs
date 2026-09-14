using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for a <see cref="DeviceTracker"/> entity: a read-only "home"/"away"/zone presence report.</summary>
public sealed class DeviceTrackerConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "device_tracker";

    /// <summary>The payload meaning "home". Defaults to "home".</summary>
    [JsonPropertyName("payload_home")]
    public string PayloadHome { get; set; } = "home";

    /// <summary>The payload meaning "away". Defaults to "not_home".</summary>
    [JsonPropertyName("payload_not_home")]
    public string PayloadNotHome { get; set; } = "not_home";

    /// <summary>The payload meaning "unknown location" (clears the tracker's state). Defaults to "None".</summary>
    [JsonPropertyName("payload_reset")]
    public string PayloadReset { get; set; } = "None";

    /// <summary>How this tracker determines location: "gps", "router", "bluetooth", or "bluetooth_le". Defaults to "gps".</summary>
    [JsonPropertyName("source_type")]
    public string SourceType { get; set; } = "gps";
}
