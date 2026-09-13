using System.Threading.Channels;
using MQTTnet;

namespace HaMqttDiscoverable.Tests;

/// <summary>A raw MQTT subscriber (independent of <see cref="HaMqttConnection"/>) used to observe exactly what was published to the broker.</summary>
public sealed class MqttMessageCapture : IAsyncDisposable
{
    private readonly IMqttClient _client;
    private readonly Channel<(string Topic, string Payload, bool Retain)> _channel = Channel.CreateUnbounded<(string, string, bool)>();

    private MqttMessageCapture(IMqttClient client)
    {
        _client = client;
    }

    public static async Task<MqttMessageCapture> ConnectAsync(int port)
    {
        var client = new MqttClientFactory().CreateMqttClient();
        var capture = new MqttMessageCapture(client);
        client.ApplicationMessageReceivedAsync += args =>
        {
            var payload = args.ApplicationMessage.ConvertPayloadToString() ?? string.Empty;
            capture._channel.Writer.TryWrite((args.ApplicationMessage.Topic, payload, args.ApplicationMessage.Retain));
            return Task.CompletedTask;
        };

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("127.0.0.1", port)
            .WithClientId($"test-capture-{Guid.NewGuid():N}")
            .WithCleanSession()
            .Build();

        await client.ConnectAsync(options);
        await client.SubscribeAsync(new MqttClientSubscribeOptionsBuilder().WithTopicFilter("#").Build());
        return capture;
    }

    /// <summary>Waits for a message on the given topic, ignoring messages on other topics.</summary>
    public async Task<(string Payload, bool Retain)> WaitForAsync(string topic, TimeSpan? timeout = null)
    {
        using var cts = new CancellationTokenSource(timeout ?? TimeSpan.FromSeconds(5));
        await foreach (var message in _channel.Reader.ReadAllAsync(cts.Token))
        {
            if (message.Topic == topic)
            {
                return (message.Payload, message.Retain);
            }
        }

        throw new TimeoutException($"No message received on topic '{topic}'.");
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _client.DisconnectAsync();
        }
        catch
        {
            // best-effort
        }

        _client.Dispose();
    }
}
