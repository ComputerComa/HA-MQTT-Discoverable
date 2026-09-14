namespace HaMqttDiscoverable.Entities;

/// <summary>
/// Configuration for a <see cref="MediaPlayer"/>. Unlike other entity configs, this isn't a
/// single Home Assistant component - it describes the bundle of real MQTT entities
/// (switch, number, select, sensors, buttons) that <see cref="MediaPlayer"/> creates.
/// </summary>
public sealed class MediaPlayerConfig
{
    /// <summary>
    /// The base name for the media player, e.g. "Living Room TV". Combined with each sub-entity's
    /// short name (e.g. "Power", "Volume") the same way <see cref="EntityConfig.Name"/> is elsewhere
    /// in this library.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The base unique id for the media player. Each sub-entity gets its own unique id derived
    /// from this (e.g. "{UniqueId}_power"). Required.
    /// </summary>
    public string UniqueId { get; set; } = string.Empty;

    /// <summary>The device this media player belongs to. Required.</summary>
    public Device Device { get; set; } = null!;

    /// <summary>
    /// The list of selectable input sources, e.g. ["HDMI 1", "HDMI 2", "Chromecast"]. When empty
    /// (the default), no <see cref="MediaPlayer.Source"/> entity is created.
    /// </summary>
    public List<string> Sources { get; set; } = new();

    /// <summary>When true, a mute <see cref="MediaPlayer.Mute"/> switch is created in addition to the power switch.</summary>
    public bool SupportsMute { get; set; }

    /// <summary>The minimum value reported/accepted by <see cref="MediaPlayer.Volume"/>. Defaults to 0.</summary>
    public double VolumeMin { get; set; } = 0;

    /// <summary>
    /// The maximum value reported/accepted by <see cref="MediaPlayer.Volume"/>. Defaults to 1,
    /// matching Home Assistant's own 0.0-1.0 <c>volume_level</c> convention - keep this at 1
    /// unless you have a specific reason to use a different scale (e.g. 0-100), since Home
    /// Assistant's "Universal Media Player" reads this value directly with no conversion.
    /// </summary>
    public double VolumeMax { get; set; } = 1;

    /// <summary>The step between selectable volume values. Defaults to 0.01.</summary>
    public double VolumeStep { get; set; } = 0.01;
}
