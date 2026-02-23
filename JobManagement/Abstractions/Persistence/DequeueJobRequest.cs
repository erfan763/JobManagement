using JobManagement.Domain.Enums;

namespace JobManagement.Abstractions.Persistence;

/// <summary>
///     Represents a request to dequeue the next available job.
/// </summary>
public sealed record DequeueJobRequest
{
    /// <summary>
    ///     Gets Worker id used for leasing (required).
    /// </summary>
    public string WorkerId { get; init; } = string.Empty;

    /// <summary>
    ///     Gets Duration of the initial lease.
    /// </summary>
    public TimeSpan LeaseDuration { get; init; } = TimeSpan.FromMinutes(1);

    /// <summary>
    ///     Gets Optional filter by statuses. If null, store should assume "ready to run" jobs (typically Queued).
    /// </summary>
    public IReadOnlyCollection<JobStatus> Statuses { get; init; }

    /// <summary>
    ///     Gets Maximum number of jobs to scan internally (store-specific optimization hint).
    /// </summary>
    public int ScanLimit { get; init; } = 100;
}