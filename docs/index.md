---
_layout: landing
---

# HaMqttDiscoverable

A .NET library for creating [Home Assistant](https://www.home-assistant.io/) [MQTT-discoverable](https://www.home-assistant.io/integrations/mqtt/#mqtt-discovery) devices and entities from C# - sensors, switches, lights, numbers, buttons, media players and more - without hand-writing discovery JSON or MQTT topic strings.

```csharp
var connection = new HaMqttConnection(new MqttSettings { Host = "192.168.1.10" });
await connection.ConnectAsync();

var device = Device.Create("weather-station-1", "Weather Station", manufacturer: "Acme");

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

That's it - the entity shows up in Home Assistant automatically, grouped under a "Weather Station" device.

## Where to go next

- **[Getting started](articles/getting-started.md)** - install the package, connect, publish your first entity.
- **[Entities](articles/entities.md)** - the sensor, switch, light, and other entity types this library ships with.
- **[Media players](articles/media-players.md)** - building a composite media player and wiring it into Home Assistant.
- **[API Reference](api/HaMqttDiscoverable.yml)** - full reference for every public type, generated from the library's XML doc comments.

## Install

```bash
dotnet add package HaMqttDiscoverable
```

Source, issues, and the Apache-2.0 license live at [github.com/ComputerComa/HA-MQTT-Discoverable](https://github.com/ComputerComa/HA-MQTT-Discoverable).
