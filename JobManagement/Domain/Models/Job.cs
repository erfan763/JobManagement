using JobManagement.Domain.Enums;
using JobManagement.Domain.Exceptions;
using JobManagement.Domain.Identifiers;

namespace JobManagement.Domain.Models;

/// <summary>
///     Main aggregate root representing a job in the system.
///     Persistence is intentionally not included in this package.
/// </summary>
public sealed record Job
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Job" /> class.
    ///     Creates a new job instance.
    /// </summary>
    /// <param name="id">Job id.</param>
    /// <param name="definition">Job definition.</param>
    /// <param name="state">Job state.</param>
    /// <exception cref="ArgumentNullException">If required parameters are null.</exception>
    public Job(JobId id, JobDefinition definition, JobState state)
    {
        Id = id;
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        State = state ?? throw new ArgumentNullException(nameof(state));
    }

    /// <summary>
    ///     Gets Job id.
    /// </summary>
    public JobId Id { get; init; }

    /// <summary>
    ///     Gets Job definition (payload, retry policy, priority, etc).
    /// </summary>
    public JobDefinition Definition { get; init; }

    /// <summary>
    ///     Gets Current job state.
    /// </summary>
    public JobState State { get; init; }

    /// <summary>
    ///     Gets Creates a new queued job with a generated id.
    /// </summary>
    /// <returns>Return Created Job. </returns>
    public static Job CreateQueued(JobDefinition definition, DateTimeOffset nowUtc)
    {
        return new Job(
            JobId.New(),
            definition,
            new JobState
            {
                Status = JobStatus.Queued,
                CreatedAtUtc = nowUtc,
                UpdatedAtUtc = nowUtc,
                RunAtUtc = null,
            });
    }

    /// <summary>
    ///     Creates a new scheduled job with a generated id.
    /// </summary>
    /// <returns>Return Created Scheduled Job. </returns>
    public static Job CreateScheduled(JobDefinition definition, DateTimeOffset runAtUtc, DateTimeOffset nowUtc)
    {
        return new Job(
            JobId.New(),
            definition,
            new JobState
            {
                Status = JobStatus.Scheduled,
                CreatedAtUtc = nowUtc,
                UpdatedAtUtc = nowUtc,
                RunAtUtc = runAtUtc,
            });
    }

    /// <summary>
    ///     Validates whether the job may transition to the next status.
    /// </summary>
    /// <param name="next">Next status.</param>
    /// <exception cref="JobStateTransitionException">If the transition is not allowed.</exception>
    public void EnsureCanTransitionTo(JobStatus next)
    {
        JobStatus current = State.Status;

        if (current == next)
        {
            return;
        }

        // A simple, safe default state machine.
        bool ok =
            (current == JobStatus.Scheduled && next is JobStatus.Queued or JobStatus.Cancelled or JobStatus.Paused) ||
            (current == JobStatus.Queued && next is JobStatus.Running or JobStatus.Cancelled or JobStatus.Paused) ||
            (current == JobStatus.Running && next is JobStatus.Succeeded or JobStatus.Failed or JobStatus.Cancelled) ||
            (current == JobStatus.Failed && next is JobStatus.Queued or JobStatus.DeadLettered) ||
            (current == JobStatus.Paused && next is JobStatus.Queued or JobStatus.Cancelled);

        if (!ok)
        {
            throw new JobStateTransitionException(Id, current, next);
        }
    }

    /// <summary>
    ///     Returns a copy of this job with updated state and status.
    /// </summary>
    /// <param name="nextStatus">Next status.</param>
    /// <param name="nowUtc">Current time (UTC).</param>
    /// <returns>Return Job Statue. </returns>
    public Job TransitionTo(JobStatus nextStatus, DateTimeOffset nowUtc)
    {
        EnsureCanTransitionTo(nextStatus);

        JobState nextState = State with
        {
            Status = nextStatus,
            UpdatedAtUtc = nowUtc,
        };

        // If it becomes queued, clear RunAt for immediate pickup (optional design choice).
        if (nextStatus == JobStatus.Queued)
        {
            nextState = nextState with
            {
                RunAtUtc = null,
            };
        }

        return this with
        {
            State = nextState,
        };
    }
}