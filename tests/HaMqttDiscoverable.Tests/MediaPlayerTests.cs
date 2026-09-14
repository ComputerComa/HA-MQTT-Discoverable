using System.Text.Json;
using HaMqttDiscoverable;
using HaMqttDiscoverable.Entities;
using Xunit;

namespace HaMqttDiscoverable.Tests;

[Collection(TestBrokerCollection.Name)]
public sealed class MediaPlayerTests : IAsyncLifetime
{
    private readonly TestBroker _broker;
    private HaMqttConnection _connection = null!;
    private MqttMessageCapture _capture = null!;

    public MediaPlayerTests(TestBroker broker)
    {
        _broker = broker;
    }

    public async Task InitializeAsync()
    {
        _connection = new HaMqttConnection(new MqttSettings
        {
            Host = "127.0.0.1",
            Port = _broker.Port,
            ClientId = $"test-mp-{Guid.NewGuid():N}",
            PublishClientAvailability = false,
        });
        await _connection.ConnectAsync();
        _capture = await MqttMessageCapture.ConnectAsync(_broker.Port);
    }

    public async Task DisposeAsync()
    {
        await _capture.DisposeAsync();
        await _connection.DisposeAsync();
    }

    private static Device TestDevice() => Device.Create("living-room-tv", "Living Room TV", "Acme", "TV-9000");

    [Fact]
    public void WithoutSourcesOrMute_DoesNotCreateThoseSubEntities()
    {
        var player = new MediaPlayer(_connection, new MediaPlayerConfig
        {
            Name = "Living Room TV",
            UniqueId = "living-room-tv",
            Device = TestDevice(),
        });

        Assert.Null(player.Source);
        Assert.Null(player.Mute);
        Assert.NotNull(player.Power);
        Assert.NotNull(player.Volume);
    }

    [Fact]
    public async Task PublishDiscoveryAsync_PublishesEverySubEntity()
    {
        var player = new MediaPlayer(_connection, new MediaPlayerConfig
        {
            Name = "Living Room TV",
            UniqueId = "living-room-tv-2",
            Device = TestDevice(),
            Sources = new List<string> { "HDMI 1", "HDMI 2" },
            SupportsMute = true,
        });

        await player.PublishDiscoveryAsync();

        // Power, Volume, Mute, Source, State, Title, Artist, Play, Pause, Stop, Next, Previous.
        var topics = new[]
        {
            player.Power.DiscoveryTopic,
            player.Volume.DiscoveryTopic,
            player.Mute!.DiscoveryTopic,
            player.Source!.DiscoveryTopic,
            player.PlaybackState.DiscoveryTopic,
            player.Title.DiscoveryTopic,
            player.Artist.DiscoveryTopic,
            player.Play.DiscoveryTopic,
            player.Pause.DiscoveryTopic,
            player.Stop.DiscoveryTopic,
            player.Next.DiscoveryTopic,
            player.Previous.DiscoveryTopic,
        };

        Assert.Equal(topics.Length, topics.Distinct().Count());

        foreach (var topic in topics)
        {
            var retained = await _broker.Server.GetRetainedMessageAsync(topic);
            Assert.True(retained is not null, $"Expected a retained discovery payload on {topic}");
        }

        var sourcePayload = (await _capture.WaitForAsync(player.Source.DiscoveryTopic)).Payload;
        using var doc = JsonDocument.Parse(sourcePayload);
        var options = doc.RootElement.GetProperty("options").EnumerateArray().Select(e => e.GetString()).ToList();
        Assert.Equal(new[] { "HDMI 1", "HDMI 2" }, options);
    }

    [Fact]
    public async Task Volume_DefaultsToZeroToOneScale()
    {
        var player = new MediaPlayer(_connection, new MediaPlayerConfig
        {
            Name = "Living Room TV",
            UniqueId = "living-room-tv-3",
            Device = TestDevice(),
        });

        await player.Volume.PublishDiscoveryAsync();
        var (payload, _) = await _capture.WaitForAsync(player.Volume.DiscoveryTopic);

        using var doc = JsonDocument.Parse(payload);
        Assert.Equal(0, doc.RootElement.GetProperty("min").GetDouble());
        Assert.Equal(1, doc.RootElement.GetProperty("max").GetDouble());
    }

    [Fact]
    public async Task PowerCommandReceived_FiresWhenHomeAssistantSendsACommand()
    {
        bool? received = null;
        var tcs = new TaskCompletionSource();

        var player = new MediaPlayer(_connection, new MediaPlayerConfig
        {
            Name = "Living Room TV",
            UniqueId = "living-room-tv-4",
            Device = TestDevice(),
        });
        player.PowerCommandReceived += isOn =>
        {
            received = isOn;
            tcs.TrySetResult();
            return Task.CompletedTask;
        };

        await player.PublishDiscoveryAsync();
        await _capture.WaitForAsync(player.Power.DiscoveryTopic);

        await _connection.PublishAsync(player.Power.CommandTopic!, "ON");

        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(received);
    }

    [Fact]
    public async Task PlayRequested_FiresWhenHomeAssistantPressesPlay()
    {
        var tcs = new TaskCompletionSource();

        var player = new MediaPlayer(_connection, new MediaPlayerConfig
        {
            Name = "Living Room TV",
            UniqueId = "living-room-tv-5",
            Device = TestDevice(),
        });
        player.PlayRequested += () =>
        {
            tcs.TrySetResult();
            return Task.CompletedTask;
        };

        await player.PublishDiscoveryAsync();
        await _capture.WaitForAsync(player.Play.DiscoveryTopic);

        await _connection.PublishAsync(player.Play.CommandTopic!, player.Play.Config.PayloadPress);

        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task PublishStateAsync_OnlyPublishesNonNullFields()
    {
        var player = new MediaPlayer(_connection, new MediaPlayerConfig
        {
            Name = "Living Room TV",
            UniqueId = "living-room-tv-6",
            Device = TestDevice(),
            Sources = new List<string> { "HDMI 1" },
        });

        await player.PublishStateAsync(new MediaPlayerSnapshot
        {
            IsOn = true,
            PlaybackState = MediaPlayerPlaybackState.Playing,
            Title = "Some Movie",
        });

        var (powerPayload, _) = await _capture.WaitForAsync(player.Power.StateTopic);
        Assert.Equal("ON", powerPayload);

        var (statePayload, _) = await _capture.WaitForAsync(player.PlaybackState.StateTopic);
        Assert.Equal(MediaPlayerPlaybackState.Playing, statePayload);

        var (titlePayload, _) = await _capture.WaitForAsync(player.Title.StateTopic);
        Assert.Equal("Some Movie", titlePayload);

        // Volume and Source weren't included in the snapshot, so nothing should have been retained for them.
        Assert.Null(await _broker.Server.GetRetainedMessageAsync(player.Volume.StateTopic));
        Assert.Null(await _broker.Server.GetRetainedMessageAsync(player.Source!.StateTopic));
    }
}
