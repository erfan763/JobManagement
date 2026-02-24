using JobManagement.Domain.Enums;

namespace JobManagement.Domain.Models;

/// <summary>
///     Captures runtime state of a job (attempts, timestamps, and status).
/// </summary>
public sealed record JobState
{
    /// <summary>
    ///     Gets or sets current status.
    /// </summary>
    public JobStatus Status { get; set; } = JobStatus.Queued;

    /// <summary>
    ///     Gets or sets UTC time when the job was created.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets or sets UTC time when the job was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets or sets UTC time when the job is eligible to run (for scheduled jobs).
    /// </summary>
    public DateTimeOffset? RunAtUtc { get; set; }

    /// <summary>
    ///     Gets or sets Attempt history (ordered).
    /// </summary>
    public IReadOnlyList<JobAttempt> Attempts { get; set; } = [];

    /// <summary>
    ///     Gets or sets Optional correlation id from the caller (e.g., request id / trace id).
    /// </summary>
    public string CorrelationId { get; set; }

    /// <summary>
    ///     Gets or sets   Optional arbitrary tags/labels for searching.
    /// </summary>
    public IReadOnlyDictionary<string, string> Tags { get; set; } =
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