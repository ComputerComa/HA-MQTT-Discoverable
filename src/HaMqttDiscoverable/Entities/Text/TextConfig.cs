using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for a <see cref="Text"/> entity: a free-form string value that can be read and set.</summary>
public sealed class TextConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "text";

    /// <summary>The minimum length of the text. Defaults to 0.</summary>
    [JsonPropertyName("min")]
    public int? Min { get; set; }

    /// <summary>The maximum length of the text. Defaults to 255.</summary>
    [JsonPropertyName("max")]
    public int? Max { get; set; }

    /// <summary>A regular expression the value must match.</summary>
    [JsonPropertyName("pattern")]
    public string? Pattern { get; set; }

    /// <summary>How the field should be displayed: "text" (default) or "password".</summary>
    [JsonPropertyName("mode")]
    public string? Mode { get; set; }
}
