using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for a <see cref="Siren"/> entity: an on/off alarm sounder, optionally with tones.</summary>
public sealed class SirenConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "siren";

    /// <summary>The payload sent/expected to mean "on". Defaults to "ON".</summary>
    [JsonPropertyName("payload_on")]
    public string PayloadOn { get; set; } = "ON";

    /// <summary>The payload sent/expected to mean "off". Defaults to "OFF".</summary>
    [JsonPropertyName("payload_off")]
    public string PayloadOff { get; set; } = "OFF";

    /// <summary>Overrides the state value meaning "on", if it differs from <see cref="PayloadOn"/>.</summary>
    [JsonPropertyName("state_on")]
    public string? StateOn { get; set; }

    /// <summary>Overrides the state value meaning "off", if it differs from <see cref="PayloadOff"/>.</summary>
    [JsonPropertyName("state_off")]
    public string? StateOff { get; set; }

    /// <summary>
    /// The tones this siren can play, shown in Home Assistant's UI. Selecting one doesn't
    /// change the MQTT payload sent unless you handle it yourself - see the <see cref="Siren"/>
    /// class remarks.
    /// </summary>
    [JsonPropertyName("available_tones")]
    public List<string>? AvailableTones { get; set; }

    /// <summary>Whether Home Assistant should show a duration control. Defaults to true.</summary>
    [JsonPropertyName("support_duration")]
    public bool SupportDuration { get; set; } = true;

    /// <summary>Whether Home Assistant should show a volume control. Defaults to true.</summary>
    [JsonPropertyName("support_volume_set")]
    public bool SupportVolumeSet { get; set; } = true;
}
