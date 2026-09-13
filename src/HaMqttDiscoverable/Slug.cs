using System.Text;

namespace HaMqttDiscoverable;

/// <summary>Turns arbitrary text into a lowercase, underscore-separated token safe to use as an MQTT topic segment or object id.</summary>
public static class Slug
{
    /// <summary>Slugifies <paramref name="value"/>, e.g. "Front Door!" becomes "front_door".</summary>
    public static string Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value must not be empty.", nameof(value));
        }

        var builder = new StringBuilder(value.Length);
        var lastWasSeparator = false;
        foreach (var c in value)
        {
            if (char.IsLetterOrDigit(c))
            {
                builder.Append(char.ToLowerInvariant(c));
                lastWasSeparator = false;
            }
            else if (!lastWasSeparator && builder.Length > 0)
            {
                builder.Append('_');
                lastWasSeparator = true;
            }
        }

        while (builder.Length > 0 && builder[^1] == '_')
        {
            builder.Length--;
        }

        return builder.Length == 0 ? "entity" : builder.ToString();
    }
}
