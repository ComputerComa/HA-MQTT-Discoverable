using System.Globalization;

namespace HaMqttDiscoverable.Entities;

/// <summary>A time-of-day value that can be read and set from Home Assistant, formatted as ISO 8601 ("HH:mm:ss").</summary>
public sealed class Time : HaEntity<TimeConfig>
{
    /// <inheritdoc />
    protected override bool SupportsCommands => true;

    /// <summary>Raised when Home Assistant sends a new time.</summary>
    public event Func<TimeOnly, Task>? CommandReceived;

    /// <summary>Creates a time entity from the given config, optionally subscribing `onCommand` to `CommandReceived`.</summary>
    public Time(HaMqttConnection connection, TimeConfig config, Func<TimeOnly, Task>? onCommand = null)
        : base(connection, config)
    {
        if (onCommand is not null)
        {
            CommandReceived += onCommand;
        }
    }

    /// <summary>Publishes the current time.</summary>
    public Task PublishStateAsync(TimeOnly value, bool retain = true, CancellationToken cancellationToken = default)
        => PublishStateAsync(value.ToString("HH:mm:ss", CultureInfo.InvariantCulture), retain, cancellationToken);

    /// <inheritdoc />
    protected override async Task OnCommandReceivedAsync(string payload)
    {
        if (CommandReceived is null)
        {
            return;
        }

        if (TimeOnly.TryParse(payload, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value))
        {
            await CommandReceived.Invoke(value).ConfigureAwait(false);
        }
    }
}
