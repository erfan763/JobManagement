using JobManagement.Domain.Enums;
using JobManagement.Domain.ValueObjects;

namespace JobManagement.Domain.Models;

/// <summary>
///     Immutable job definition: what to do and how to schedule/retry it.
/// </summary>
public sealed record JobDefinition
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="JobDefinition" /> class.
    ///     Creates a new <see cref="JobDefinition" />.
    /// </summary>
    /// <param name="payload">Job payload.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="payload" /> is null.</exception>
    public JobDefinition(JobPayload payload)
    {
        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
    }

    /// <summary>
    ///     Gets Payload describing the job work.
    /// </summary>
    public JobPayload Payload { get; init; }

    /// <summary>
    ///     Gets Priority used for job ordering.
    /// </summary>
    public JobPriority Priority { get; set; } = JobPriority.Normal;

    /// <summary>
    ///     Gets Retry policy used when a job fails.
    /// </summary>
    public RetryPolicy RetryPolicy { get; set; } = RetryPolicy.Default;

    /// <summary>
    ///     Gets Optional timeout for a single attempt.
    /// </summary>
    public TimeSpan? AttemptTimeout { get; init; }

    /// <summary>
    ///     Gets  Optional name (human-friendly).
    /// </summary>
    public string DisplayName { get; init; }
}