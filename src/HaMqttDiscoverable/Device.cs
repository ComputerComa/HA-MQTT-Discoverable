using System.Text.Json.Serialization;

namespace HaMqttDiscoverable;

/// <summary>
/// Describes the physical device an entity belongs to, as understood by Home Assistant's
/// device registry. Entities that share the same <see cref="Identifiers"/> are grouped under
/// a single device in Home Assistant.
/// </summary>
public sealed class Device
{
    /// <summary>
    /// One or more unique identifiers for the device (e.g. a serial number). At least one
    /// identifier or a <see cref="Connections"/> entry is required for Home Assistant to be
    /// able to link entities to this device.
    /// </summary>
    [JsonPropertyName("identifiers")]
    public List<string> Identifiers { get; set; } = new();

    /// <summary>
    /// A list of [connection_type, connection_identifier] tuples, e.g. ["mac", "AA:BB:CC:DD:EE:FF"].
    /// </summary>
    [JsonPropertyName("connections")]
    public List<List<string>>? Connections { get; set; }

    /// <summary>The name of the device as shown in Home Assistant.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>The manufacturer of the device.</summary>
    [JsonPropertyName("manufacturer")]
    public string? Manufacturer { get; set; }

    /// <summary>The model name of the device.</summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>The model identifier/SKU of the device.</summary>
    [JsonPropertyName("model_id")]
    public string? ModelId { get; set; }

    /// <summary>The firmware/software version running on the device.</summary>
    [JsonPropertyName("sw_version")]
    public string? SoftwareVersion { get; set; }

    /// <summary>The hardware version of the device.</summary>
    [JsonPropertyName("hw_version")]
    public string? HardwareVersion { get; set; }

    /// <summary>A URL that shows more info about the device.</summary>
    [JsonPropertyName("configuration_url")]
    public string? ConfigurationUrl { get; set; }

    /// <summary>
    /// The identifier of a device that "routes" messages for this device, e.g. a hub or
    /// gateway. Must match the <see cref="Identifiers"/> of another discovered device.
    /// </summary>
    [JsonPropertyName("via_device")]
    public string? ViaDevice { get; set; }

    /// <summary>Suggests an area in Home Assistant this device should be placed in.</summary>
    [JsonPropertyName("suggested_area")]
    public string? SuggestedArea { get; set; }

    /// <summary>
    /// Creates a device with a single identifier, which covers the common case.
    /// </summary>
    public static Device Create(string identifier, string name, string? manufacturer = null, string? model = null, string? softwareVersion = null)
        => new()
        {
            Identifiers = new List<string> { identifier },
            Name = name,
            Manufacturer = manufacturer,
            Model = model,
            SoftwareVersion = softwareVersion,
        };
}
