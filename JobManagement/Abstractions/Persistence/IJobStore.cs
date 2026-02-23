using JobManagement.Domain.Identifiers;
using JobManagement.Domain.Models;
using JobManagement.Domain.Requests;
using JobManagement.Domain.Results;

namespace JobManagement.Abstractions.Persistence;

/// <summary>
///     Persistence contract for JobManagement.
///     Implement this interface in your application using SQL, MongoDB, Redis, etc.
///     This library does not ship with a database provider. :contentReference[oaicite:3]{index=3}
/// </summary>
public interface IJobStore
{
    /// <summary>
    ///     Inserts a new job.
    /// </summary>
    /// <returns>
    ///     A <see cref="Task" /> representing the asynchronous operation.
    /// </returns>
    Task CreateAsync(Job job, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a job by id.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    Task<Job> GetAsync(JobId jobId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates an existing job (full replace or upsert is store-defined, but must be consistent).
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    Task UpdateAsync(Job job, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Searches jobs using a storage-agnostic query model.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    Task<PagedResult<Job>> QueryAsync(JobQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Attempts to dequeue the next available job and returns a lease if successful.
    ///     Store should ensure only one worker can receive the same job at a time.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    Task<DequeueJobResult> DequeueAsync(DequeueJobRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Renews an existing lease to prevent it from expiring during long processing.
    /// </summary>
    /// <param name="jobId">Job id.</param>
    /// <param name="leaseToken">Opaque lease token.</param>
    /// <param name="additionalTime">How much to extend the lease.</param>
    /// <returns>True if renewed; false if lease is invalid/expired.</returns>
    Task<bool> RenewLeaseAsync(
        JobId jobId,
        string leaseToken,
        TimeSpan additionalTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Completes a job after successful execution. Must validate the lease token.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    Task<bool> CompleteAsync(
        JobId jobId,
        string leaseToken,
        Job jobSnapshot,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Marks a job as failed after execution. Must validate the lease token.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    Task<bool> FailAsync(
        JobId jobId,
        string leaseToken,
        Job jobSnapshot,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Cancels a job (store should enforce valid transitions if desired).
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    Task<bool> CancelAsync(JobId jobId, CancellationToken cancellationToken = default);
}