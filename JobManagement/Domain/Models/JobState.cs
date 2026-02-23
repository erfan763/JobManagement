using JobManagement.Domain.Enums;

namespace JobManagement.Domain.Models;

/// <summary>
///     Captures runtime state of a job (attempts, timestamps, and status).
/// </summary>
public sealed record JobState
{
    /// <summary>
    ///     Gets Current status.
    /// </summary>
    public JobStatus Status { get; init; } = JobStatus.Queued;

    /// <summary>
    ///     Gets UTC time when the job was created.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets UTC time when the job was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets UTC time when the job is eligible to run (for scheduled jobs).
    /// </summary>
    public DateTimeOffset? RunAtUtc { get; init; }

    /// <summary>
    ///     Gets Attempt history (ordered).
    /// </summary>
    public IReadOnlyList<JobAttempt> Attempts { get; init; } = [];

    /// <summary>
    ///     Gets Optional correlation id from the caller (e.g., request id / trace id).
    /// </summary>
    public string CorrelationId { get; init; }

    /// <summary>
    ///     Gets   Optional arbitrary tags/labels for searching.
    /// </summary>
    public IReadOnlyDictionary<string, string> Tags { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    ///     Gets Total attempts so far.
    /// </summary>
    public int AttemptCount => Attempts.Count;

    /// <summary>
    ///     Gets Returns the last attempt if exists.
    /// </summary>
    public JobAttempt LastAttempt => Attempts.Count == 0 ? null : Attempts[^1];
}