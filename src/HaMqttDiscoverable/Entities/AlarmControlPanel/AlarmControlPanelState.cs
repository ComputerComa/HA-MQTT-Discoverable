namespace HaMqttDiscoverable.Entities;

/// <summary>The state values Home Assistant's alarm control panel component understands, for use with <see cref="HaEntity{TConfig}.PublishStateAsync(string, bool, CancellationToken)"/>.</summary>
public static class AlarmControlPanelState
{
    /// <summary>The panel is disarmed.</summary>
    public const string Disarmed = "disarmed";

    /// <summary>The panel is armed in home mode.</summary>
    public const string ArmedHome = "armed_home";

    /// <summary>The panel is armed in away mode.</summary>
    public const string ArmedAway = "armed_away";

    /// <summary>The panel is armed in night mode.</summary>
    public const string ArmedNight = "armed_night";

    /// <summary>The panel is armed in vacation mode.</summary>
    public const string ArmedVacation = "armed_vacation";

    /// <summary>The panel is armed with custom bypass.</summary>
    public const string ArmedCustomBypass = "armed_custom_bypass";

    /// <summary>The panel is pending arm/disarm.</summary>
    public const string Pending = "pending";

    /// <summary>The panel is in the process of arming.</summary>
    public const string Arming = "arming";

    /// <summary>The panel is in the process of disarming.</summary>
    public const string Disarming = "disarming";

    /// <summary>The alarm has been triggered.</summary>
    public const string Triggered = "triggered";
}
