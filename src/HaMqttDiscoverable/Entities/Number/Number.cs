using System.Globalization;

namespace HaMqttDiscoverable.Entities;

/// <summary>A numeric value that can be read and set from Home Assistant, e.g. a target temperature.</summary>
public sealed class Number : HaEntity<NumberConfig>
{
    /// <inheritdoc />
    protected override bool SupportsCommands => true;

    /// <summary>Raised when Home Assistant sends a new value.</summary>
    public event Func<double, Task>? CommandReceived;

    /// <summary>Creates a number from the given config, optionally subscribing <paramref name="onCommand"/> to <see cref="CommandReceived"/>.</summary>
    public Number(HaMqttConnection connection, NumberConfig config, Func<double, Task>? onCommand = null)
        : base(connection, config)
    {
        if (onCommand is not null)
        {
            CommandReceived += onCommand;
        }
    }

    /// <summary>Publishes the current value.</summary>
    public Task PublishStateAsync(double value, bool retain = true, CancellationToken cancellationToken = default)
        => PublishStateAsync(value.ToString(CultureInfo.InvariantCulture), retain, cancellationToken);

    /// <inheritdoc />
    protected override async Task OnCommandReceivedAsync(string payload)
    {
        if (CommandReceived is null)
        {
            return;
        }

        if (double.TryParse(payload, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            await CommandReceived.Invoke(value).ConfigureAwait(false);
        }
    }
}
