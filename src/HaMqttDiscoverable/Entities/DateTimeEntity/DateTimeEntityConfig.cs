using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for a <see cref="DateTimeEntity"/>: a combined date and time value that can be read and set.</summary>
public sealed class DateTimeEntityConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "datetime";

    /// <summary>
    /// The timezone Home Assistant should interpret naive (offset-less) values in. Uses an
    /// IANA timezone name, e.g. "Europe/Amsterdam". Defaults to Home Assistant's own configured
    /// timezone.
    /// </summary>
    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }
}
