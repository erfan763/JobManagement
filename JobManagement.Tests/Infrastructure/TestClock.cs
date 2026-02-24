using JobManagement.Abstractions.Clock;

namespace JobManagement.Tests.Infrastructure;

/// <summary>
///     Deterministic clock for tests.
/// </summary>
public sealed class TestClock(DateTimeOffset? initialUtc = null) : IClock
{
    public DateTimeOffset UtcNow { get; private set; } =
        initialUtc ?? DateTimeOffset.UtcNow;

    public void Set(DateTimeOffset utc)
    {
        UtcNow = utc;
    }

    public void Advance(TimeSpan delta)
    {
        UtcNow = UtcNow.Add(delta);
    }
}