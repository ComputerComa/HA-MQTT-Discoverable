namespace HaMqttDiscoverable.Entities;

/// <summary>
/// The playback state values Home Assistant's media player component understands, for use with
/// <see cref="MediaPlayer.PublishPlaybackStateAsync"/>. Matches `homeassistant.components.media_player.MediaPlayerState`.
/// </summary>
public static class MediaPlayerPlaybackState
{
    /// <summary>The player is off.</summary>
    public const string Off = "off";

    /// <summary>The player is on, but no further detail is known.</summary>
    public const string On = "on";

    /// <summary>The player is on and accepting commands, but nothing is playing.</summary>
    public const string Idle = "idle";

    /// <summary>Media is actively playing.</summary>
    public const string Playing = "playing";

    /// <summary>Media is loaded but paused.</summary>
    public const string Paused = "paused";

    /// <summary>The player is preparing to start playback.</summary>
    public const string Buffering = "buffering";
}
