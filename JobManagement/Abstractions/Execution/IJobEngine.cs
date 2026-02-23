using JobManagement.Domain.Identifiers;
using JobManagement.Domain.Requests;
using JobManagement.Domain.Results;

namespace JobManagement.Abstractions.Execution;

/// <summary>
///     High-level API for job operations (enqueue and execution orchestration).
///     Concrete implementation will use <see cref="Persistence.IJobStore" /> and handler registry.
/// </summary>
public interface IJobEngine
{
    /// <summary>
    ///     Gets Enqueues a job (immediate or scheduled).
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    Task<EnqueueJobResult> EnqueueAsync(EnqueueJobRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets Attempts to process a single job if available.
    ///     Gets Returns true if a job was found (and processed), otherwise false.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    Task<bool> TryProcessOneAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets Cancels a job.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    Task<bool> CancelAsync(JobId jobId, CancellationToken cancellationToken = default);
}