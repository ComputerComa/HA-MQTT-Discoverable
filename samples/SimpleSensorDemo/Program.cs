using HaMqttDiscoverable;
using HaMqttDiscoverable.Entities;

// A minimal end-to-end demo: connect to an MQTT broker, announce a device with a sensor,
// a switch, and a button, then keep the sensor's value updating every few seconds.
//
// Point it at your broker with environment variables, e.g.:
//   MQTT_HOST=192.168.1.10 MQTT_USERNAME=ha MQTT_PASSWORD=secret dotnet run

var connection = new HaMqttConnection(new MqttSettings
{
    Host = Environment.GetEnvironmentVariable("MQTT_HOST") ?? "localhost",
    Username = Environment.GetEnvironmentVariable("MQTT_USERNAME"),
    Password = Environment.GetEnvironmentVariable("MQTT_PASSWORD"),
});

await connection.ConnectAsync();
Console.WriteLine("Connected to MQTT broker.");

var device = Device.Create(
    identifier: "demo-weather-station",
    name: "Demo Weather Station",
    manufacturer: "HaMqttDiscoverable",
    model: "Sample");

var temperature = new Sensor(connection, new SensorConfig
{
    Name = "Temperature",
    UniqueId = "demo-weather-station_temperature",
    Device = device,
    UnitOfMeasurement = "°C",
    DeviceClass = "temperature",
    StateClass = "measurement",
});

var relay = new Switch(connection, new SwitchConfig
{
    Name = "Heater",
    UniqueId = "demo-weather-station_heater",
    Device = device,
});
relay.CommandReceived += async isOn =>
{
    Console.WriteLine($"Heater command received: {(isOn ? "ON" : "OFF")}");
    await relay.PublishStateAsync(isOn);
};

var restart = new Button(connection, new ButtonConfig
{
    Name = "Restart",
    UniqueId = "demo-weather-station_restart",
    Device = device,
});
restart.Pressed += () =>
{
    Console.WriteLine("Restart button pressed!");
    return Task.CompletedTask;
};

await temperature.PublishDiscoveryAsync();
await relay.PublishDiscoveryAsync();
await restart.PublishDiscoveryAsync();
await relay.PublishStateAsync(false);

Console.WriteLine("Discovery published. Press Ctrl+C to stop.");

var random = new Random();
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

try
{
    while (!cts.IsCancellationRequested)
    {
        var reading = 18 + random.NextDouble() * 6;
        await temperature.PublishStateAsync(reading.ToString("F1"));
        Console.WriteLine($"Published temperature: {reading:F1}°C");
        await Task.Delay(TimeSpan.FromSeconds(5), cts.Token);
    }
}
catch (OperationCanceledException)
{
    // graceful shutdown
}

await connection.DisposeAsync();
