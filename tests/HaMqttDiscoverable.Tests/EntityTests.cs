using System.Text.Json;
using HaMqttDiscoverable;
using HaMqttDiscoverable.Entities;
using MQTTnet;
using Xunit;

namespace HaMqttDiscoverable.Tests;

[Collection(TestBrokerCollection.Name)]
public sealed class EntityTests : IAsyncLifetime
{
    private readonly TestBroker _broker;
    private HaMqttConnection _connection = null!;
    private MqttMessageCapture _capture = null!;

    public EntityTests(TestBroker broker)
    {
        _broker = broker;
    }

    public async Task InitializeAsync()
    {
        _connection = new HaMqttConnection(new MqttSettings
        {
            Host = "127.0.0.1",
            Port = _broker.Port,
            ClientId = $"test-{Guid.NewGuid():N}",
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

    private static Device TestDevice() => Device.Create("weather-station-1", "Weather Station", "Acme", "WS-100");

    [Fact]
    public async Task Sensor_PublishDiscovery_PublishesExpectedTopicsAndFields()
    {
        var sensor = new Sensor(_connection, new SensorConfig
        {
            Name = "Temperature",
            UniqueId = "weather-station-1_temperature",
            Device = TestDevice(),
            UnitOfMeasurement = "°C",
            DeviceClass = "temperature",
            StateClass = "measurement",
        });

        await sensor.PublishDiscoveryAsync();

        var (payload, _) = await _capture.WaitForAsync(sensor.DiscoveryTopic);

        // Confirm the discovery payload was actually retained by the broker (so Home Assistant
        // picks it up even if it wasn't running yet), not just delivered live.
        var retainedMessage = await _broker.Server.GetRetainedMessageAsync(sensor.DiscoveryTopic);
        Assert.NotNull(retainedMessage);

        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;
        Assert.Equal("Temperature", root.GetProperty("name").GetString());
        Assert.Equal("weather-station-1_temperature", root.GetProperty("unique_id").GetString());
        Assert.Equal("°C", root.GetProperty("unit_of_measurement").GetString());
        Assert.Equal("temperature", root.GetProperty("device_class").GetString());
        Assert.Equal("measurement", root.GetProperty("state_class").GetString());
        Assert.Equal(sensor.StateTopic, root.GetProperty("state_topic").GetString());
        Assert.False(root.TryGetProperty("command_topic", out _));
        Assert.Equal("weather-station-1", root.GetProperty("device").GetProperty("identifiers")[0].GetString());
    }

    [Fact]
    public async Task Sensor_PublishState_PublishesRawPayloadToStateTopic()
    {
        var sensor = new Sensor(_connection, new SensorConfig
        {
            Name = "Temperature",
            UniqueId = "weather-station-1_temperature",
            Device = TestDevice(),
        });

        await sensor.PublishStateAsync("21.5");

        var (payload, _) = await _capture.WaitForAsync(sensor.StateTopic);
        Assert.Equal("21.5", payload);

        var retainedMessage = await _broker.Server.GetRetainedMessageAsync(sensor.StateTopic);
        Assert.NotNull(retainedMessage);
        Assert.Equal("21.5", retainedMessage!.ConvertPayloadToString());
    }

    [Fact]
    public async Task Sensor_Remove_PublishesEmptyRetainedPayloadToDiscoveryTopic()
    {
        var sensor = new Sensor(_connection, new SensorConfig
        {
            Name = "Temperature",
            UniqueId = "weather-station-1_temperature",
            Device = TestDevice(),
        });

        // Prime the broker with a real (retained) discovery payload first, so removal has something to clear.
        await sensor.PublishDiscoveryAsync();
        await _capture.WaitForAsync(sensor.DiscoveryTopic);
        Assert.NotNull(await _broker.Server.GetRetainedMessageAsync(sensor.DiscoveryTopic));

        await sensor.RemoveAsync();

        var (payload, _) = await _capture.WaitForAsync(sensor.DiscoveryTopic);
        Assert.Equal(string.Empty, payload);

        // An empty retained payload tells the broker to forget the retained message entirely.
        Assert.Null(await _broker.Server.GetRetainedMessageAsync(sensor.DiscoveryTopic));
    }

    [Fact]
    public async Task BinarySensor_PublishState_MapsBooleanToConfiguredPayloads()
    {
        var doorSensor = new BinarySensor(_connection, new BinarySensorConfig
        {
            Name = "Front Door",
            UniqueId = "weather-station-1_front_door",
            Device = TestDevice(),
            DeviceClass = "door",
        });

        await doorSensor.PublishStateAsync(true);
        var (onPayload, _) = await _capture.WaitForAsync(doorSensor.StateTopic);
        Assert.Equal("ON", onPayload);

        await doorSensor.PublishStateAsync(false);
        var (offPayload, _) = await _capture.WaitForAsync(doorSensor.StateTopic);
        Assert.Equal("OFF", offPayload);
    }

    [Fact]
    public async Task Switch_CommandFromHomeAssistant_RaisesCommandReceivedWithParsedBoolean()
    {
        bool? received = null;
        var tcs = new TaskCompletionSource();

        var relay = new Switch(_connection, new SwitchConfig
        {
            Name = "Relay",
            UniqueId = "weather-station-1_relay",
            Device = TestDevice(),
        }, onCommand: isOn =>
        {
            received = isOn;
            tcs.TrySetResult();
            return Task.CompletedTask;
        });

        await relay.PublishDiscoveryAsync();
        await _capture.WaitForAsync(relay.DiscoveryTopic);

        // Simulate Home Assistant sending a command.
        await _connection.PublishAsync(relay.CommandTopic!, "ON");

        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(received);
    }

    [Fact]
    public async Task Button_CommandFromHomeAssistant_RaisesPressed()
    {
        var tcs = new TaskCompletionSource();

        var restart = new Button(_connection, new ButtonConfig
        {
            Name = "Restart",
            UniqueId = "weather-station-1_restart",
            Device = TestDevice(),
        }, onPressed: () =>
        {
            tcs.TrySetResult();
            return Task.CompletedTask;
        });

        await restart.PublishDiscoveryAsync();
        await _capture.WaitForAsync(restart.DiscoveryTopic);

        await _connection.PublishAsync(restart.CommandTopic!, restart.Config.PayloadPress);

        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Button_DiscoveryPayload_HasNoStateTopic()
    {
        var restart = new Button(_connection, new ButtonConfig
        {
            Name = "Restart",
            UniqueId = "weather-station-1_restart_2",
            Device = TestDevice(),
        });

        await restart.PublishDiscoveryAsync();
        var (payload, _) = await _capture.WaitForAsync(restart.DiscoveryTopic);

        using var doc = JsonDocument.Parse(payload);
        Assert.False(doc.RootElement.TryGetProperty("state_topic", out _));
        Assert.True(doc.RootElement.TryGetProperty("command_topic", out _));
    }

    [Fact]
    public async Task Entity_WithAvailabilityOverride_UsesItInsteadOfConnectionDefault()
    {
        var sensor = new Sensor(_connection, new SensorConfig
        {
            Name = "Battery",
            UniqueId = "weather-station-1_battery",
            Device = TestDevice(),
            Availability = new Availability("custom/availability/topic"),
        });

        await sensor.PublishDiscoveryAsync();
        var (payload, _) = await _capture.WaitForAsync(sensor.DiscoveryTopic);

        using var doc = JsonDocument.Parse(payload);
        Assert.Equal("custom/availability/topic", doc.RootElement.GetProperty("availability_topic").GetString());
    }

    [Fact]
    public async Task Light_CommandFromHomeAssistant_RaisesCommandReceivedWithParsedState()
    {
        LightState? received = null;
        var tcs = new TaskCompletionSource();

        var lamp = new Light(_connection, new LightConfig
        {
            Name = "Lamp",
            UniqueId = "weather-station-1_lamp",
            Device = TestDevice(),
            SupportsBrightness = true,
        }, onCommand: state =>
        {
            received = state;
            tcs.TrySetResult();
            return Task.CompletedTask;
        });

        await lamp.PublishDiscoveryAsync();
        await _capture.WaitForAsync(lamp.DiscoveryTopic);

        await _connection.PublishAsync(lamp.CommandTopic!, """{"state":"ON","brightness":128}""");

        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.NotNull(received);
        Assert.Equal("ON", received!.State);
        Assert.Equal(128, received.Brightness);
    }
}
