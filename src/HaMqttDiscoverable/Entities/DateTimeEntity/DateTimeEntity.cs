using System.Globalization;

namespace HaMqttDiscoverable.Entities;

/// <summary>
/// A combined date and time value that can be read and set from Home Assistant, formatted as
/// ISO 8601 ("yyyy-MM-ddTHH:mm:ss"). Named <c>DateTimeEntity</c> rather than <c>DateTime</c> to
/// avoid colliding with <see cref="System.DateTime"/>.
/// </summary>
public sealed class DateTimeEntity : HaEntity<DateTimeEntityConfig>
{
    /// <inheritdoc />
    protected override bool SupportsCommands => true;

    /// <summary>Raised when Home Assistant sends a new date/time.</summary>
    public event Func<DateTimeOffset, Task>? CommandReceived;

    /// <summary>Creates a date/time entity from the given config, optionally subscribing `onCommand` to `CommandReceived`.</summary>
    public DateTimeEntity(HaMqttConnection connection, DateTimeEntityConfig config, Func<DateTimeOffset, Task>? onCommand = null)
        : base(connection, config)
    {
        if (onCommand is not null)
        {
            CommandReceived += onCommand;
        }
    }

    /// <summary>Publishes the current date/time.</summary>
    public Task PublishStateAsync(DateTimeOffset value, bool retain = true, CancellationToken cancellationToken = default)
        => PublishStateAsync(value.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture), retain, cancellationToken);

    /// <inheritdoc />
    protected override async Task OnCommandReceivedAsync(string payload)
    {
        if (CommandReceived is null)
        {
            return;
        }

        if (DateTimeOffset.TryParse(payload, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value))
        {
            await CommandReceived.Invoke(value).ConfigureAwait(false);
        }
    }
}
