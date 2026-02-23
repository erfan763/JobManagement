using JobManagement.Domain.Enums;
using JobManagement.Domain.ValueObjects;

namespace JobManagement.Domain.Requests;

/// <summary>
///     Request model used by the public API to enqueue a job.
/// </summary>
public sealed record EnqueueJobRequest
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EnqueueJobRequest" /> class.
    ///     Creates a new enqueue request.
    /// </summary>
    /// <param name="payload">Job payload.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="payload" /> is null.</exception>
    public EnqueueJobRequest(JobPayload payload)
    {
        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
    }

    /// <summary>
    ///     Gets Job payload.
    /// </summary>
    public JobPayload Payload { get; init; }

    /// <summary>
    ///     Gets Optional priority (default: Normal).
    /// </summary>
    public JobPriority Priority { get; init; } = JobPriority.Normal;

    /// <summary>
    ///     Gets Optional retry policy (default: <see cref="RetryPolicy.Default" />).
    /// </summary>
    public RetryPolicy RetryPolicy { get; init; } = RetryPolicy.Default;

    /// <summary>
    ///     Gets Optional job run time in UTC (null means now/queued).
    /// </summary>
    public DateTimeOffset? RunAtUtc { get; init; }

    /// <summary>
    ///     Gets Optional attempt timeout.
    /// </summary>
    public TimeSpan? AttemptTimeout { get; init; }

    /// <summary>
    ///     Gets Optional correlation id for tracing.
    /// </summary>
    public string CorrelationId { get; init; }

    /// <summary>
    ///     Gets Optional tags for indexing/searching in the host application.
    /// </summary>
    public IReadOnlyDictionary<string, string> Tags { get; init; }

    /// <summary>
    ///     Gets Optional human-friendly name.
    /// </summary>
    public string DisplayName { get; init; }
}