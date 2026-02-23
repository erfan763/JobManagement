namespace JobManagement.Abstractions.Clock;

/// <summary>
///     Default clock implementation based on system time.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}