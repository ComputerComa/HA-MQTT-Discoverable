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
| <xref:HaMqttDiscoverable.Entities.AlarmControlPanel> | `alarm_control_panel` | Read/write | Arm/disarm/trigger. |
| <xref:HaMqttDiscoverable.Entities.LockEntity> | `lock` | Read/write | Lock/unlock, optionally open. Named `LockEntity` to avoid colliding with `System.Threading.Lock`. |
| <xref:HaMqttDiscoverable.Entities.Siren> | `siren` | Read/write | On/off alarm sounder. |
| <xref:HaMqttDiscoverable.Entities.Scene> | `scene` | Write-only | A stateless trigger, like `Button` under Home Assistant's "scene" domain. |
| <xref:HaMqttDiscoverable.Entities.Notify> | `notify` | Write-only | Receives text messages Home Assistant sends it. |
| <xref:HaMqttDiscoverable.Entities.Date>, <xref:HaMqttDiscoverable.Entities.Time>, <xref:HaMqttDiscoverable.Entities.DateTimeEntity> | `date`, `time`, `datetime` | Read/write | Date/time values, formatted as ISO 8601. `DateTimeEntity` to avoid colliding with `System.DateTime`. |
| <xref:HaMqttDiscoverable.Entities.DeviceTracker> | `device_tracker` | Read-only | Home/away presence. |
| <xref:HaMqttDiscoverable.Entities.Event> | `event` | Read-only | Discrete named events, e.g. a doorbell's press types. |
| <xref:HaMqttDiscoverable.Entities.Update> | `update` | Read/write | Available software updates, with an optional Install action. |
| <xref:HaMqttDiscoverable.Entities.Image> | `image` | Write-only | A still image (published base64-encoded). |
| <xref:HaMqttDiscoverable.Entities.Camera> | `camera` | Write-only | A live-updating image feed (published base64-encoded). |
| <xref:HaMqttDiscoverable.Entities.Cover> | `cover` | Read/write | Open/close/stop, optionally with position and/or tilt. |
| <xref:HaMqttDiscoverable.Entities.Valve> | `valve` | Read/write | Open/close/stop, or an open-to-a-position valve. |
| <xref:HaMqttDiscoverable.Entities.Fan> | `fan` | Read/write | On/off, optionally with speed percentage and/or named presets. |
| <xref:HaMqttDiscoverable.Entities.Humidifier> | `humidifier` | Read/write | On/off with target humidity, optionally with named modes. |
| <xref:HaMqttDiscoverable.Entities.Vacuum> | `vacuum` | Read/write | Start/pause/stop/return/clean-spot/locate, optionally with fan speed. |
| <xref:HaMqttDiscoverable.Entities.LawnMower> | `lawn_mower` | Read/write | Start/dock/pause, with an activity report. |
| <xref:HaMqttDiscoverable.Entities.WaterHeater> | `water_heater` | Read/write | Operating mode plus target/current temperature. |
| <xref:HaMqttDiscoverable.Entities.Climate> | `climate` | Read/write | Thermostat: mode, temperature (or a range), and optional fan/swing/preset mode, humidity, and a separate power toggle. |

> [!NOTE]
> Home Assistant's MQTT integration also has an `infrared` platform (`emitter`/`receiver` schemas), but it's very new/still landing upstream as of this writing, so it isn't implemented here yet.

## Shared configuration

Every `*Config` class inherits these fields from <xref:HaMqttDiscoverable.Entities.EntityConfig>:

- `Name`, `UniqueId` (required), `ObjectId`, `Device` (required)
- `Icon`, `DeviceClass`, `EntityCategory`, `EnabledByDefault`, `Qos`
- `Availability` - overrides the connection-level availability topic for just this entity

See the [Home Assistant MQTT integration docs](https://www.home-assistant.io/integrations/mqtt/) for the full set of fields each component understands, and the API reference for each `*Config` class for what this library exposes.

## Read-only entities

`Sensor`, `BinarySensor`, `DeviceTracker`, `Event`, `Image`, and `Camera` never accept commands - they just report state (or, for `Image`/`Camera`, publish binary content):

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

Most other entities accept commands from Home Assistant. Each exposes a `CommandReceived` (or a more specific name, like `Pressed` on `Button` or `Activated` on `Scene`) event, and accepts an equivalent callback in its constructor:

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

`PublishDiscoveryAsync()` subscribes to the entity's command topic(s) in addition to publishing its discovery payload, so commands only start arriving once you've called it.

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

## Multi-topic entities

Home Assistant's MQTT schema for some components has more than one independently-optional command/state topic pair - a cover's position is separate from its open/close/stop commands; a climate device has a topic pair for each of mode, temperature, fan mode, swing mode, preset mode, and humidity. `Cover`, `Fan`, `Humidifier`, `Vacuum`, `WaterHeater`, and `Climate` model this with **config flags that turn optional features on**, each adding its own event and `PublishXAsync` method:

```csharp
var thermostat = new Climate(connection, new ClimateConfig
{
    Name = "Thermostat",
    UniqueId = "living-room-thermostat",
    Device = device,
    SupportsFanMode = true,
    SupportsPresetMode = true,
    PresetModes = new List<string> { "eco", "away" },
});

thermostat.ModeCommandReceived += async mode => { SetMode(mode); await thermostat.PublishModeAsync(mode); };
thermostat.TargetTemperatureCommandReceived += async temp => { SetTarget(temp); await thermostat.PublishTargetTemperatureAsync(temp); };
thermostat.FanModeCommandReceived += async mode => { SetFanMode(mode); await thermostat.PublishFanModeAsync(mode); };
thermostat.PresetModeCommandReceived += async preset => { SetPreset(preset); await thermostat.PublishPresetModeAsync(preset); };

await thermostat.PublishDiscoveryAsync();
await thermostat.PublishCurrentTemperatureAsync(21.0);
```

Leaving a flag off (e.g. not setting `SupportsFanMode`) simply omits that topic pair from discovery and leaves the corresponding event/method unused - see each type's XML doc comments (or the API reference) for its full set of flags.

## Removing an entity

Call `RemoveAsync()` to delete an entity from Home Assistant (publishes an empty retained payload to its discovery topic, and unsubscribes its command topic(s) if it has any).

## Adding your own entity type

More Home Assistant MQTT components (the `infrared` platform noted above, or a future one) can be added by subclassing <xref:HaMqttDiscoverable.Entities.HaEntity`1> the same way the built-in types under [`src/HaMqttDiscoverable/Entities`](https://github.com/ComputerComa/HA-MQTT-Discoverable/tree/main/src/HaMqttDiscoverable/Entities) do:

1. A `*Config : EntityConfig` with the fields specific to that component, and an overridden `Component` property (e.g. `"cover"`).
2. An entity class deriving from `HaEntity<TConfig>`, overriding `SupportsCommands` (and `HasStateTopic`, if the component has no state topic) and `OnCommandReceivedAsync` to parse incoming commands, exposing whatever events make sense for that component.
3. If the component needs more than one command topic, call the protected `RegisterAuxiliaryCommandTopic(topic, handler)` from the constructor for each extra one - `HaEntity` handles subscribing/unsubscribing them alongside the primary command topic.

`Switch` is the simplest example to copy from; `Light` shows how to work with a JSON-schema entity instead of a plain-string one; `Cover` and `Climate` show the auxiliary-topic pattern for entities with more than one command topic.
