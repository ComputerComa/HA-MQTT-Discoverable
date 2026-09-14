using System.Text.Json.Serialization;

namespace HaMqttDiscoverable.Entities;

/// <summary>Configuration for an <see cref="Update"/> entity: reports available firmware/software updates, with an optional "Install" action.</summary>
public sealed class UpdateConfig : EntityConfig
{
    /// <inheritdoc />
    public override string Component => "update";

    /// <summary>Number of decimals to display for the update percentage. Defaults to 0.</summary>
    [JsonPropertyName("display_precision")]
    public int DisplayPrecision { get; set; }

    /// <summary>The payload published back to <see cref="Update.InstallRequested"/> when the user clicks "Install". Defaults to "install".</summary>
    [JsonPropertyName("payload_install")]
    public string PayloadInstall { get; set; } = "install";

    /// <summary>A static title for the software, e.g. "Firmware". Can also be reported per-update via <see cref="UpdateState.Title"/>.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }
}
