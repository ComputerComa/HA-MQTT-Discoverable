namespace HaMqttDiscoverable;

/// <summary>Classifies an entity for Home Assistant's UI, matching the `entity_category` discovery field.</summary>
public enum EntityCategory
{
    /// <summary>A regular, primary entity (the default; not sent to Home Assistant).</summary>
    None,

    /// <summary>An entity exposing configuration that doesn't need to be prominent in the main UI.</summary>
    Config,

    /// <summary>An entity exposing non-primary, diagnostic information.</summary>
    Diagnostic,
}
