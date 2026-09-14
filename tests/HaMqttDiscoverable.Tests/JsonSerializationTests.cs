using System.Text.Json;
using HaMqttDiscoverable.Json;
using Xunit;

namespace HaMqttDiscoverable.Tests;

public sealed class JsonSerializationTests
{
    [Fact]
    public void Device_SerializesWithSnakeCaseKeysAndOmitsNulls()
    {
        var device = Device.Create("id-1", "Weather Station", "Acme", "WS-100");

        var json = JsonSerializer.Serialize(device, HaJsonOptions.Default);

        Assert.Contains("\"identifiers\":[\"id-1\"]", json);
        Assert.Contains("\"name\":\"Weather Station\"", json);
        Assert.Contains("\"manufacturer\":\"Acme\"", json);
        Assert.Contains("\"model\":\"WS-100\"", json);
        Assert.DoesNotContain("hw_version", json);
        Assert.DoesNotContain("via_device", json);
    }

    [Fact]
    public void Device_WithFullDetails_SerializesEveryField()
    {
        var device = new Device
        {
            Identifiers = new List<string> { "id-1" },
            Name = "Weather Station",
            Manufacturer = "Acme",
            Model = "WS-100",
            SoftwareVersion = "1.2.3",
            HardwareVersion = "rev-b",
            ConfigurationUrl = "https://example.com",
            ViaDevice = "hub-1",
            SuggestedArea = "Garden",
        };

        var json = JsonSerializer.Serialize(device, HaJsonOptions.Default);

        Assert.Contains("\"sw_version\":\"1.2.3\"", json);
        Assert.Contains("\"hw_version\":\"rev-b\"", json);
        Assert.Contains("\"configuration_url\":\"https://example.com\"", json);
        Assert.Contains("\"via_device\":\"hub-1\"", json);
        Assert.Contains("\"suggested_area\":\"Garden\"", json);
    }
}
