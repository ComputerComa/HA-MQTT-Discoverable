using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for a <see cref="Select"/> entity: a value picked from a fixed list of options.</summary>
public sealed class SelectConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "select";

    /// <summary>The list of values the user can pick from. Required.</summary>
    [JsonPropertyName("options")]
    public List<string> Options { get; set; } = new();
}
