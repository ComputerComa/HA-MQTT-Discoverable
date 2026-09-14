using System.Net;
using System.Net.Sockets;
using MQTTnet.Server;
using Xunit;

namespace HaMqttDiscoverable.Tests;

/// <summary>Starts a real, local, in-process MQTT broker so tests exercise the actual MQTTnet client wire protocol rather than a mock.</summary>
public sealed class TestBroker : IAsyncLifetime
{
    public int Port { get; private set; }

    /// <summary>The running broker, exposed so tests can inspect its retained-message store directly instead of racing a second MQTT client.</summary>
    public MqttServer Server { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Port = GetFreeTcpPort();

        var options = new MqttServerOptionsBuilder()
            .WithDefaultEndpoint()
            .WithDefaultEndpointBoundIPAddress(IPAddress.Loopback)
            .WithDefaultEndpointBoundIPV6Address(IPAddress.None)
            .WithDefaultEndpointPort(Port)
            .Build();

        Server = new MqttServerFactory().CreateMqttServer(options);
        await Server.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Server.StopAsync(new MqttServerStopOptionsBuilder().Build());
        Server.Dispose();
    }

    private static int GetFreeTcpPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}

[CollectionDefinition(Name)]
public sealed class TestBrokerCollection : ICollectionFixture<TestBroker>
{
    public const string Name = "TestBroker";
}
