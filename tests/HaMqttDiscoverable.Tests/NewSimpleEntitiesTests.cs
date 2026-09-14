using System.Text.Json;
using HaMqttDiscoverable;
using HaMqttDiscoverable.Entities;
using Xunit;

namespace HaMqttDiscoverable.Tests;

[Collection(TestBrokerCollection.Name)]
public sealed class NewSimpleEntitiesTests : IAsyncLifetime
{
    private readonly TestBroker _broker;
    private HaMqttConnection _connection = null!;
    private MqttMessageCapture _capture = null!;

    public NewSimpleEntitiesTests(TestBroker broker)
    {
        _broker = broker;
    }

    public async Task InitializeAsync()
    {
        _connection = new HaMqttConnection(new MqttSettings
        {
            Host = "127.0.0.1",
            Port = _broker.Port,
            ClientId = $"test-new-{Guid.NewGuid():N}",
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

    private static Device TestDevice() => Device.Create("house-1", "House", "Acme", "H-1");

    [Fact]
    public async Task AlarmControlPanel_CommandRoundTrip()
    {
        AlarmControlPanelCommand? received = null;
        var tcs = new TaskCompletionSource();

        var alarm = new AlarmControlPanel(_connection, new AlarmControlPanelConfig
        {
            Name = "Alarm",
            UniqueId = "house-1_alarm",
            Device = TestDevice(),
        }, onCommand: c => { received = c; tcs.TrySetResult(); return Task.CompletedTask; });

        await alarm.PublishDiscoveryAsync();
        await _capture.WaitForAsync(alarm.DiscoveryTopic);

        await _connection.PublishAsync(alarm.CommandTopic!, alarm.Config.PayloadArmAway);
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(AlarmControlPanelCommand.ArmAway, received);

        await alarm.PublishStateAsync(AlarmControlPanelState.ArmedAway);
        var (payload, _) = await _capture.WaitForAsync(alarm.StateTopic);
        Assert.Equal("armed_away", payload);
    }

    [Fact]
    public async Task Lock_CommandRoundTripAndState()
    {
        LockCommand? received = null;
        var tcs = new TaskCompletionSource();

        var doorLock = new LockEntity(_connection, new LockConfig
        {
            Name = "Front Door Lock",
            UniqueId = "house-1_front_lock",
            Device = TestDevice(),
        }, onCommand: c => { received = c; tcs.TrySetResult(); return Task.CompletedTask; });

        await doorLock.PublishDiscoveryAsync();
        await _capture.WaitForAsync(doorLock.DiscoveryTopic);

        await _connection.PublishAsync(doorLock.CommandTopic!, doorLock.Config.PayloadUnlock);
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(LockCommand.Unlock, received);

        await doorLock.PublishStateAsync(LockState.Unlocked);
        var (payload, _) = await _capture.WaitForAsync(doorLock.StateTopic);
        Assert.Equal("UNLOCKED", payload);
    }

    [Fact]
    public async Task Siren_CommandRoundTripAndState()
    {
        bool? received = null;
        var tcs = new TaskCompletionSource();

        var siren = new Siren(_connection, new SirenConfig
        {
            Name = "Siren",
            UniqueId = "house-1_siren",
            Device = TestDevice(),
        }, onCommand: isOn => { received = isOn; tcs.TrySetResult(); return Task.CompletedTask; });

        await siren.PublishDiscoveryAsync();
        await _capture.WaitForAsync(siren.DiscoveryTopic);

        await _connection.PublishAsync(siren.CommandTopic!, "ON");
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(received);

        await siren.PublishStateAsync(true);
        var (payload, _) = await _capture.WaitForAsync(siren.StateTopic);
        Assert.Equal("ON", payload);
    }

    [Fact]
    public async Task Scene_ActivatedRaisedOnCommand()
    {
        var tcs = new TaskCompletionSource();
        var scene = new Scene(_connection, new SceneConfig
        {
            Name = "Movie Night",
            UniqueId = "house-1_movie_night",
            Device = TestDevice(),
        }, onActivated: () => { tcs.TrySetResult(); return Task.CompletedTask; });

        await scene.PublishDiscoveryAsync();
        await _capture.WaitForAsync(scene.DiscoveryTopic);

        await _connection.PublishAsync(scene.CommandTopic!, scene.Config.PayloadOn);
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Notify_MessageReceived()
    {
        string? received = null;
        var tcs = new TaskCompletionSource();
        var notify = new Notify(_connection, new NotifyConfig
        {
            Name = "Kitchen Display",
            UniqueId = "house-1_kitchen_display",
            Device = TestDevice(),
        }, onMessage: m => { received = m; tcs.TrySetResult(); return Task.CompletedTask; });

        await notify.PublishDiscoveryAsync();
        await _capture.WaitForAsync(notify.DiscoveryTopic);

        await _connection.PublishAsync(notify.CommandTopic!, "Dinner's ready!");
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal("Dinner's ready!", received);
    }

    [Fact]
    public async Task Date_CommandRoundTripAndState()
    {
        DateOnly? received = null;
        var tcs = new TaskCompletionSource();
        var date = new Date(_connection, new DateConfig
        {
            Name = "Birthday",
            UniqueId = "house-1_birthday",
            Device = TestDevice(),
        }, onCommand: d => { received = d; tcs.TrySetResult(); return Task.CompletedTask; });

        await date.PublishDiscoveryAsync();
        await _capture.WaitForAsync(date.DiscoveryTopic);

        await _connection.PublishAsync(date.CommandTopic!, "2026-03-14");
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(new DateOnly(2026, 3, 14), received);

        await date.PublishStateAsync(new DateOnly(2026, 3, 14));
        var (payload, _) = await _capture.WaitForAsync(date.StateTopic);
        Assert.Equal("2026-03-14", payload);
    }

    [Fact]
    public async Task Time_CommandRoundTripAndState()
    {
        TimeOnly? received = null;
        var tcs = new TaskCompletionSource();
        var time = new Time(_connection, new TimeConfig
        {
            Name = "Alarm Time",
            UniqueId = "house-1_alarm_time",
            Device = TestDevice(),
        }, onCommand: t => { received = t; tcs.TrySetResult(); return Task.CompletedTask; });

        await time.PublishDiscoveryAsync();
        await _capture.WaitForAsync(time.DiscoveryTopic);

        await _connection.PublishAsync(time.CommandTopic!, "07:30:00");
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(new TimeOnly(7, 30, 0), received);
    }

    [Fact]
    public async Task DateTimeEntity_CommandRoundTripAndState()
    {
        DateTimeOffset? received = null;
        var tcs = new TaskCompletionSource();
        var dt = new DateTimeEntity(_connection, new DateTimeEntityConfig
        {
            Name = "Next Service",
            UniqueId = "house-1_next_service",
            Device = TestDevice(),
        }, onCommand: v => { received = v; tcs.TrySetResult(); return Task.CompletedTask; });

        await dt.PublishDiscoveryAsync();
        await _capture.WaitForAsync(dt.DiscoveryTopic);

        await _connection.PublishAsync(dt.CommandTopic!, "2026-03-14T07:30:00");
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.NotNull(received);
        Assert.Equal(2026, received!.Value.Year);
        Assert.Equal(7, received.Value.Hour);
    }

    [Fact]
    public async Task DeviceTracker_PublishesHomeAndAway()
    {
        var tracker = new DeviceTracker(_connection, new DeviceTrackerConfig
        {
            Name = "Phone",
            UniqueId = "house-1_phone",
            Device = TestDevice(),
        });

        await tracker.PublishStateAsync(true);
        var (home, _) = await _capture.WaitForAsync(tracker.StateTopic);
        Assert.Equal("home", home);

        await tracker.PublishStateAsync(false);
        var (away, _) = await _capture.WaitForAsync(tracker.StateTopic);
        Assert.Equal("not_home", away);
    }

    [Fact]
    public async Task Event_PublishesEventTypeAndExtraAttributes()
    {
        var doorbell = new Event(_connection, new EventConfig
        {
            Name = "Doorbell",
            UniqueId = "house-1_doorbell",
            Device = TestDevice(),
            EventTypes = new List<string> { "single_press", "double_press" },
        });

        await doorbell.PublishEventAsync("single_press", new Dictionary<string, object?> { ["source"] = "front" });

        var (payload, _) = await _capture.WaitForAsync(doorbell.StateTopic);
        using var doc = JsonDocument.Parse(payload);
        Assert.Equal("single_press", doc.RootElement.GetProperty("event_type").GetString());
        Assert.Equal("front", doc.RootElement.GetProperty("source").GetString());

        await Assert.ThrowsAsync<ArgumentException>(() => doorbell.PublishEventAsync("triple_press"));
    }

    [Fact]
    public async Task Update_InstallRequestedAndState()
    {
        var tcs = new TaskCompletionSource();
        var update = new Update(_connection, new UpdateConfig
        {
            Name = "Firmware",
            UniqueId = "house-1_firmware",
            Device = TestDevice(),
        }, onInstallRequested: () => { tcs.TrySetResult(); return Task.CompletedTask; });

        await update.PublishDiscoveryAsync();
        await _capture.WaitForAsync(update.DiscoveryTopic);

        await update.PublishStateAsync(new UpdateState { InstalledVersion = "1.0", LatestVersion = "1.1" });
        var (payload, _) = await _capture.WaitForAsync(update.StateTopic);
        using var doc = JsonDocument.Parse(payload);
        Assert.Equal("1.1", doc.RootElement.GetProperty("latest_version").GetString());

        await _connection.PublishAsync(update.CommandTopic!, update.Config.PayloadInstall);
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Image_PublishesBase64EncodedBytesAndDiscoveryUsesImageTopic()
    {
        var image = new Image(_connection, new ImageConfig
        {
            Name = "Snapshot",
            UniqueId = "house-1_snapshot",
            Device = TestDevice(),
        });

        await image.PublishDiscoveryAsync();
        var (discoveryPayload, _) = await _capture.WaitForAsync(image.DiscoveryTopic);
        using var doc = JsonDocument.Parse(discoveryPayload);
        Assert.Equal(image.StateTopic, doc.RootElement.GetProperty("image_topic").GetString());
        Assert.Equal("b64", doc.RootElement.GetProperty("image_encoding").GetString());
        Assert.False(doc.RootElement.TryGetProperty("state_topic", out _));

        var bytes = new byte[] { 1, 2, 3, 4 };
        await image.PublishImageAsync(bytes);
        var (payload, _) = await _capture.WaitForAsync(image.StateTopic);
        Assert.Equal(Convert.ToBase64String(bytes), payload);
    }

    [Fact]
    public async Task Camera_PublishesBase64EncodedFrameAndDiscoveryUsesTopic()
    {
        var camera = new Camera(_connection, new CameraConfig
        {
            Name = "Front Camera",
            UniqueId = "house-1_front_camera",
            Device = TestDevice(),
        });

        await camera.PublishDiscoveryAsync();
        var (discoveryPayload, _) = await _capture.WaitForAsync(camera.DiscoveryTopic);
        using var doc = JsonDocument.Parse(discoveryPayload);
        Assert.Equal(camera.StateTopic, doc.RootElement.GetProperty("topic").GetString());

        var bytes = new byte[] { 5, 6, 7 };
        await camera.PublishFrameAsync(bytes);
        var (payload, _) = await _capture.WaitForAsync(camera.StateTopic);
        Assert.Equal(Convert.ToBase64String(bytes), payload);
    }
}
