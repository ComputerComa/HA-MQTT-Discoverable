using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for a <see cref="LockEntity"/>.</summary>
public sealed class LockConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "lock";

    /// <summary>A regular expression Home Assistant's UI uses to validate a code before sending a command, if you require one. Not enforced by this library.</summary>
    [JsonPropertyName("code_format")]
    public string? CodeFormat { get; set; }

    /// <summary>The payload sent to lock. Defaults to "LOCK".</summary>
    [JsonPropertyName("payload_lock")]
    public string PayloadLock { get; set; } = "LOCK";

    /// <summary>The payload sent to unlock. Defaults to "UNLOCK".</summary>
    [JsonPropertyName("payload_unlock")]
    public string PayloadUnlock { get; set; } = "UNLOCK";

    /// <summary>The payload sent to open (e.g. a latch), if this lock supports it. Null (the default) means "open" isn't supported.</summary>
    [JsonPropertyName("payload_open")]
    public string? PayloadOpen { get; set; }

    /// <summary>The state value meaning "locked". Defaults to "LOCKED".</summary>
    [JsonPropertyName("state_locked")]
    public string StateLocked { get; set; } = "LOCKED";

    /// <summary>The state value meaning "unlocked". Defaults to "UNLOCKED".</summary>
    [JsonPropertyName("state_unlocked")]
    public string StateUnlocked { get; set; } = "UNLOCKED";

    /// <summary>The state value meaning "locking" (in progress). Defaults to "LOCKING".</summary>
    [JsonPropertyName("state_locking")]
    public string StateLocking { get; set; } = "LOCKING";

    /// <summary>The state value meaning "unlocking" (in progress). Defaults to "UNLOCKING".</summary>
    [JsonPropertyName("state_unlocking")]
    public string StateUnlocking { get; set; } = "UNLOCKING";

    /// <summary>The state value meaning "jammed". Defaults to "JAMMED".</summary>
    [JsonPropertyName("state_jammed")]
    public string StateJammed { get; set; } = "JAMMED";

    /// <summary>The state value meaning "open". Defaults to "OPEN".</summary>
    [JsonPropertyName("state_open")]
    public string StateOpen { get; set; } = "OPEN";

    /// <summary>The state value meaning "opening" (in progress). Defaults to "OPENING".</summary>
    [JsonPropertyName("state_opening")]
    public string StateOpening { get; set; } = "OPENING";
}
