namespace JobManagement.Domain.ValueObjects;

/// <summary>
///     Defines retry behavior for a job.
/// </summary>
/// <param name="MaxAttempts">
///     Maximum number of attempts allowed (including the first attempt).
///     Use 1 to disable retries.
/// </param>
/// <param name="BaseDelay">
///     Base delay used to compute backoff (e.g., exponential).
/// </param>
/// <param name="MaxDelay">
///     Maximum delay cap for backoff.
/// </param>
/// <param name="UseExponentialBackoff">
///     If true, delay grows exponentially with attempts; otherwise uses constant <paramref name="BaseDelay" />.
/// </param>
public sealed record RetryPolicy(
    int MaxAttempts,
    TimeSpan BaseDelay,
    TimeSpan MaxDelay,
    bool UseExponentialBackoff = true)
{
    /// <summary>
    ///     Gets a default retry policy: 3 attempts, exponential backoff, base 5s, max 2m.
    /// </summary>
    public static RetryPolicy Default { get; } =
        new(3, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(2));

    /// <summary>
    ///     Gets a no-retry policy: only 1 attempt.
    /// </summary>
    public static RetryPolicy NoRetry { get; } =
        new(1, TimeSpan.Zero, TimeSpan.Zero, false);

    /// <summary>
    ///     Calculates the delay before the next attempt.
    /// </summary>
    /// <param name="nextAttemptNumber">The next attempt number (1-based).</param>
    /// <returns>Delay duration. Can be zero.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="nextAttemptNumber" /> is less than 1.</exception>
    public TimeSpan ComputeDelay(int nextAttemptNumber)
    {
        if (nextAttemptNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(nextAttemptNumber), "Attempt number must be >= 1.");
        }

        if (MaxAttempts <= 1 || BaseDelay <= TimeSpan.Zero)
        {
            return TimeSpan.Zero;
        }

        if (!UseExponentialBackoff)
        {
            return Clamp(BaseDelay, TimeSpan.Zero, MaxDelay);
        }

        // attempt 1 => delay 0 (first run has no delay)
        // attempt 2 => baseDelay * 1
        // attempt 3 => baseDelay * 2
        // attempt 4 => baseDelay * 4 ...
        if (nextAttemptNumber <= 1)
        {
            return TimeSpan.Zero;
        }

        int exponent = nextAttemptNumber - 2; // 0,1,2...
        long factor = 1L << Math.Min(exponent, 30); // cap shifts to avoid overflow
        long rawTicks = BaseDelay.Ticks * factor;

        // avoid overflow to TimeSpan
        long safeTicks = rawTicks < 0 ? long.MaxValue : rawTicks;
        var delay = new TimeSpan(Math.Min(safeTicks, TimeSpan.MaxValue.Ticks));

        return Clamp(delay, TimeSpan.Zero, MaxDelay);
    }

    /// <summary>
    ///     Restricts a <see cref="TimeSpan" /> value to a specified range.
    /// </summary>
    /// <param name="value">
    ///     The <see cref="TimeSpan" /> value to clamp.
    /// </param>
    /// <param name="min">
    ///     The minimum allowed value. If <paramref name="value" /> is less than this,
    ///     <paramref name="min" /> will be returned.
    /// </param>
    /// <param name="max">
    ///     The maximum allowed value. If greater than <see cref="TimeSpan.Zero" /> and
    ///     <paramref name="value" /> exceeds this value, <paramref name="max" /> will be returned.
    ///     If <paramref name="max" /> is <see cref="TimeSpan.Zero" /> or negative, the upper bound is ignored.
    /// </param>
    /// <returns>
    ///     A <see cref="TimeSpan" /> within the specified range:
    ///     <list type="bullet">
    ///         <item>
    ///             <description><paramref name="min" /> if <paramref name="value" /> is less than <paramref name="min" />.</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="max" /> if <paramref name="max" /> is positive and <paramref name="value" />
    ///                 is greater than <paramref name="max" />.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>Otherwise, the original <paramref name="value" />.</description>
    ///         </item>
    ///     </list>
    /// </returns>
    private static TimeSpan Clamp(TimeSpan value, TimeSpan min, TimeSpan max)
    {
        if (value < min) return min;
        if (max > TimeSpan.Zero && value > max) return max;
        return value;
    }
}