using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>The JSON body published to an <see cref="Update"/> entity's state topic.</summary>
public sealed class UpdateState
{
    /// <summary>The currently installed version. Required for Home Assistant to know whether an update is available.</summary>
    [JsonPropertyName("installed_version")]
    public string? InstalledVersion { get; set; }

    /// <summary>The latest available version. Home Assistant shows an update as available when this differs from <see cref="InstalledVersion"/>.</summary>
    [JsonPropertyName("latest_version")]
    public string? LatestVersion { get; set; }

    /// <summary>Overrides <see cref="UpdateConfig.Title"/> for this specific update.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>A short summary of what's in the release, shown in Home Assistant's UI (supports Markdown).</summary>
    [JsonPropertyName("release_summary")]
    public string? ReleaseSummary { get; set; }

    /// <summary>A URL with more information about the release.</summary>
    [JsonPropertyName("release_url")]
    public string? ReleaseUrl { get; set; }

    /// <summary>A URL for an image representing the software (e.g. an app icon).</summary>
    [JsonPropertyName("entity_picture")]
    public string? EntityPicture { get; set; }

    /// <summary>Whether an update/install is currently in progress.</summary>
    [JsonPropertyName("in_progress")]
    public bool? InProgress { get; set; }

    /// <summary>The install progress, 0-100, or null if not known/applicable.</summary>
    [JsonPropertyName("update_percentage")]
    public int? UpdatePercentage { get; set; }
}
