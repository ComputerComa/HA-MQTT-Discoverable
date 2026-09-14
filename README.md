# HaMqttDiscoverable

A .NET library for creating [Home Assistant](https://www.home-assistant.io/) [MQTT-discoverable](https://www.home-assistant.io/integrations/mqtt/#mqtt-discovery) devices and entities from C# - sensors, switches, lights, numbers, buttons and more - without hand-writing discovery JSON or MQTT topic strings.

Inspired by the Python [`ha-mqtt-discoverable`](https://github.com/unixorn/ha-mqtt-discoverable) library.

📖 **[Full documentation](https://computercoma.github.io/HA-MQTT-Discoverable/)** - getting started guide, entity-by-entity walkthroughs, and a full API reference generated from this repo's XML doc comments.

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
| `MediaPlayer` | *(composite)* | Not a single component - see [Media players](#media-players) below. |
| `AlarmControlPanel` | `alarm_control_panel` | Arm/disarm/trigger. |
| `LockEntity` | `lock` | Lock/unlock, optionally open. Named `LockEntity` (not `Lock`) to avoid colliding with `System.Threading.Lock`. |
| `Siren` | `siren` | Controllable on/off alarm sounder. |
| `Scene` | `scene` | Stateless trigger (like `Button`, under Home Assistant's "scene" domain). |
| `Notify` | `notify` | Receives text messages Home Assistant sends it. |
| `Date` / `Time` / `DateTimeEntity` | `date` / `time` / `datetime` | Controllable date/time values (ISO 8601). `DateTimeEntity`, not `DateTime`, to avoid colliding with `System.DateTime`. |
| `DeviceTracker` | `device_tracker` | Read-only home/away presence. |
| `Event` | `event` | Read-only discrete named events (e.g. a doorbell's press types). |
| `Update` | `update` | Reports available software updates, with an optional Install action. |
| `Image` | `image` | Publishes a still image (base64-encoded). |
| `Camera` | `camera` | Publishes a live-updating image feed (base64-encoded). |
| `Cover` | `cover` | Open/close/stop, optionally with position and/or tilt. |
| `Valve` | `valve` | Open/close/stop, or an open-to-a-position valve. |
| `Fan` | `fan` | On/off, optionally with speed percentage and/or named presets. |
| `Humidifier` | `humidifier` | On/off with target humidity, optionally with named modes. |
| `Vacuum` | `vacuum` | Start/pause/stop/return/clean-spot/locate, optionally with fan speed. |
| `LawnMower` | `lawn_mower` | Start/dock/pause, with an activity report. |
| `WaterHeater` | `water_heater` | Operating mode plus target/current temperature. |
| `Climate` | `climate` | Thermostat: mode, temperature (or a range), and optional fan/swing/preset mode, humidity, and a separate power toggle. |

`Cover`, `Fan`, `Humidifier`, `Vacuum`, `WaterHeater`, and `Climate` have more moving parts than the rest - see [Entities](https://computercoma.github.io/HA-MQTT-Discoverable/articles/entities.html) and their XML doc comments for what each optional feature needs.

> [!NOTE]
> Home Assistant's MQTT integration also has an `infrared` platform (`emitter`/`receiver` schemas) that's very new/still landing upstream as of this writing - it isn't implemented here yet.

More entity types can be added by subclassing `HaEntity<TConfig>` following the same pattern used for the built-in types under `src/HaMqttDiscoverable/Entities`; `Switch` is the simplest starting point, `Cover`/`Climate` show the pattern for an entity with more than one command topic (via `RegisterAuxiliaryCommandTopic`).

## Media players

Home Assistant's MQTT integration has no native `media_player` platform - only the component types listed above are MQTT-discoverable. `MediaPlayer` works around that by publishing a bundle of real entities (a power switch, a volume number, optionally a mute switch and a source select, sensors for playback state/title/artist, and play/pause/stop/next/previous buttons) grouped under one device:

```csharp
var player = new MediaPlayer(connection, new MediaPlayerConfig
{
    Name = "Living Room TV",
    UniqueId = "living-room-tv",
    Device = device,
    Sources = new List<string> { "HDMI 1", "HDMI 2", "Chromecast" },
});

player.PowerCommandReceived += async isOn =>
{
    SetPower(isOn);
    await player.PublishPowerAsync(isOn);
};
player.PlayRequested += async () =>
{
    Play();
    await player.PublishPlaybackStateAsync(MediaPlayerPlaybackState.Playing);
};

await player.PublishDiscoveryAsync();
```

To present these as a single `media_player` entity in Home Assistant's UI, wire them together with Home Assistant's built-in **[Universal Media Player](https://www.home-assistant.io/integrations/universal/)** (`universal`) platform, in `configuration.yaml`:

```yaml
media_player:
  - platform: universal
    name: Living Room TV
    unique_id: living_room_tv_universal
    attributes:
      state: sensor.living_room_tv_state
      volume_level: number.living_room_tv_volume
      source: select.living_room_tv_source
      source_list: select.living_room_tv_source|options
      media_title: sensor.living_room_tv_title
      media_artist: sensor.living_room_tv_artist
    commands:
      turn_on:
        action: switch.turn_on
        target: { entity_id: switch.living_room_tv_power }
      turn_off:
        action: switch.turn_off
        target: { entity_id: switch.living_room_tv_power }
      volume_set:
        action: number.set_value
        target: { entity_id: number.living_room_tv_volume }
        data:
          value: "{{ volume_level }}"
      media_play:
        action: button.press
        target: { entity_id: button.living_room_tv_play }
      media_pause:
        action: button.press
        target: { entity_id: button.living_room_tv_pause }
      media_stop:
        action: button.press
        target: { entity_id: button.living_room_tv_stop }
      media_next_track:
        action: button.press
        target: { entity_id: button.living_room_tv_next }
      media_previous_track:
        action: button.press
        target: { entity_id: button.living_room_tv_previous }
      select_source:
        action: select.select_option
        target: { entity_id: select.living_room_tv_source }
        data:
          option: "{{ source }}"
```

A few things worth knowing:
- `attributes` entries take `entity_id` (reads that entity's *state*) or `entity_id|attribute_name` (reads one of its attributes) - that's why `source_list` reads the select's `options` attribute rather than its state.
- The `state:` attribute must resolve to one of Home Assistant's own media player states (`off`, `on`, `idle`, `playing`, `paused`, `buffering`) - that's exactly what the `MediaPlayerPlaybackState` constants give you.
- `volume_level` is expected to be `0.0`-`1.0`, which is why `MediaPlayerConfig.VolumeMax` defaults to `1` rather than `100`.
- Entity ids above assume Home Assistant's default id-generation from each sub-entity's `object_id`/`unique_id`; check the actual ids assigned in Home Assistant's entity list if yours differ.

Omit `Sources`/`SupportsMute` (and the corresponding `attributes`/`commands` entries) if you don't need them - `MediaPlayer.Source` and `.Mute` are `null` when not configured.

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

## Documentation site

The [`.github/workflows/docs.yml`](.github/workflows/docs.yml) workflow builds [`docs/`](docs) with [DocFX](https://dotnet.github.io/docfx/) - the getting-started/entities/media-player articles plus a full API reference generated from the library's XML doc comments - and deploys it to GitHub Pages on every push to `main` that touches `src/` or `docs/`. This needs GitHub Pages enabled once, under the repo's **Settings → Pages → Source: GitHub Actions**.

To build and preview it locally:

```bash
dotnet tool install -g docfx   # once
docfx docs/docfx.json --serve  # builds, then serves at http://localhost:8080
```

## License

Apache-2.0, see [LICENSE](LICENSE).
