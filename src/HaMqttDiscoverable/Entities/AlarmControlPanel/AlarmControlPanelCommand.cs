namespace HaMqttDiscoverable.Entities;

/// <summary>The commands Home Assistant can send to an <see cref="AlarmControlPanel"/>.</summary>
public enum AlarmControlPanelCommand
{
    /// <summary>Arm in away mode.</summary>
    ArmAway,
    /// <summary>Arm in home mode.</summary>
    ArmHome,
    /// <summary>Arm in night mode.</summary>
    ArmNight,
    /// <summary>Arm in vacation mode.</summary>
    ArmVacation,
    /// <summary>Arm with custom bypass.</summary>
    ArmCustomBypass,
    /// <summary>Disarm the panel.</summary>
    Disarm,
    /// <summary>Trigger the alarm.</summary>
    Trigger,
}
