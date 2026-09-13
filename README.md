# HaMqttDiscoverable

A .NET library for creating [Home Assistant](https://www.home-assistant.io/) [MQTT-discoverable](https://www.home-assistant.io/integrations/mqtt/#mqtt-discovery) devices and entities from C# - sensors, switches, lights, numbers, buttons and more - without hand-writing discovery JSON or MQTT topic strings.

Inspired by the Python [`ha-mqtt-discoverable`](https://github.com/unixorn/ha-mqtt-discoverable) library.

## Install

```bash
dotnet add package HaMqttDiscoverable
```

## Quick start

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

var device = Device.Create("weather-station-1", "Weather Station", manufacturer: "Acme", model: "WS-100");

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

See [`samples/SimpleSensorDemo`](samples/SimpleSensorDemo) for a complete runnable example that publishes a sensor, a switch, and a button.

## Concepts

- **`HaMqttConnection`** wraps a single MQTT connection ([MQTTnet](https://github.com/dotnet/MQTTnet) under the hood). Create one per application and share it across every entity. It also manages a device-wide availability topic (via MQTT last-will), so Home Assistant marks everything unavailable if your process crashes.
- **`Device`** groups related entities together in Home Assistant's device registry.
- Each entity type has a **config** (e.g. `SensorConfig`) describing its Home Assistant discovery fields, and an **entity class** (e.g. `Sensor`) that computes MQTT topics, publishes discovery/state, and (for controllable entities) handles commands.

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

await relay.PublishDiscoveryAsync();
```

Call `PublishDiscoveryAsync()` once after connecting (it also subscribes to the entity's command topic, if it has one). Call `RemoveAsync()` to remove an entity from Home Assistant entirely.

## Supported entities

| Entity | Component | Notes |
|---|---|---|
| `Sensor` | `sensor` | Read-only value. |
| `BinarySensor` | `binary_sensor` | Read-only on/off. |
| `Switch` | `switch` | Controllable on/off. |
| `Button` | `button` | Stateless, momentary action. |
| `Number` | `number` | Controllable numeric value, with min/max/step. |
| `Text` | `text` | Controllable free-form string. |
| `Select` | `select` | Controllable value from a fixed list of options. |
| `Light` | `light` | Controllable on/off + optional brightness, using Home Assistant's JSON light schema. |

More entity types (covers, climate, locks, ...) can be added by subclassing `HaEntity<TConfig>` following the same pattern used for the built-in types under `src/HaMqttDiscoverable/Entities`.

## Configuration reference

Every entity config derives from `EntityConfig`, which covers the fields shared by all entities:

- `Name`, `UniqueId` (required), `ObjectId`, `Device` (required)
- `Icon`, `DeviceClass`, `EntityCategory`, `EnabledByDefault`, `Qos`
- `Availability` - overrides the shared connection-level availability topic for just this entity

Each concrete config (`SensorConfig`, `SwitchConfig`, ...) adds the fields specific to that Home Assistant component - see the XML doc comments on each class, or the [Home Assistant MQTT integration docs](https://www.home-assistant.io/integrations/mqtt/) for the full set of options per component.

## Building from source

```bash
dotnet build
dotnet test
```

Tests spin up a real, local, in-process MQTT broker ([MQTTnet.Server](https://github.com/dotnet/MQTTnet)) rather than mocking the MQTT client, so they exercise the actual wire protocol.

## License

Apache-2.0, see [LICENSE](LICENSE).
