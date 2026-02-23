using JobManagement.Domain.Models;

namespace JobManagement.Abstractions.Persistence;

/// <summary>
///     Represents the result of a dequeue operation.
/// </summary>
public sealed record DequeueJobResult
{
    /// <summary>
    ///     Gets a value indicating whether gets True if a job was dequeued successfully.
    /// </summary>
    public bool Found { get; init; }

    /// <summary>
    ///     Gets The dequeued job, if <see cref="Found" /> is true.
    /// </summary>
    public Job Job { get; init; }

    /// <summary>
    ///     Gets Lease information for the dequeued job, if <see cref="Found" /> is true.
    /// </summary>
    public JobLease Lease { get; init; }

    /// <summary>
    ///     Gets Creates an empty result (no job available).
    /// </summary>
    /// <returns>Return Empty DequeueJobResult Instance. </returns>
    public static DequeueJobResult Empty()
    {
        return new DequeueJobResult
        {
            Found = false,
        };
    }

    /// <summary>
    ///     Gets Creates a successful result.
    /// </summary>
    /// ///
    /// <returns>Return Success DequeueJobResult Instance. </returns>
    public static DequeueJobResult Success(Job job, JobLease lease)
    {
        return new DequeueJobResult
        {
            Found = true,
            Job = job,
            Lease = lease,
        };
    }
}