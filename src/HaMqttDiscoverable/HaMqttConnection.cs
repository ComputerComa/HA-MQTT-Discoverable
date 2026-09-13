using MQTTnet;

namespace HaMqttDiscoverable;

/// <summary>
/// Wraps an MQTT connection used to publish Home Assistant discovery configs, entity state,
/// and to receive commands. Create one connection per application (or per broker) and pass it
/// to every <see cref="Entities.HaEntity{TConfig}"/> you create.
/// </summary>
public sealed class HaMqttConnection : IAsyncDisposable
{
    private readonly MqttSettings _settings;
    private readonly IMqttClient _client;
    private readonly Dictionary<string, List<Func<string, Task>>> _subscriptions = new();
    private readonly SemaphoreSlim _subscriptionsLock = new(1, 1);

    /// <summary>The underlying MQTTnet client, exposed for advanced scenarios not covered by this library.</summary>
    public IMqttClient MqttClient => _client;

    /// <summary>The discovery prefix configured on the Home Assistant side (default "homeassistant").</summary>
    public string DiscoveryPrefix => _settings.DiscoveryPrefix;

    /// <summary>
    /// The shared availability topic published by this connection (its MQTT last-will topic),
    /// used by entities that don't define their own <see cref="Availability"/> override.
    /// </summary>
    public string AvailabilityTopic { get; }

    /// <summary>Whether the underlying MQTT client is currently connected.</summary>
    public bool IsConnected => _client.IsConnected;

    /// <summary>Creates a connection from the given settings. Call <see cref="ConnectAsync"/> to actually connect.</summary>
    public HaMqttConnection(MqttSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _client = new MqttClientFactory().CreateMqttClient();
        var clientId = string.IsNullOrEmpty(settings.ClientId)
            ? $"ha-mqtt-discoverable-{Guid.NewGuid():N}"
            : settings.ClientId;
        AvailabilityTopic = $"{settings.DiscoveryPrefix}/status/{clientId}";
        _client.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync;
    }

    /// <summary>Connects to the MQTT broker, registering a last-will message so Home Assistant can detect an unexpected disconnect.</summary>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        var clientId = string.IsNullOrEmpty(_settings.ClientId)
            ? AvailabilityTopic.Split('/')[^1]
            : _settings.ClientId;

        var optionsBuilder = new MqttClientOptionsBuilder()
            .WithTcpServer(_settings.Host, _settings.EffectivePort)
            .WithClientId(clientId)
            .WithCleanSession();

        if (!string.IsNullOrEmpty(_settings.Username))
        {
            optionsBuilder = optionsBuilder.WithCredentials(_settings.Username, _settings.Password);
        }

        if (_settings.UseTls)
        {
            optionsBuilder = optionsBuilder.WithTlsOptions(tls => tls.UseTls());
        }

        if (_settings.PublishClientAvailability)
        {
            optionsBuilder = optionsBuilder
                .WithWillTopic(AvailabilityTopic)
                .WithWillPayload("offline")
                .WithWillRetain(true)
                .WithWillQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
        }

        await _client.ConnectAsync(optionsBuilder.Build(), cancellationToken).ConfigureAwait(false);

        if (_settings.PublishClientAvailability)
        {
            await PublishAsync(AvailabilityTopic, "online", retain: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>Publishes "offline" to the shared availability topic (if enabled) and disconnects gracefully.</summary>
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (!_client.IsConnected)
        {
            return;
        }

        if (_settings.PublishClientAvailability)
        {
            await PublishAsync(AvailabilityTopic, "offline", retain: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        await _client.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().Build(), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Publishes a raw payload to an arbitrary MQTT topic.</summary>
    public async Task PublishAsync(string topic, string payload, bool retain = false, int qos = 0, CancellationToken cancellationToken = default)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithRetainFlag(retain)
            .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)qos)
            .Build();

        await _client.PublishAsync(message, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Subscribes to an MQTT topic, invoking <paramref name="handler"/> with the payload of every message received on it.</summary>
    public async Task SubscribeAsync(string topic, Func<string, Task> handler, int qos = 0, CancellationToken cancellationToken = default)
    {
        await _subscriptionsLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!_subscriptions.TryGetValue(topic, out var handlers))
            {
                handlers = new List<Func<string, Task>>();
                _subscriptions[topic] = handlers;
            }

            handlers.Add(handler);
        }
        finally
        {
            _subscriptionsLock.Release();
        }

        var options = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter(topic, (MQTTnet.Protocol.MqttQualityOfServiceLevel)qos)
            .Build();

        await _client.SubscribeAsync(options, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Removes a previously registered handler for a topic, unsubscribing entirely once no handlers remain.</summary>
    public async Task UnsubscribeAsync(string topic, Func<string, Task> handler, CancellationToken cancellationToken = default)
    {
        var shouldUnsubscribe = false;
        await _subscriptionsLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_subscriptions.TryGetValue(topic, out var handlers))
            {
                handlers.Remove(handler);
                if (handlers.Count == 0)
                {
                    _subscriptions.Remove(topic);
                    shouldUnsubscribe = true;
                }
            }
        }
        finally
        {
            _subscriptionsLock.Release();
        }

        if (shouldUnsubscribe)
        {
            var options = new MqttClientUnsubscribeOptionsBuilder().WithTopicFilter(topic).Build();
            await _client.UnsubscribeAsync(options, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        List<Func<string, Task>>? handlers;
        await _subscriptionsLock.WaitAsync().ConfigureAwait(false);
        try
        {
            _subscriptions.TryGetValue(args.ApplicationMessage.Topic, out handlers);
            handlers = handlers is null ? null : new List<Func<string, Task>>(handlers);
        }
        finally
        {
            _subscriptionsLock.Release();
        }

        if (handlers is null)
        {
            return;
        }

        var payload = args.ApplicationMessage.ConvertPayloadToString() ?? string.Empty;
        foreach (var handler in handlers)
        {
            await handler(payload).ConfigureAwait(false);
        }
    }

    /// <summary>Disconnects (best-effort) and releases the underlying MQTT client.</summary>
    public async ValueTask DisposeAsync()
    {
        try
        {
            await DisconnectAsync().ConfigureAwait(false);
        }
        catch
        {
            // Best-effort: don't let a failed graceful disconnect prevent disposal.
        }

        _client.Dispose();
        _subscriptionsLock.Dispose();
    }
}
