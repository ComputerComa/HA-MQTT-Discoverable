namespace HaMqttDiscoverable.Entities;

/// <summary>
/// A partial snapshot of media player state, for use with <see cref="MediaPlayer.PublishStateAsync(MediaPlayerSnapshot, CancellationToken)"/>.
/// Only the non-null fields are published, so you can report just what changed.
/// </summary>
public sealed class MediaPlayerSnapshot
{
    /// <summary>Whether the player is powered on.</summary>
    public bool? IsOn { get; set; }

    /// <summary>Volume level, from <see cref="MediaPlayerConfig.VolumeMin"/> to <see cref="MediaPlayerConfig.VolumeMax"/> (0.0-1.0 by default).</summary>
    public double? Volume { get; set; }

    /// <summary>Whether the player is muted. Ignored if the <see cref="MediaPlayer"/> wasn't created with <see cref="MediaPlayerConfig.SupportsMute"/>.</summary>
    public bool? IsMuted { get; set; }

    /// <summary>The currently active source. Ignored if the <see cref="MediaPlayer"/> has no configured <see cref="MediaPlayerConfig.Sources"/>.</summary>
    public string? Source { get; set; }

    /// <summary>The playback state - one of the <see cref="MediaPlayerPlaybackState"/> constants.</summary>
    public string? PlaybackState { get; set; }

    /// <summary>The title of the currently playing media.</summary>
    public string? Title { get; set; }

    /// <summary>The artist of the currently playing media.</summary>
    public string? Artist { get; set; }
}
