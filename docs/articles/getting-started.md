# Getting started

## Install

```bash
dotnet add package HaMqttDiscoverable
```

The package targets .NET 10 and depends on [MQTTnet](https://github.com/dotnet/MQTTnet) for the underlying MQTT client.

## 1. Connect to your broker

Every entity is published through a single <xref:HaMqttDiscoverable.HaMqttConnection>, configured with <xref:HaMqttDiscoverable.MqttSettings>. Create one connection per application (or per broker) and share it across every entity you create.

```csharp
using HaMqttDiscoverable;
using HaMqttDiscoverable.Entities;

var connection = new HaMqttConnection(new MqttSettings
{
    Host = "192.168.1.10",
    Username = "ha",
    Password = "secret",
});

await connection.ConnectAsync();
```

By default the connection also publishes a device-wide availability topic backed by an MQTT last-will message (see [Connections & availability](connections.md)), so Home Assistant marks every entity unavailable if your process crashes rather than showing stale state forever.

## 2. Describe your device

A <xref:HaMqttDiscoverable.Device> groups related entities together under one device in Home Assistant's device registry:

```csharp
var device = Device.Create(
    identifier: "weather-station-1",
    name: "Weather Station",
    manufacturer: "Acme",
    model: "WS-100");
```

## 3. Create and publish an entity

Each entity type has a **config** class describing its Home Assistant discovery fields (here, <xref:HaMqttDiscoverable.Entities.SensorConfig>) and an **entity** class (<xref:HaMqttDiscoverable.Entities.Sensor>) that computes MQTT topics and publishes discovery/state:

```csharp
var temperature = new Sensor(connection, new SensorConfig
{
    Name = "Temperature",
    UniqueId = "weather-station-1_temperature",
    Device = device,
    UnitOfMeasurement = "°C",
    DeviceClass = "temperature",
    StateClass = "measurement",
});

await temperature.PublishDiscoveryAsync();
await temperature.PublishStateAsync("21.5");
```

`PublishDiscoveryAsync()` publishes the retained discovery payload Home Assistant reads to create the entity - call it once after connecting. `PublishStateAsync(...)` publishes the current value; call it whenever the value changes.

## 4. Handle commands from Home Assistant

Entities that Home Assistant can control (switch, number, select, text, button, light, and the sub-entities of `MediaPlayer`) raise a C# event when a command arrives, in addition to accepting a callback in their constructor:

```csharp
var relay = new Switch(connection, new SwitchConfig
{
    Name = "Relay",
    UniqueId = "garage-relay",
    Device = device,
});

relay.CommandReceived += async isOn =>
{
    SetHardwareRelay(isOn);
    await relay.PublishStateAsync(isOn); // confirm the new state back to Home Assistant
};

await relay.PublishDiscoveryAsync(); // also subscribes to the command topic
```

See [Entities](entities.md) for the full list of entity types and what each one supports.

## 5. Clean up

Call `RemoveAsync()` on an entity to delete it from Home Assistant (publishes an empty retained discovery payload), and dispose the connection when your application shuts down:

```csharp
await temperature.RemoveAsync();
await connection.DisposeAsync();
```

## Full example

See [`samples/SimpleSensorDemo`](https://github.com/ComputerComa/HA-MQTT-Discoverable/tree/main/samples/SimpleSensorDemo) in the repository for a complete, runnable console app that publishes a sensor, a switch, and a button, and keeps the sensor's value updating on a timer.
