using JobManagement.Domain.Identifiers;

namespace JobManagement.Execution.Exceptions;

/// <summary>
///     Thrown when a job lease cannot be renewed or validated during processing.
///     This typically indicates that another worker may have taken over the job.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="JobLeaseLostException" /> class.
///     Creates a new instance of <see cref="JobLeaseLostException" />.
/// </remarks>
public sealed class JobLeaseLostException(JobId jobId, string leaseToken)
    : InvalidOperationException($"Lease lost for job '{jobId}'. The lease token is invalid or expired.")
{
    /// <summary>
    ///     Gets job id.
    /// </summary>
    public JobId JobId { get; } = jobId;

    /// <summary>
    ///     Gets lease token (opaque).
    /// </summary>
    public string LeaseToken { get; } = leaseToken;
}