namespace JobManagement.Abstractions.Clock;

/// <summary>
///     Provides the current time. Useful for testability and deterministic behavior.
/// </summary>
public interface IClock
{
    /// <summary>
    ///     Gets the current UTC time.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}