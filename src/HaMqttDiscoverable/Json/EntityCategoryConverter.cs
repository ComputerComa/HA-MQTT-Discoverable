using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Json;

/// <summary>
/// Serializes <see cref="EntityCategory"/> using Home Assistant's lowercase discovery values,
/// writing nothing at all for <see cref="EntityCategory.None"/> (handled by the property's
/// <c>ShouldSerialize*</c> guard - this converter only needs to handle Config/Diagnostic).
/// </summary>
internal sealed class EntityCategoryConverter : JsonConverter<EntityCategory>
{
    public override EntityCategory Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "config" => EntityCategory.Config,
            "diagnostic" => EntityCategory.Diagnostic,
            _ => EntityCategory.None,
        };
    }

    public override void Write(Utf8JsonWriter writer, EntityCategory value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            EntityCategory.Config => "config",
            EntityCategory.Diagnostic => "diagnostic",
            _ => null,
        });
    }
}
