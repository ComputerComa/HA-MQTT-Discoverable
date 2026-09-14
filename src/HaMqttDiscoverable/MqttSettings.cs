namespace HaMqttDiscoverable;

/// <summary>
/// Connection settings for the MQTT broker used to talk to Home Assistant.
/// </summary>
public sealed class MqttSettings
{
    /// <summary>Hostname or IP address of the MQTT broker.</summary>
    public string Host { get; set; } = "localhost";

    /// <summary>TCP port of the MQTT broker. Defaults to 1883 (or 8883 when <see cref="UseTls"/> is true).</summary>
    public int? Port { get; set; }

    /// <summary>Username used to authenticate with the broker, if required.</summary>
    public string? Username { get; set; }

    /// <summary>Password used to authenticate with the broker, if required.</summary>
    public string? Password { get; set; }

    /// <summary>Whether to connect over TLS.</summary>
    public bool UseTls { get; set; }

    /// <summary>
    /// The MQTT client id to use. When omitted, a random id is generated so multiple
    /// instances of your application don't collide.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>The `homeassistant` discovery prefix configured on the Home Assistant side. Defaults to "homeassistant".</summary>
    public string DiscoveryPrefix { get; set; } = "homeassistant";

    /// <summary>
    /// When true (the default), the connection publishes a device-wide availability topic and
    /// registers it as an MQTT last-will message, so Home Assistant marks every entity that
    /// doesn't define its own <see cref="Availability"/> unavailable if the process dies
    /// unexpectedly.
    /// </summary>
    public bool PublishClientAvailability { get; set; } = true;

    /// <summary>Resolves the effective port, taking <see cref="UseTls"/> into account.</summary>
    public int EffectivePort => Port ?? (UseTls ? 8883 : 1883);
}
