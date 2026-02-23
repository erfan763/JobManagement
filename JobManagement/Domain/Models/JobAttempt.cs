using JobManagement.Domain.Identifiers;

namespace JobManagement.Domain.Models;

/// <summary>
///     Represents a single attempt to execute a job.
/// </summary>
public sealed record JobAttempt
{
    /// <summary>
    ///     Gets Unique run id for this attempt.
    /// </summary>
    public JobRunId RunId { get; init; }

    /// <summary>
    ///     Gets Attempt number (1-based).
    /// </summary>
    public int AttemptNumber { get; init; }

    /// <summary>
    ///     Gets UTC start time of the attempt.
    /// </summary>
    public DateTimeOffset StartedAtUtc { get; init; }

    /// <summary>
    ///     Gets UTC end time of the attempt (if finished).
    /// </summary>
    public DateTimeOffset? FinishedAtUtc { get; init; }

    /// <summary>
    ///     Gets optional error summary for a failed attempt.
    /// </summary>
    public string Error { get; init; }

    /// <summary>
    ///     Gets optional error details (stack trace, etc).
    /// </summary>
    public string ErrorDetails { get; init; }

    /// <summary>
    ///     Creates a started attempt instance.
    /// </summary>
    /// <returns>Return Start JubAttempt. </returns>
    public static JobAttempt Start(int attemptNumber, DateTimeOffset startedAtUtc)
    {
        return new JobAttempt
        {
            RunId = JobRunId.New(),
            AttemptNumber = attemptNumber,
            StartedAtUtc = startedAtUtc,
        };
    }

    /// <summary>
    ///     Marks the attempt as succeeded.
    /// </summary>
    /// <returns>Return Succeed JubAttempt. </returns>
    public JobAttempt Succeed(DateTimeOffset finishedAtUtc)
    {
        return this with
        {
            FinishedAtUtc = finishedAtUtc,
            Error = null,
            ErrorDetails = null,
        };
    }

    /// <summary>
    ///     Marks the attempt as failed.
    /// </summary>
    /// <returns>Return Fail JubAttempt. </returns>
    public JobAttempt Fail(DateTimeOffset finishedAtUtc, string error, string errorDetails = null)
    {
        return this with
        {
            FinishedAtUtc = finishedAtUtc,
            Error = error,
            ErrorDetails = errorDetails,
        };
    }
}