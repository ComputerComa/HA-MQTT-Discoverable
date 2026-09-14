using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for a <see cref="Vacuum"/> entity: a robot vacuum with start/pause/stop/return/locate controls and an optional fan speed.</summary>
public sealed class VacuumConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "vacuum";

    /// <summary>The payload meaning "start cleaning". Defaults to "start".</summary>
    [JsonPropertyName("payload_start")]
    public string PayloadStart { get; set; } = "start";

    /// <summary>The payload meaning "pause". Defaults to "pause".</summary>
    [JsonPropertyName("payload_pause")]
    public string PayloadPause { get; set; } = "pause";

    /// <summary>The payload meaning "stop". Defaults to "stop".</summary>
    [JsonPropertyName("payload_stop")]
    public string PayloadStop { get; set; } = "stop";

    /// <summary>The payload meaning "return to the dock". Defaults to "return_to_base".</summary>
    [JsonPropertyName("payload_return_to_base")]
    public string PayloadReturnToBase { get; set; } = "return_to_base";

    /// <summary>The payload meaning "clean the current spot". Defaults to "clean_spot".</summary>
    [JsonPropertyName("payload_clean_spot")]
    public string PayloadCleanSpot { get; set; } = "clean_spot";

    /// <summary>The payload meaning "locate" (e.g. play a sound). Defaults to "locate".</summary>
    [JsonPropertyName("payload_locate")]
    public string PayloadLocate { get; set; } = "locate";

    /// <summary>
    /// Restricts which actions Home Assistant exposes, as a subset of "start", "pause", "stop",
    /// "return_home", "fan_speed", "clean_spot", "locate". Defaults to "start", "stop",
    /// "return_home", "clean_spot".
    /// </summary>
    [JsonPropertyName("supported_features")]
    public List<string>? SupportedFeatures { get; set; }

    /// <summary>
    /// The fan speeds this vacuum supports (e.g. "low", "medium", "high"). When non-empty, a fan
    /// speed command topic is added - see <see cref="Vacuum.FanSpeedCommandReceived"/>.
    /// </summary>
    [JsonPropertyName("fan_speed_list")]
    public List<string> FanSpeedList { get; set; } = new();
}
