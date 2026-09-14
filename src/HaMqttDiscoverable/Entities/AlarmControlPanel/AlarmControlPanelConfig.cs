using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for an <see cref="AlarmControlPanel"/> entity.</summary>
public sealed class AlarmControlPanelConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "alarm_control_panel";

    /// <summary>A code Home Assistant's UI requires the user to enter to arm/disarm/trigger the panel. Not validated by this library - HA enforces it client-side.</summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>Whether a code is required to arm the panel. Defaults to true.</summary>
    [JsonPropertyName("code_arm_required")]
    public bool CodeArmRequired { get; set; } = true;

    /// <summary>Whether a code is required to disarm the panel. Defaults to true.</summary>
    [JsonPropertyName("code_disarm_required")]
    public bool CodeDisarmRequired { get; set; } = true;

    /// <summary>Whether a code is required to trigger the panel. Defaults to true.</summary>
    [JsonPropertyName("code_trigger_required")]
    public bool CodeTriggerRequired { get; set; } = true;

    /// <summary>The payload sent to arm-away. Defaults to "ARM_AWAY".</summary>
    [JsonPropertyName("payload_arm_away")]
    public string PayloadArmAway { get; set; } = "ARM_AWAY";

    /// <summary>The payload sent to arm-home. Defaults to "ARM_HOME".</summary>
    [JsonPropertyName("payload_arm_home")]
    public string PayloadArmHome { get; set; } = "ARM_HOME";

    /// <summary>The payload sent to arm-night. Defaults to "ARM_NIGHT".</summary>
    [JsonPropertyName("payload_arm_night")]
    public string PayloadArmNight { get; set; } = "ARM_NIGHT";

    /// <summary>The payload sent to arm-vacation. Defaults to "ARM_VACATION".</summary>
    [JsonPropertyName("payload_arm_vacation")]
    public string PayloadArmVacation { get; set; } = "ARM_VACATION";

    /// <summary>The payload sent to arm with custom bypass. Defaults to "ARM_CUSTOM_BYPASS".</summary>
    [JsonPropertyName("payload_arm_custom_bypass")]
    public string PayloadArmCustomBypass { get; set; } = "ARM_CUSTOM_BYPASS";

    /// <summary>The payload sent to disarm. Defaults to "DISARM".</summary>
    [JsonPropertyName("payload_disarm")]
    public string PayloadDisarm { get; set; } = "DISARM";

    /// <summary>The payload sent to trigger the alarm. Defaults to "TRIGGER".</summary>
    [JsonPropertyName("payload_trigger")]
    public string PayloadTrigger { get; set; } = "TRIGGER";

    /// <summary>
    /// Restricts which actions Home Assistant exposes, as a subset of "arm_home", "arm_away",
    /// "arm_night", "arm_vacation", "arm_custom_bypass", "trigger". Defaults to all of them.
    /// </summary>
    [JsonPropertyName("supported_features")]
    public List<string>? SupportedFeatures { get; set; }
}
