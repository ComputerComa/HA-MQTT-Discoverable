namespace HaMqttDiscoverable.Entities;

/// <summary>What a <see cref="Climate"/> device is actually doing right now, for use with <see cref="Climate.PublishActionAsync"/>.</summary>
public enum HvacAction
{
    /// <summary>Actively cooling.</summary>
    Cooling,
    /// <summary>Running a defrost cycle.</summary>
    Defrosting,
    /// <summary>Actively dehumidifying.</summary>
    Drying,
    /// <summary>Running the fan only.</summary>
    Fan,
    /// <summary>Actively heating.</summary>
    Heating,
    /// <summary>On, but not currently heating/cooling/etc.</summary>
    Idle,
    /// <summary>Off.</summary>
    Off,
    /// <summary>Preheating before starting the main cycle.</summary>
    Preheating,
}
