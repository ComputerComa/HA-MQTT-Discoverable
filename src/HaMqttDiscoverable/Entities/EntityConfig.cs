using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>
/// Configuration shared by every discoverable entity type. Subclasses add the fields that are
/// specific to their Home Assistant component (e.g. <c>unit_of_measurement</c> for sensors).
/// </summary>
public abstract class EntityConfig
{
    /// <summary>
    /// The MQTT component Home Assistant should treat this entity as, e.g. <c>sensor</c> or
    /// <c>switch</c>. Used to build the discovery topic; not part of the discovery payload itself.
    /// </summary>
    [JsonIgnore]
    public abstract string Component { get; }

    /// <summary>
    /// The friendly name of the entity. When <see cref="HasEntityName"/> is true (the default)
    /// this is combined with the device's name by Home Assistant, e.g. device "Weather Station"
    /// + entity name "Temperature" becomes "Weather Station Temperature". Leave null to have the
    /// entity take the device's name directly (appropriate for a device that only exposes one
    /// entity).
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// A globally unique id for this entity, used by Home Assistant's entity registry to track
    /// it across restarts and renames. Required.
    /// </summary>
    [JsonPropertyName("unique_id")]
    public string UniqueId { get; set; } = string.Empty;

    /// <summary>
    /// Used to generate `entity_id`, e.g. an object id of "front_door" produces
    /// `sensor.front_door`. Optional; Home Assistant derives one from the name when omitted.
    /// </summary>
    [JsonPropertyName("object_id")]
    public string? ObjectId { get; set; }

    /// <summary>The device this entity belongs to. Required - every entity must belong to a device.</summary>
    [JsonPropertyName("device")]
    public Device Device { get; set; } = null!;

    /// <summary>
    /// When true (the default), <see cref="Name"/> is treated as a suffix of the device's name
    /// rather than a standalone entity name, matching current Home Assistant conventions.
    /// </summary>
    [JsonPropertyName("has_entity_name")]
    public bool HasEntityName { get; set; } = true;

    /// <summary>An icon to display, e.g. "mdi:thermometer".</summary>
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    /// <summary>
    /// The Home Assistant device class for this entity, which controls its icon, unit handling
    /// and UI treatment. The allowed values depend on the entity type - see Home Assistant's
    /// documentation for the component you're configuring, e.g.
    /// https://www.home-assistant.io/integrations/sensor/#device-class.
    /// </summary>
    [JsonPropertyName("device_class")]
    public string? DeviceClass { get; set; }

    /// <summary>Classifies the entity as configuration or diagnostic rather than a primary feature.</summary>
    [JsonPropertyName("entity_category")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public EntityCategory EntityCategory { get; set; } = EntityCategory.None;

    /// <summary>Whether the entity should be enabled when it's first added. Defaults to Home Assistant's own default (true).</summary>
    [JsonPropertyName("enabled_by_default")]
    public bool? EnabledByDefault { get; set; }

    /// <summary>The QoS level used for the entity's MQTT topics. Defaults to 0.</summary>
    [JsonPropertyName("qos")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Qos { get; set; }

    /// <summary>
    /// Overrides the availability topic/payloads used for this specific entity. When null, the
    /// entity uses the shared device availability topic managed by the <see cref="HaMqttConnection"/>
    /// it is created with (if <see cref="MqttSettings.PublishClientAvailability"/> is enabled).
    /// </summary>
    [JsonIgnore]
    public Availability? Availability { get; set; }
}
