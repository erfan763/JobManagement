using JobManagement.Domain.Models;

namespace JobManagement.Abstractions.Handlers;

/// <summary>
///     Executes a job. Applications provide implementations per job type (payload type).
/// </summary>
public interface IJobHandler
{
    /// <summary>
    ///     Gets the payload type this handler supports (e.g., "SendEmail").
    /// </summary>
    string Type { get; }

    /// <summary>
    ///     Executes the job.
    /// </summary>
    /// <param name="job">Job snapshot to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    Task<JobExecutionResult> HandleAsync(Job job, CancellationToken cancellationToken = default);
}