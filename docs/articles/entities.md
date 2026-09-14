# Entities

Every entity type follows the same shape: a `*Config` class holding its Home Assistant discovery fields (all deriving from <xref:HaMqttDiscoverable.Entities.EntityConfig>), and an entity class deriving from <xref:HaMqttDiscoverable.Entities.HaEntity`1> that computes MQTT topics and publishes discovery/state/commands. Full member lists are in the [API Reference](../api/index.md); this page is a task-oriented overview of what each one is for.

| Entity | Component | Read/write | Notes |
|---|---|---|---|
| <xref:HaMqttDiscoverable.Entities.Sensor> | `sensor` | Read-only | A reported value, e.g. a temperature reading. |
| <xref:HaMqttDiscoverable.Entities.BinarySensor> | `binary_sensor` | Read-only | A two-state value, e.g. a door sensor. |
| <xref:HaMqttDiscoverable.Entities.Switch> | `switch` | Read/write | A simple on/off control. |
| <xref:HaMqttDiscoverable.Entities.Button> | `button` | Write-only | A stateless, momentary action. |
| <xref:HaMqttDiscoverable.Entities.Number> | `number` | Read/write | A numeric value with min/max/step. |
| <xref:HaMqttDiscoverable.Entities.Text> | `text` | Read/write | A free-form string value. |
| <xref:HaMqttDiscoverable.Entities.Select> | `select` | Read/write | A value from a fixed list of options. |
| <xref:HaMqttDiscoverable.Entities.Light> | `light` | Read/write | On/off + optional brightness, using Home Assistant's JSON light schema. |
| <xref:HaMqttDiscoverable.Entities.MediaPlayer> | *(composite)* | Read/write | Not a single component - see [Media players](media-players.md). |

## Shared configuration

Every `*Config` class inherits these fields from <xref:HaMqttDiscoverable.Entities.EntityConfig>:

- `Name`, `UniqueId` (required), `ObjectId`, `Device` (required)
- `Icon`, `DeviceClass`, `EntityCategory`, `EnabledByDefault`, `Qos`
- `Availability` - overrides the connection-level availability topic for just this entity

See the [Home Assistant MQTT integration docs](https://www.home-assistant.io/integrations/mqtt/) for the full set of fields each component understands, and the API reference for each `*Config` class for what this library exposes.

## Read-only entities

`Sensor` and `BinarySensor` never accept commands - they just report state:

```csharp
var doorSensor = new BinarySensor(connection, new BinarySensorConfig
{
    Name = "Front Door",
    UniqueId = "front-door",
    Device = device,
    DeviceClass = "door",
});

await doorSensor.PublishDiscoveryAsync();
await doorSensor.PublishStateAsync(true); // maps to PayloadOn/PayloadOff ("ON"/"OFF" by default)
```

## Controllable entities

`Switch`, `Number`, `Text`, `Select`, `Button`, and `Light` all accept commands from Home Assistant. Each exposes a `CommandReceived` (or, for `Button`, `Pressed`) event, and accepts an equivalent callback in its constructor:

```csharp
var volume = new Number(connection, new NumberConfig
{
    Name = "Volume",
    UniqueId = "speaker-volume",
    Device = device,
    Min = 0,
    Max = 100,
    Step = 1,
});

volume.CommandReceived += async value =>
{
    SetHardwareVolume(value);
    await volume.PublishStateAsync(value);
};
```

`PublishDiscoveryAsync()` subscribes to the entity's command topic in addition to publishing its discovery payload, so commands only start arriving once you've called it.

## Light

`Light` uses Home Assistant's [MQTT JSON light schema](https://www.home-assistant.io/integrations/light.mqtt/#json-schema), so state and commands are small JSON objects (<xref:HaMqttDiscoverable.Entities.LightState>) rather than plain strings:

```csharp
var lamp = new Light(connection, new LightConfig
{
    Name = "Lamp",
    UniqueId = "living-room-lamp",
    Device = device,
    SupportsBrightness = true,
});

lamp.CommandReceived += async state =>
{
    ApplyToHardware(state.State == "ON", state.Brightness);
    await lamp.PublishStateAsync(state);
};
```

## Removing an entity

Call `RemoveAsync()` to delete an entity from Home Assistant (publishes an empty retained payload to its discovery topic, and unsubscribes its command topic if it has one).

## Adding your own entity type

More Home Assistant MQTT components (covers, climate, locks, ...) can be added by subclassing <xref:HaMqttDiscoverable.Entities.HaEntity`1> the same way the built-in types under [`src/HaMqttDiscoverable/Entities`](https://github.com/ComputerComa/HA-MQTT-Discoverable/tree/main/src/HaMqttDiscoverable/Entities) do:

1. A `*Config : EntityConfig` with the fields specific to that component, and an overridden `Component` property (e.g. `"cover"`).
2. An entity class deriving from `HaEntity<TConfig>`, overriding `SupportsCommands` (and `HasStateTopic`, if the component has no state topic) and `OnCommandReceivedAsync` to parse incoming commands, exposing whatever events make sense for that component.

`Switch` is the simplest example to copy from; `Light` shows how to work with a JSON-schema entity instead of a plain-string one.
