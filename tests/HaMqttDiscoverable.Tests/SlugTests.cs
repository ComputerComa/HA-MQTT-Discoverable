using Xunit;

namespace HaMqttDiscoverable.Tests;

public sealed class SlugTests
{
    [Theory]
    [InlineData("Front Door!", "front_door")]
    [InlineData("weather-station-1", "weather_station_1")]
    [InlineData("already_slug", "already_slug")]
    [InlineData("  leading and trailing  ", "leading_and_trailing")]
    [InlineData("CamelCaseName", "camelcasename")]
    public void Create_NormalizesToLowercaseUnderscoreSeparated(string input, string expected)
    {
        Assert.Equal(expected, Slug.Create(input));
    }

    [Fact]
    public void Create_ThrowsOnEmpty()
    {
        Assert.Throws<ArgumentException>(() => Slug.Create(""));
    }
}
