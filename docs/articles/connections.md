# Connections & availability

## HaMqttConnection

<xref:HaMqttDiscoverable.HaMqttConnection> wraps a single [MQTTnet](https://github.com/dotnet/MQTTnet) client. Create one per application (or per broker) and share it across every entity:

```csharp
var connection = new HaMqttConnection(new MqttSettings
{
    Host = "192.168.1.10",
    Port = 1883,          // defaults to 1883, or 8883 when UseTls is true
    Username = "ha",
    Password = "secret",
    UseTls = false,
    ClientId = null,      // a random id is generated when omitted
    DiscoveryPrefix = "homeassistant", // must match Home Assistant's configured prefix
});

await connection.ConnectAsync();
```

Beyond the entity-specific `PublishDiscoveryAsync`/`PublishStateAsync` helpers, the connection also exposes general-purpose `PublishAsync` and `SubscribeAsync`/`UnsubscribeAsync` methods, and the underlying `MqttClient` (`IMqttClient`) for anything this library doesn't cover directly.

Call `DisposeAsync()` (or `DisconnectAsync()`) when your application shuts down.

## Availability

By default (`MqttSettings.PublishClientAvailability = true`), the connection:

1. Registers an MQTT last-will message on `{DiscoveryPrefix}/status/{ClientId}` with payload `offline`, so the broker publishes it automatically if your process disappears without disconnecting cleanly.
2. Publishes `online` (retained) to that same topic once connected.
3. Publishes `offline` (retained) before disconnecting gracefully.

Every entity's discovery payload references this shared topic as its `availability_topic` automatically - you don't need to configure anything per entity for this to work.

### Overriding availability per entity

Set `Availability` on any `*Config` (inherited from <xref:HaMqttDiscoverable.Entities.EntityConfig>) to use a different topic for just that entity - useful when an entity tracks the availability of its own piece of hardware rather than your whole application:

```csharp
var sensor = new Sensor(connection, new SensorConfig
{
    Name = "Battery",
    UniqueId = "remote-sensor-battery",
    Device = device,
    Availability = new Availability("remote-sensor/lwt")
    {
        PayloadAvailable = "1",
        PayloadNotAvailable = "0",
    },
});
```

See <xref:HaMqttDiscoverable.Availability> for the full set of fields, including `ValueTemplate` for extracting availability from a JSON payload.

## Discovery JSON

Discovery payloads are serialized with the options in <xref:HaMqttDiscoverable.Json.HaJsonOptions> - snake_case property names (matching Home Assistant's discovery schema), enums written as their lowercase Home Assistant values, and null/default fields omitted. You generally won't need to touch this directly; it's used internally by <xref:HaMqttDiscoverable.Entities.HaEntity`1>.
