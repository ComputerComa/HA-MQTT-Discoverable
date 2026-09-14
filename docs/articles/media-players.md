# Media players

Home Assistant's MQTT integration has no native `media_player` platform - only the component types covered in [Entities](entities.md) are MQTT-discoverable. <xref:HaMqttDiscoverable.Entities.MediaPlayer> works around that by publishing a bundle of real entities (a power switch, a volume number, optionally a mute switch and a source select, sensors for playback state/title/artist, and play/pause/stop/next/previous buttons) grouped under one device.

> [!NOTE]
> This isn't a workaround for a missing feature in this library - Home Assistant's `mqtt` integration genuinely has never supported a `media_player` platform (checked against `homeassistant/components/mqtt/const.py` in Home Assistant core, from the 2023.1.0 release through current `dev`). A discovery payload published to `homeassistant/media_player/.../config` would simply be ignored.

## Creating one

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

`Sources` and `SupportsMute` are optional - `MediaPlayer.Source` and `MediaPlayer.Mute` are `null` when you don't set them.

See the <xref:HaMqttDiscoverable.Entities.MediaPlayer> API reference for the full list of sub-entities, events, and `PublishXAsync` helpers, plus <xref:HaMqttDiscoverable.Entities.MediaPlayerSnapshot> for publishing several fields at once.

## Wiring it into Home Assistant as one entity

To present these sub-entities as a single `media_player` entity in Home Assistant's UI, wire them together with Home Assistant's built-in **[Universal Media Player](https://www.home-assistant.io/integrations/universal/)** (`universal`) platform, in `configuration.yaml`:

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
- The `state:` attribute must resolve to one of Home Assistant's own media player states (`off`, `on`, `idle`, `playing`, `paused`, `buffering`) - that's exactly what the <xref:HaMqttDiscoverable.Entities.MediaPlayerPlaybackState> constants give you.
- `volume_level` is expected to be `0.0`-`1.0`, which is why `MediaPlayerConfig.VolumeMax` defaults to `1` rather than `100`.
- Entity ids above assume Home Assistant's default id-generation from each sub-entity's `object_id`/`unique_id`; check the actual ids assigned in Home Assistant's entity list if yours differ.
- Omit the `Sources`/`SupportsMute` config (and the corresponding `attributes`/`commands` entries) if you don't need them.
