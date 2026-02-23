namespace JobManagement.Domain.Enums;

/// <summary>
///     Represents the lifecycle state of a job.
/// </summary>
public enum JobStatus
{
    /// <summary>
    ///     Job is created but not yet eligible to run (e.g., scheduled in the future).
    /// </summary>
    Scheduled = 0,

    /// <summary>
    ///     Job is ready to be picked up by a worker.
    /// </summary>
    Queued = 1,

    /// <summary>
    ///     Job is currently executing.
    /// </summary>
    Running = 2,

    /// <summary>
    ///     Job completed successfully.
    /// </summary>
    Succeeded = 3,

    /// <summary>
    ///     Job failed and may retry based on policy.
    /// </summary>
    Failed = 4,

    /// <summary>
    ///     Job was cancelled by user/system request.
    /// </summary>
    Cancelled = 5,

    /// <summary>
    ///     Job is paused and should not be executed until resumed.
    /// </summary>
    Paused = 6,

    /// <summary>
    ///     Job is permanently dead and will never run again (e.g., max retries exceeded).
    /// </summary>
    DeadLettered = 7,
}