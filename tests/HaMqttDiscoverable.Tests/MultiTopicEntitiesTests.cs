using System.Text.Json;
using HaMqttDiscoverable;
using HaMqttDiscoverable.Entities;
using Xunit;

namespace HaMqttDiscoverable.Tests;

[Collection(TestBrokerCollection.Name)]
public sealed class MultiTopicEntitiesTests : IAsyncLifetime
{
    private readonly TestBroker _broker;
    private HaMqttConnection _connection = null!;
    private MqttMessageCapture _capture = null!;

    public MultiTopicEntitiesTests(TestBroker broker)
    {
        _broker = broker;
    }

    public async Task InitializeAsync()
    {
        _connection = new HaMqttConnection(new MqttSettings
        {
            Host = "127.0.0.1",
            Port = _broker.Port,
            ClientId = $"test-multi-{Guid.NewGuid():N}",
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

    private static Device TestDevice() => Device.Create("house-2", "House 2", "Acme", "H-2");

    [Fact]
    public async Task Cover_BasicOpenCloseStop()
    {
        CoverCommand? received = null;
        var tcs = new TaskCompletionSource();

        var cover = new Cover(_connection, new CoverConfig
        {
            Name = "Garage Door",
            UniqueId = "house-2_garage",
            Device = TestDevice(),
        });
        cover.CommandReceived += c => { received = c; tcs.TrySetResult(); return Task.CompletedTask; };

        await cover.PublishDiscoveryAsync();
        await _capture.WaitForAsync(cover.DiscoveryTopic);
        Assert.Null(cover.SetPositionTopic);

        await _connection.PublishAsync(cover.CommandTopic!, cover.Config.PayloadOpen);
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(CoverCommand.Open, received);

        await cover.PublishStateAsync(CoverState.Open);
        var (payload, _) = await _capture.WaitForAsync(cover.StateTopic);
        Assert.Equal("open", payload);
    }

    [Fact]
    public async Task Cover_PositionAndTilt()
    {
        int? positionReceived = null;
        int? tiltReceived = null;
        var positionTcs = new TaskCompletionSource();
        var tiltTcs = new TaskCompletionSource();

        var cover = new Cover(_connection, new CoverConfig
        {
            Name = "Blind",
            UniqueId = "house-2_blind",
            Device = TestDevice(),
            SupportsPosition = true,
            SupportsTilt = true,
        });
        cover.PositionCommandReceived += p => { positionReceived = p; positionTcs.TrySetResult(); return Task.CompletedTask; };
        cover.TiltCommandReceived += t => { tiltReceived = t; tiltTcs.TrySetResult(); return Task.CompletedTask; };

        await cover.PublishDiscoveryAsync();
        var (discoveryPayload, _) = await _capture.WaitForAsync(cover.DiscoveryTopic);
        using (var doc = JsonDocument.Parse(discoveryPayload))
        {
            Assert.True(doc.RootElement.TryGetProperty("set_position_topic", out _));
            Assert.True(doc.RootElement.TryGetProperty("tilt_command_topic", out _));
            Assert.True(doc.RootElement.TryGetProperty("tilt_status_topic", out _));
        }

        await _connection.PublishAsync(cover.SetPositionTopic!, "42");
        await positionTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(42, positionReceived);

        await _connection.PublishAsync(cover.TiltCommandTopic!, "77");
        await tiltTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(77, tiltReceived);

        await cover.PublishPositionAsync(50, CoverState.Open);
        var (statePayload, _) = await _capture.WaitForAsync(cover.StateTopic);
        using var stateDoc = JsonDocument.Parse(statePayload);
        Assert.Equal(50, stateDoc.RootElement.GetProperty("position").GetInt32());
        Assert.Equal("open", stateDoc.RootElement.GetProperty("state").GetString());

        await cover.PublishTiltAsync(30);
        var (tiltPayload, _) = await _capture.WaitForAsync(cover.TiltStatusTopic!);
        Assert.Equal("30", tiltPayload);
    }

    [Fact]
    public async Task Fan_OnOffPercentageAndPreset()
    {
        bool? onReceived = null;
        int? percentageReceived = null;
        string? presetReceived = null;
        var onTcs = new TaskCompletionSource();
        var percentageTcs = new TaskCompletionSource();
        var presetTcs = new TaskCompletionSource();

        var fan = new Fan(_connection, new FanConfig
        {
            Name = "Ceiling Fan",
            UniqueId = "house-2_ceiling_fan",
            Device = TestDevice(),
            SupportsPercentage = true,
            PresetModes = new List<string> { "eco", "sleep" },
        });
        fan.CommandReceived += isOn => { onReceived = isOn; onTcs.TrySetResult(); return Task.CompletedTask; };
        fan.PercentageCommandReceived += p => { percentageReceived = p; percentageTcs.TrySetResult(); return Task.CompletedTask; };
        fan.PresetModeCommandReceived += m => { presetReceived = m; presetTcs.TrySetResult(); return Task.CompletedTask; };

        await fan.PublishDiscoveryAsync();
        await _capture.WaitForAsync(fan.DiscoveryTopic);

        await _connection.PublishAsync(fan.CommandTopic!, "ON");
        await onTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(onReceived);

        await _connection.PublishAsync(fan.PercentageCommandTopic!, "66");
        await percentageTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(66, percentageReceived);

        await _connection.PublishAsync(fan.PresetModeCommandTopic!, "eco");
        await presetTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal("eco", presetReceived);

        await fan.PublishPercentageAsync(66);
        var (payload, _) = await _capture.WaitForAsync(fan.PercentageStateTopic!);
        Assert.Equal("66", payload);
    }

    [Fact]
    public async Task Humidifier_OnOffHumidityAndMode()
    {
        double? humidityReceived = null;
        var humidityTcs = new TaskCompletionSource();

        var humidifier = new Humidifier(_connection, new HumidifierConfig
        {
            Name = "Humidifier",
            UniqueId = "house-2_humidifier",
            Device = TestDevice(),
            Modes = new List<string> { "auto", "sleep" },
        });
        humidifier.TargetHumidityCommandReceived += h => { humidityReceived = h; humidityTcs.TrySetResult(); return Task.CompletedTask; };

        await humidifier.PublishDiscoveryAsync();
        var (discoveryPayload, _) = await _capture.WaitForAsync(humidifier.DiscoveryTopic);
        using var doc = JsonDocument.Parse(discoveryPayload);
        Assert.True(doc.RootElement.TryGetProperty("target_humidity_command_topic", out _));
        Assert.True(doc.RootElement.TryGetProperty("mode_command_topic", out _));

        await _connection.PublishAsync(humidifier.TargetHumidityCommandTopic!, "55");
        await humidityTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(55, humidityReceived);

        await humidifier.PublishTargetHumidityAsync(55);
        var (payload, _) = await _capture.WaitForAsync(humidifier.TargetHumidityStateTopic);
        Assert.Equal("55", payload);
    }

    [Fact]
    public async Task Vacuum_CommandsAndJsonState()
    {
        VacuumCommand? received = null;
        var tcs = new TaskCompletionSource();

        var vacuum = new Vacuum(_connection, new VacuumConfig
        {
            Name = "Robot Vacuum",
            UniqueId = "house-2_vacuum",
            Device = TestDevice(),
            FanSpeedList = new List<string> { "low", "high" },
        });
        vacuum.CommandReceived += c => { received = c; tcs.TrySetResult(); return Task.CompletedTask; };

        await vacuum.PublishDiscoveryAsync();
        await _capture.WaitForAsync(vacuum.DiscoveryTopic);

        await _connection.PublishAsync(vacuum.CommandTopic!, vacuum.Config.PayloadStart);
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(VacuumCommand.Start, received);

        await vacuum.PublishStateAsync(VacuumActivity.Cleaning, fanSpeed: "high", extraAttributes: new Dictionary<string, object?> { ["battery_level"] = 80 });
        var (payload, _) = await _capture.WaitForAsync(vacuum.StateTopic);
        using var doc = JsonDocument.Parse(payload);
        Assert.Equal("cleaning", doc.RootElement.GetProperty("state").GetString());
        Assert.Equal("high", doc.RootElement.GetProperty("fan_speed").GetString());
        Assert.Equal(80, doc.RootElement.GetProperty("battery_level").GetInt32());
    }

    [Fact]
    public async Task Valve_NonPositionMode()
    {
        ValveCommand? received = null;
        var tcs = new TaskCompletionSource();

        var valve = new Valve(_connection, new ValveConfig
        {
            Name = "Sprinkler",
            UniqueId = "house-2_sprinkler",
            Device = TestDevice(),
        });
        valve.CommandReceived += c => { received = c; tcs.TrySetResult(); return Task.CompletedTask; };

        await valve.PublishDiscoveryAsync();
        await _capture.WaitForAsync(valve.DiscoveryTopic);

        await _connection.PublishAsync(valve.CommandTopic!, valve.Config.PayloadOpen!);
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(ValveCommand.Open, received);

        await valve.PublishStateAsync(ValveState.Open);
        var (payload, _) = await _capture.WaitForAsync(valve.StateTopic);
        Assert.Equal("open", payload);
    }

    [Fact]
    public async Task Valve_PositionMode()
    {
        int? received = null;
        var tcs = new TaskCompletionSource();

        var valve = new Valve(_connection, new ValveConfig
        {
            Name = "Water Valve",
            UniqueId = "house-2_water_valve",
            Device = TestDevice(),
            ReportsPosition = true,
        });
        valve.PositionCommandReceived += p => { received = p; tcs.TrySetResult(); return Task.CompletedTask; };

        await valve.PublishDiscoveryAsync();
        await _capture.WaitForAsync(valve.DiscoveryTopic);

        await _connection.PublishAsync(valve.CommandTopic!, "42");
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(42, received);

        await valve.PublishPositionAsync(42);
        var (payload, _) = await _capture.WaitForAsync(valve.StateTopic);
        Assert.Equal("42", payload);
    }

    [Fact]
    public async Task WaterHeater_ModeTemperatureAndPower()
    {
        string? modeReceived = null;
        double? tempReceived = null;
        bool? powerReceived = null;
        var modeTcs = new TaskCompletionSource();
        var tempTcs = new TaskCompletionSource();
        var powerTcs = new TaskCompletionSource();

        var heater = new WaterHeater(_connection, new WaterHeaterConfig
        {
            Name = "Water Heater",
            UniqueId = "house-2_water_heater",
            Device = TestDevice(),
            SupportsPower = true,
        });
        heater.ModeCommandReceived += m => { modeReceived = m; modeTcs.TrySetResult(); return Task.CompletedTask; };
        heater.TargetTemperatureCommandReceived += t => { tempReceived = t; tempTcs.TrySetResult(); return Task.CompletedTask; };
        heater.PowerCommandReceived += p => { powerReceived = p; powerTcs.TrySetResult(); return Task.CompletedTask; };

        await heater.PublishDiscoveryAsync();
        await _capture.WaitForAsync(heater.DiscoveryTopic);

        await _connection.PublishAsync(heater.ModeCommandTopic, "eco");
        await modeTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal("eco", modeReceived);

        await _connection.PublishAsync(heater.TargetTemperatureCommandTopic, "50.5");
        await tempTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(50.5, tempReceived);

        await _connection.PublishAsync(heater.PowerCommandTopic!, "ON");
        await powerTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(powerReceived);

        await heater.PublishCurrentTemperatureAsync(48.0);
        var (payload, _) = await _capture.WaitForAsync(heater.CurrentTemperatureTopic);
        Assert.Equal("48", payload);
    }

    [Fact]
    public async Task Climate_ModeTemperatureFanSwingPresetHumidityAndAction()
    {
        string? modeReceived = null;
        double? tempReceived = null;
        string? fanReceived = null;
        string? swingReceived = null;
        string? presetReceived = null;
        double? humidityReceived = null;
        var modeTcs = new TaskCompletionSource();
        var tempTcs = new TaskCompletionSource();
        var fanTcs = new TaskCompletionSource();
        var swingTcs = new TaskCompletionSource();
        var presetTcs = new TaskCompletionSource();
        var humidityTcs = new TaskCompletionSource();

        var climate = new Climate(_connection, new ClimateConfig
        {
            Name = "Thermostat",
            UniqueId = "house-2_thermostat",
            Device = TestDevice(),
            SupportsAction = true,
            SupportsFanMode = true,
            SupportsSwingMode = true,
            SupportsPresetMode = true,
            PresetModes = new List<string> { "eco", "away" },
            SupportsHumidity = true,
        });
        climate.ModeCommandReceived += m => { modeReceived = m; modeTcs.TrySetResult(); return Task.CompletedTask; };
        climate.TargetTemperatureCommandReceived += t => { tempReceived = t; tempTcs.TrySetResult(); return Task.CompletedTask; };
        climate.FanModeCommandReceived += m => { fanReceived = m; fanTcs.TrySetResult(); return Task.CompletedTask; };
        climate.SwingModeCommandReceived += m => { swingReceived = m; swingTcs.TrySetResult(); return Task.CompletedTask; };
        climate.PresetModeCommandReceived += m => { presetReceived = m; presetTcs.TrySetResult(); return Task.CompletedTask; };
        climate.TargetHumidityCommandReceived += h => { humidityReceived = h; humidityTcs.TrySetResult(); return Task.CompletedTask; };

        await climate.PublishDiscoveryAsync();
        var (discoveryPayload, _) = await _capture.WaitForAsync(climate.DiscoveryTopic);
        using (var doc = JsonDocument.Parse(discoveryPayload))
        {
            Assert.False(doc.RootElement.TryGetProperty("state_topic", out _));
            Assert.True(doc.RootElement.TryGetProperty("mode_command_topic", out _));
            Assert.True(doc.RootElement.TryGetProperty("action_topic", out _));
        }

        await _connection.PublishAsync(climate.ModeCommandTopic, "heat");
        await modeTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal("heat", modeReceived);

        await _connection.PublishAsync(climate.TargetTemperatureCommandTopic!, "21.5");
        await tempTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(21.5, tempReceived);

        await _connection.PublishAsync(climate.FanModeCommandTopic!, "high");
        await fanTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal("high", fanReceived);

        await _connection.PublishAsync(climate.SwingModeCommandTopic!, "on");
        await swingTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal("on", swingReceived);

        await _connection.PublishAsync(climate.PresetModeCommandTopic!, "eco");
        await presetTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal("eco", presetReceived);

        await _connection.PublishAsync(climate.TargetHumidityCommandTopic!, "40");
        await humidityTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(40, humidityReceived);

        await climate.PublishActionAsync(HvacAction.Heating);
        var (actionPayload, _) = await _capture.WaitForAsync(climate.ActionTopic!);
        Assert.Equal("heating", actionPayload);
    }

    [Fact]
    public async Task Climate_TemperatureRangeMode()
    {
        double? lowReceived = null;
        double? highReceived = null;
        var tcs = new TaskCompletionSource();

        var climate = new Climate(_connection, new ClimateConfig
        {
            Name = "Dual Zone Thermostat",
            UniqueId = "house-2_dual_thermostat",
            Device = TestDevice(),
            SupportsTemperatureRange = true,
        });
        climate.TargetTemperatureRangeCommandReceived += (low, high) =>
        {
            lowReceived = low;
            highReceived = high;
            tcs.TrySetResult();
            return Task.CompletedTask;
        };

        await climate.PublishDiscoveryAsync();
        await _capture.WaitForAsync(climate.DiscoveryTopic);
        Assert.Null(climate.TargetTemperatureCommandTopic);

        await _connection.PublishAsync(climate.TargetTemperatureLowCommandTopic!, "18");
        await _connection.PublishAsync(climate.TargetTemperatureHighCommandTopic!, "24");
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(18, lowReceived);
        Assert.Equal(24, highReceived);

        await climate.PublishTargetTemperatureRangeAsync(18, 24);
        var (lowPayload, _) = await _capture.WaitForAsync(climate.TargetTemperatureLowStateTopic!);
        var (highPayload, _) = await _capture.WaitForAsync(climate.TargetTemperatureHighStateTopic!);
        Assert.Equal("18", lowPayload);
        Assert.Equal("24", highPayload);
    }

    [Fact]
    public async Task LawnMower_StartDockPauseAndActivity()
    {
        var startTcs = new TaskCompletionSource();
        var dockTcs = new TaskCompletionSource();
        var pauseTcs = new TaskCompletionSource();

        var mower = new LawnMower(_connection, new LawnMowerConfig
        {
            Name = "Mower",
            UniqueId = "house-2_mower",
            Device = TestDevice(),
        });
        mower.StartMowingRequested += () => { startTcs.TrySetResult(); return Task.CompletedTask; };
        mower.DockRequested += () => { dockTcs.TrySetResult(); return Task.CompletedTask; };
        mower.PauseRequested += () => { pauseTcs.TrySetResult(); return Task.CompletedTask; };

        await mower.PublishDiscoveryAsync();
        await _capture.WaitForAsync(mower.DiscoveryTopic);

        await _connection.PublishAsync(mower.StartMowingCommandTopic, "start_mowing");
        await startTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await _connection.PublishAsync(mower.DockCommandTopic, "dock");
        await dockTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await _connection.PublishAsync(mower.PauseCommandTopic, "pause");
        await pauseTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await mower.PublishStateAsync(LawnMowerActivity.Mowing);
        var (payload, _) = await _capture.WaitForAsync(mower.ActivityStateTopic);
        Assert.Equal("mowing", payload);
    }
}
