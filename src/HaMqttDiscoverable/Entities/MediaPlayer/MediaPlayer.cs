namespace HaMqttDiscoverable.Entities;

/// <summary>
/// A media player, built from several individually-discoverable MQTT entities (a power switch,
/// a volume number, optionally a mute switch and a source select, a handful of sensors for
/// metadata, and transport buttons).
/// </summary>
/// <remarks>
/// Home Assistant's built-in MQTT integration has no native <c>media_player</c> platform - only
/// component types like sensor, switch, number, select, etc. are MQTT-discoverable. This class
/// works around that by publishing a bundle of those real entities under one device. To present
/// them to Home Assistant as a single <c>media_player</c> entity, wire them together with Home
/// Assistant's built-in "Universal Media Player" (<c>universal</c>) platform - see the README
/// for a ready-to-use YAML example.
/// </remarks>
/// <example>
/// <code>
/// var player = new MediaPlayer(connection, new MediaPlayerConfig
/// {
///     Name = "Living Room TV",
///     UniqueId = "living-room-tv",
///     Device = device,
///     Sources = new List&lt;string&gt; { "HDMI 1", "HDMI 2", "Chromecast" },
/// });
///
/// player.PowerCommandReceived += async isOn => { SetPower(isOn); await player.PublishPowerAsync(isOn); };
/// player.PlayRequested += async () => { Play(); await player.PublishPlaybackStateAsync(MediaPlayerPlaybackState.Playing); };
///
/// await player.PublishDiscoveryAsync();
/// </code>
/// </example>
public sealed class MediaPlayer : IAsyncDisposable
{
    /// <summary>Turns the player on/off.</summary>
    public Switch Power { get; }

    /// <summary>The current volume, from <see cref="MediaPlayerConfig.VolumeMin"/> to <see cref="MediaPlayerConfig.VolumeMax"/>.</summary>
    public Number Volume { get; }

    /// <summary>Mutes/unmutes the player, or null if the config didn't set <see cref="MediaPlayerConfig.SupportsMute"/>.</summary>
    public Switch? Mute { get; }

    /// <summary>Selects the active input source, or null if the config had no <see cref="MediaPlayerConfig.Sources"/>.</summary>
    public Select? Source { get; }

    /// <summary>Reports playback state - publish one of the <see cref="MediaPlayerPlaybackState"/> constants.</summary>
    public Sensor PlaybackState { get; }

    /// <summary>Reports the title of the currently playing media.</summary>
    public Sensor Title { get; }

    /// <summary>Reports the artist of the currently playing media.</summary>
    public Sensor Artist { get; }

    /// <summary>Requests playback start.</summary>
    public Button Play { get; }

    /// <summary>Requests playback pause.</summary>
    public Button Pause { get; }

    /// <summary>Requests playback stop.</summary>
    public Button Stop { get; }

    /// <summary>Requests skipping to the next track.</summary>
    public Button Next { get; }

    /// <summary>Requests skipping to the previous track.</summary>
    public Button Previous { get; }

    /// <summary>Raised when Home Assistant sends a power command.</summary>
    public event Func<bool, Task>? PowerCommandReceived;

    /// <summary>Raised when Home Assistant sends a new volume level.</summary>
    public event Func<double, Task>? VolumeCommandReceived;

    /// <summary>Raised when Home Assistant sends a mute command. Never raised if <see cref="Mute"/> is null.</summary>
    public event Func<bool, Task>? MuteCommandReceived;

    /// <summary>Raised when Home Assistant sends a new source selection. Never raised if <see cref="Source"/> is null.</summary>
    public event Func<string, Task>? SourceCommandReceived;

    /// <summary>Raised when Home Assistant requests playback start.</summary>
    public event Func<Task>? PlayRequested;

    /// <summary>Raised when Home Assistant requests playback pause.</summary>
    public event Func<Task>? PauseRequested;

    /// <summary>Raised when Home Assistant requests playback stop.</summary>
    public event Func<Task>? StopRequested;

    /// <summary>Raised when Home Assistant requests skipping to the next track.</summary>
    public event Func<Task>? NextTrackRequested;

    /// <summary>Raised when Home Assistant requests skipping to the previous track.</summary>
    public event Func<Task>? PreviousTrackRequested;

    /// <summary>Creates a media player's sub-entities from the given config. Call <see cref="PublishDiscoveryAsync"/> to publish all of them.</summary>
    public MediaPlayer(HaMqttConnection connection, MediaPlayerConfig config)
    {
        if (connection is null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        if (config is null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        if (string.IsNullOrWhiteSpace(config.UniqueId))
        {
            throw new ArgumentException("MediaPlayerConfig.UniqueId is required.", nameof(config));
        }

        Power = new Switch(connection, new SwitchConfig
        {
            Name = "Power",
            UniqueId = $"{config.UniqueId}_power",
            Device = config.Device,
            Icon = "mdi:power",
        });
        Power.CommandReceived += async isOn =>
        {
            if (PowerCommandReceived is not null)
            {
                await PowerCommandReceived.Invoke(isOn).ConfigureAwait(false);
            }
        };

        Volume = new Number(connection, new NumberConfig
        {
            Name = "Volume",
            UniqueId = $"{config.UniqueId}_volume",
            Device = config.Device,
            Icon = "mdi:volume-high",
            Min = config.VolumeMin,
            Max = config.VolumeMax,
            Step = config.VolumeStep,
            Mode = "slider",
        });
        Volume.CommandReceived += async volume =>
        {
            if (VolumeCommandReceived is not null)
            {
                await VolumeCommandReceived.Invoke(volume).ConfigureAwait(false);
            }
        };

        if (config.SupportsMute)
        {
            Mute = new Switch(connection, new SwitchConfig
            {
                Name = "Mute",
                UniqueId = $"{config.UniqueId}_mute",
                Device = config.Device,
                Icon = "mdi:volume-mute",
            });
            Mute.CommandReceived += async isMuted =>
            {
                if (MuteCommandReceived is not null)
                {
                    await MuteCommandReceived.Invoke(isMuted).ConfigureAwait(false);
                }
            };
        }

        if (config.Sources.Count > 0)
        {
            Source = new Select(connection, new SelectConfig
            {
                Name = "Source",
                UniqueId = $"{config.UniqueId}_source",
                Device = config.Device,
                Icon = "mdi:import",
                Options = config.Sources,
            });
            Source.CommandReceived += async source =>
            {
                if (SourceCommandReceived is not null)
                {
                    await SourceCommandReceived.Invoke(source).ConfigureAwait(false);
                }
            };
        }

        PlaybackState = new Sensor(connection, new SensorConfig
        {
            Name = "State",
            UniqueId = $"{config.UniqueId}_state",
            Device = config.Device,
            Icon = "mdi:play-circle-outline",
        });

        Title = new Sensor(connection, new SensorConfig
        {
            Name = "Title",
            UniqueId = $"{config.UniqueId}_title",
            Device = config.Device,
            Icon = "mdi:music-note",
        });

        Artist = new Sensor(connection, new SensorConfig
        {
            Name = "Artist",
            UniqueId = $"{config.UniqueId}_artist",
            Device = config.Device,
            Icon = "mdi:account-music",
        });

        Play = CreateTransportButton(connection, config, "Play", "play", "mdi:play", () => PlayRequested);
        Pause = CreateTransportButton(connection, config, "Pause", "pause", "mdi:pause", () => PauseRequested);
        Stop = CreateTransportButton(connection, config, "Stop", "stop", "mdi:stop", () => StopRequested);
        Next = CreateTransportButton(connection, config, "Next", "next", "mdi:skip-next", () => NextTrackRequested);
        Previous = CreateTransportButton(connection, config, "Previous", "previous", "mdi:skip-previous", () => PreviousTrackRequested);
    }

    private static Button CreateTransportButton(
        HaMqttConnection connection,
        MediaPlayerConfig config,
        string name,
        string idSuffix,
        string icon,
        Func<Func<Task>?> getHandler)
    {
        var button = new Button(connection, new ButtonConfig
        {
            Name = name,
            UniqueId = $"{config.UniqueId}_{idSuffix}",
            Device = config.Device,
            Icon = icon,
        });
        button.Pressed += async () =>
        {
            var handler = getHandler();
            if (handler is not null)
            {
                await handler.Invoke().ConfigureAwait(false);
            }
        };
        return button;
    }

    /// <summary>Publishes the Home Assistant discovery payload for every sub-entity.</summary>
    public async Task PublishDiscoveryAsync(CancellationToken cancellationToken = default)
    {
        await Power.PublishDiscoveryAsync(cancellationToken).ConfigureAwait(false);
        await Volume.PublishDiscoveryAsync(cancellationToken).ConfigureAwait(false);

        if (Mute is not null)
        {
            await Mute.PublishDiscoveryAsync(cancellationToken).ConfigureAwait(false);
        }

        if (Source is not null)
        {
            await Source.PublishDiscoveryAsync(cancellationToken).ConfigureAwait(false);
        }

        await PlaybackState.PublishDiscoveryAsync(cancellationToken).ConfigureAwait(false);
        await Title.PublishDiscoveryAsync(cancellationToken).ConfigureAwait(false);
        await Artist.PublishDiscoveryAsync(cancellationToken).ConfigureAwait(false);
        await Play.PublishDiscoveryAsync(cancellationToken).ConfigureAwait(false);
        await Pause.PublishDiscoveryAsync(cancellationToken).ConfigureAwait(false);
        await Stop.PublishDiscoveryAsync(cancellationToken).ConfigureAwait(false);
        await Next.PublishDiscoveryAsync(cancellationToken).ConfigureAwait(false);
        await Previous.PublishDiscoveryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Removes every sub-entity from Home Assistant.</summary>
    public async Task RemoveAsync(CancellationToken cancellationToken = default)
    {
        await Power.RemoveAsync(cancellationToken).ConfigureAwait(false);
        await Volume.RemoveAsync(cancellationToken).ConfigureAwait(false);

        if (Mute is not null)
        {
            await Mute.RemoveAsync(cancellationToken).ConfigureAwait(false);
        }

        if (Source is not null)
        {
            await Source.RemoveAsync(cancellationToken).ConfigureAwait(false);
        }

        await PlaybackState.RemoveAsync(cancellationToken).ConfigureAwait(false);
        await Title.RemoveAsync(cancellationToken).ConfigureAwait(false);
        await Artist.RemoveAsync(cancellationToken).ConfigureAwait(false);
        await Play.RemoveAsync(cancellationToken).ConfigureAwait(false);
        await Pause.RemoveAsync(cancellationToken).ConfigureAwait(false);
        await Stop.RemoveAsync(cancellationToken).ConfigureAwait(false);
        await Next.RemoveAsync(cancellationToken).ConfigureAwait(false);
        await Previous.RemoveAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Publishes the player's power state.</summary>
    public Task PublishPowerAsync(bool isOn, CancellationToken cancellationToken = default)
        => Power.PublishStateAsync(isOn, cancellationToken: cancellationToken);

    /// <summary>Publishes the player's current volume level.</summary>
    public Task PublishVolumeAsync(double volume, CancellationToken cancellationToken = default)
        => Volume.PublishStateAsync(volume, cancellationToken: cancellationToken);

    /// <summary>Publishes the player's mute state. Does nothing if <see cref="Mute"/> is null.</summary>
    public Task PublishMuteAsync(bool isMuted, CancellationToken cancellationToken = default)
        => Mute?.PublishStateAsync(isMuted, cancellationToken: cancellationToken) ?? Task.CompletedTask;

    /// <summary>Publishes the player's active source. Does nothing if <see cref="Source"/> is null.</summary>
    public Task PublishSourceAsync(string source, CancellationToken cancellationToken = default)
        => Source?.PublishStateAsync(source, cancellationToken: cancellationToken) ?? Task.CompletedTask;

    /// <summary>Publishes the player's playback state - use one of the <see cref="MediaPlayerPlaybackState"/> constants.</summary>
    public Task PublishPlaybackStateAsync(string state, CancellationToken cancellationToken = default)
        => PlaybackState.PublishStateAsync(state, cancellationToken: cancellationToken);

    /// <summary>Publishes the title of the currently playing media.</summary>
    public Task PublishTitleAsync(string title, CancellationToken cancellationToken = default)
        => Title.PublishStateAsync(title, cancellationToken: cancellationToken);

    /// <summary>Publishes the artist of the currently playing media.</summary>
    public Task PublishArtistAsync(string artist, CancellationToken cancellationToken = default)
        => Artist.PublishStateAsync(artist, cancellationToken: cancellationToken);

    /// <summary>Publishes every non-null field of <paramref name="snapshot"/>.</summary>
    public async Task PublishStateAsync(MediaPlayerSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        if (snapshot.IsOn is { } isOn)
        {
            await PublishPowerAsync(isOn, cancellationToken).ConfigureAwait(false);
        }

        if (snapshot.Volume is { } volume)
        {
            await PublishVolumeAsync(volume, cancellationToken).ConfigureAwait(false);
        }

        if (snapshot.IsMuted is { } isMuted)
        {
            await PublishMuteAsync(isMuted, cancellationToken).ConfigureAwait(false);
        }

        if (snapshot.Source is not null)
        {
            await PublishSourceAsync(snapshot.Source, cancellationToken).ConfigureAwait(false);
        }

        if (snapshot.PlaybackState is not null)
        {
            await PublishPlaybackStateAsync(snapshot.PlaybackState, cancellationToken).ConfigureAwait(false);
        }

        if (snapshot.Title is not null)
        {
            await PublishTitleAsync(snapshot.Title, cancellationToken).ConfigureAwait(false);
        }

        if (snapshot.Artist is not null)
        {
            await PublishArtistAsync(snapshot.Artist, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>Unsubscribes every sub-entity's command topic. Does not remove anything from Home Assistant - call <see cref="RemoveAsync"/> for that.</summary>
    public async ValueTask DisposeAsync()
    {
        await Power.DisposeAsync().ConfigureAwait(false);
        await Volume.DisposeAsync().ConfigureAwait(false);

        if (Mute is not null)
        {
            await Mute.DisposeAsync().ConfigureAwait(false);
        }

        if (Source is not null)
        {
            await Source.DisposeAsync().ConfigureAwait(false);
        }

        await PlaybackState.DisposeAsync().ConfigureAwait(false);
        await Title.DisposeAsync().ConfigureAwait(false);
        await Artist.DisposeAsync().ConfigureAwait(false);
        await Play.DisposeAsync().ConfigureAwait(false);
        await Pause.DisposeAsync().ConfigureAwait(false);
        await Stop.DisposeAsync().ConfigureAwait(false);
        await Next.DisposeAsync().ConfigureAwait(false);
        await Previous.DisposeAsync().ConfigureAwait(false);
    }
}
