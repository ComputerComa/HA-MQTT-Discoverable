using System.Globalization;

namespace HaMqttDiscoverable.Entities;

/// <summary>A calendar date that can be read and set from Home Assistant, formatted as ISO 8601 ("yyyy-MM-dd").</summary>
public sealed class Date : HaEntity<DateConfig>
{
    /// <inheritdoc />
    protected override bool SupportsCommands => true;

    /// <summary>Raised when Home Assistant sends a new date.</summary>
    public event Func<DateOnly, Task>? CommandReceived;

    /// <summary>Creates a date entity from the given config, optionally subscribing `onCommand` to `CommandReceived`.</summary>
    public Date(HaMqttConnection connection, DateConfig config, Func<DateOnly, Task>? onCommand = null)
        : base(connection, config)
    {
        if (onCommand is not null)
        {
            CommandReceived += onCommand;
        }
    }

    /// <summary>Publishes the current date.</summary>
    public Task PublishStateAsync(DateOnly value, bool retain = true, CancellationToken cancellationToken = default)
        => PublishStateAsync(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), retain, cancellationToken);

    /// <inheritdoc />
    protected override async Task OnCommandReceivedAsync(string payload)
    {
        if (CommandReceived is null)
        {
            return;
        }

        if (DateOnly.TryParse(payload, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value))
        {
            await CommandReceived.Invoke(value).ConfigureAwait(false);
        }
    }
}
