using JobManagement.Domain.Enums;
using JobManagement.Domain.Identifiers;

namespace JobManagement.Domain.Exceptions;

/// <summary>
///     Thrown when an invalid job status transition is attempted.
/// </summary>
/// <remarks>
///     Creates the exception.
/// </remarks>
public sealed class JobStateTransitionException(JobId jobId, JobStatus current, JobStatus next)
    : InvalidOperationException($"Invalid status transition for job '{jobId}': {current} -> {next}.")
{
    /// <summary>
    ///     Gets job id for which the transition was attempted.
    /// </summary>
    public JobId JobId { get; } = jobId;

    /// <summary>
    ///     Gets current status.
    /// </summary>
    public JobStatus Current { get; } = current;

    /// <summary>
    ///     Gets desired next status.
    /// </summary>
    public JobStatus Next { get; } = next;
}