using JobManagement.Domain.Identifiers;

namespace JobManagement.Domain.Results;

/// <summary>
///     Result returned after enqueueing a job.
/// </summary>
public sealed record EnqueueJobResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EnqueueJobResult" /> class.
    ///     Creates a result.
    /// </summary>
    /// <param name="jobId">Created job id.</param>
    public EnqueueJobResult(JobId jobId)
    {
        JobId = jobId;
    }

    /// <summary>
    ///     Gets The newly created job id.
    /// </summary>
    public JobId JobId { get; init; }
}