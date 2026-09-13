using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Json;

/// <summary>Shared JSON settings used to build and parse Home Assistant MQTT discovery payloads.</summary>
public static class HaJsonOptions
{
    /// <summary>
    /// Options matching Home Assistant's discovery schema: snake_case property names, enums
    /// written as their lowercase Home Assistant values, and null/default fields omitted so
    /// discovery payloads stay minimal.
    /// </summary>
    public static readonly JsonSerializerOptions Default = Create();

    private static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
        };
        options.Converters.Add(new EntityCategoryConverter());
        options.Converters.Add(new JsonStringEnumConverter(SnakeCaseNamingPolicy.Instance));
        return options;
    }
}
