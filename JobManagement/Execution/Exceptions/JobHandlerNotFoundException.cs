using JobManagement.Domain.Identifiers;

namespace JobManagement.Execution.Exceptions;

/// <summary>
///     Thrown when no handler is registered for a job payload type.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="JobHandlerNotFoundException" /> class.
///     Creates a new instance of <see cref="JobHandlerNotFoundException" />.
/// </remarks>
/// <param name="jobId">Job id.</param>
/// <param name="payloadType">Payload type.</param>
public sealed class JobHandlerNotFoundException(JobId jobId, string payloadType)
    : InvalidOperationException($"No handler registered for payload type '{payloadType}' (job: {jobId}).")
{
    /// <summary>
    ///     Gets job id for which the handler was required.
    /// </summary>
    public JobId JobId { get; } = jobId;

    /// <summary>
    ///     Gets the missing payload type.
    /// </summary>
    public string PayloadType { get; } = payloadType;
}