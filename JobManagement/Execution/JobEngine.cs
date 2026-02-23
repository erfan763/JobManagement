using JobManagement.Abstractions.Clock;
using JobManagement.Abstractions.Execution;
using JobManagement.Abstractions.Handlers;
using JobManagement.Abstractions.Persistence;
using JobManagement.Abstractions.Telemetry;
using JobManagement.Domain.Enums;
using JobManagement.Domain.Identifiers;
using JobManagement.Domain.Models;
using JobManagement.Domain.Requests;
using JobManagement.Domain.Results;
using JobManagement.Domain.ValueObjects;
using JobManagement.Execution.Exceptions;

namespace JobManagement.Execution;

/// <summary>
///     Default implementation of <see cref="IJobEngine" />.
///     Orchestrates enqueueing, dequeue with leasing, handler execution, retries, and state transitions.
/// </summary>
public sealed class JobEngine : IJobEngine
{
    private readonly IClock _clock;
    private readonly IJobEventSink _events;
    private readonly IJobHandlerRegistry _handlers;
    private readonly JobEngineOptions _options;
    private readonly IJobStore _store;

    /// <summary>
    ///     Initializes a new instance of the <see cref="JobEngine" /> class.
    ///     Creates a new <see cref="JobEngine" />.
    /// </summary>
    /// <param name="store">Persistence adapter.</param>
    /// <param name="handlers">Handler registry.</param>
    /// <param name="clock">Clock abstraction.</param>
    /// <param name="events">Telemetry sink.</param>
    /// <param name="options">Engine options.</param>
    /// <exception cref="ArgumentNullException">If required parameters are null.</exception>
    public JobEngine(
        IJobStore store,
        IJobHandlerRegistry handlers,
        IClock clock,
        IJobEventSink events = null,
        JobEngineOptions options = null)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _events = events ?? new NullJobEventSink();
        _options = options ?? new JobEngineOptions();

        if (string.IsNullOrWhiteSpace(_options.WorkerId))
        {
            throw new ArgumentException("WorkerId must be provided.", nameof(options));
        }
    }

    /// <inheritdoc />
    public async Task<EnqueueJobResult> EnqueueAsync(
        EnqueueJobRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        DateTimeOffset now = _clock.UtcNow;

        var def = new JobDefinition(request.Payload)
        {
            Priority = request.Priority,
            RetryPolicy = request.RetryPolicy ?? RetryPolicy.Default,
            AttemptTimeout = request.AttemptTimeout,
            DisplayName = request.DisplayName,
        };

        Job job = request.RunAtUtc is not null
            ? Job.CreateScheduled(def, request.RunAtUtc.Value, now)
            : Job.CreateQueued(def, now);

        Dictionary<string, string> tags = request.Tags is null
            ? []
            : new Dictionary<string, string>(request.Tags);

        job = job with
        {
            State = job.State with
            {
                CorrelationId = string.IsNullOrWhiteSpace(request.CorrelationId) ? null : request.CorrelationId,
                Tags = tags,
            },
        };

        await _store.CreateAsync(job, cancellationToken).ConfigureAwait(false);

        _events.Emit(job.Id, "job.enqueued", now, job.State.Status.ToString());

        return new EnqueueJobResult(job.Id);
    }

    /// <inheritdoc />
    public Task<bool> CancelAsync(JobId jobId, CancellationToken cancellationToken = default)
    {
        return _store.CancelAsync(jobId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> TryProcessOneAsync(CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = _clock.UtcNow;

        DequeueJobResult dequeue = await _store.DequeueAsync(
                new DequeueJobRequest
                {
                    WorkerId = _options.WorkerId,
                    LeaseDuration = _options.DefaultLeaseDuration,
                    Statuses = [JobStatus.Queued],
                },
                cancellationToken)
            .ConfigureAwait(false);

        if (!dequeue.Found || dequeue.Job is null || dequeue.Lease is null)
        {
            return false;
        }

        Job job = dequeue.Job;
        JobLease lease = dequeue.Lease;

        _events.Emit(job.Id, "job.dequeued", now, $"worker={_options.WorkerId}");

        string payloadType = job.Definition.Payload.Type;
        if (!_handlers.TryResolve(payloadType, out IJobHandler handler))
        {
            await HandleFailureAsync(
                    job,
                    lease,
                    now,
                    "HandlerNotFound",
                    $"No handler registered for type '{payloadType}'.",
                    cancellationToken)
                .ConfigureAwait(false);

            return true;
        }

        Job runningJob = StartAttempt(job, now);

        await _store.UpdateAsync(runningJob, cancellationToken).ConfigureAwait(false);
        _events.Emit(job.Id, "job.running", now, $"attempt={runningJob.State.AttemptCount}");

        JobExecutionResult result;
        try
        {
            result = await ExecuteWithLeaseRenewalAsync(runningJob, lease, handler, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await HandleFailureAsync(
                    runningJob,
                    lease,
                    _clock.UtcNow,
                    "Cancelled",
                    "Processing cancelled by caller.",
                    cancellationToken)
                .ConfigureAwait(false);

            return true;
        }
        catch (Exception ex)
        {
            await HandleFailureAsync(
                    runningJob,
                    lease,
                    _clock.UtcNow,
                    "UnhandledException",
                    ex.ToString(),
                    cancellationToken)
                .ConfigureAwait(false);

            return true;
        }

        if (result.Succeeded)
        {
            await HandleSuccessAsync(runningJob, lease, _clock.UtcNow, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await HandleFailureAsync(
                    runningJob,
                    lease,
                    _clock.UtcNow,
                    result.Error ?? "HandlerFailure",
                    result.ErrorDetails,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        return true;
    }

    private static Job StartAttempt(Job job, DateTimeOffset nowUtc)
    {
        int attemptNumber = job.State.AttemptCount + 1;
        var attempt = JobAttempt.Start(attemptNumber, nowUtc);

        var attempts = job.State.Attempts.ToList();
        attempts.Add(attempt);

        JobState nextState = job.State with
        {
            Attempts = attempts,
            Status = JobStatus.Running,
            UpdatedAtUtc = nowUtc,
        };

        job.EnsureCanTransitionTo(JobStatus.Running);

        return job with
        {
            State = nextState,
        };
    }

    private static Job MarkAttemptSucceeded(Job job, DateTimeOffset nowUtc)
    {
        var attempts = job.State.Attempts.ToList();
        if (attempts.Count == 0)
        {
            return job;
        }

        JobAttempt last = attempts[^1];
        attempts[^1] = last.Succeed(nowUtc);

        return job with
        {
            State = job.State with
            {
                Attempts = attempts,
                UpdatedAtUtc = nowUtc,
            },
        };
    }

    private static Job MarkAttemptFailed(Job job, DateTimeOffset nowUtc, string error, string details)
    {
        var attempts = job.State.Attempts.ToList();
        if (attempts.Count == 0)
        {
            return job;
        }

        JobAttempt last = attempts[^1];
        attempts[^1] = last.Fail(nowUtc, error, details);

        return job with
        {
            State = job.State with
            {
                Attempts = attempts,
                UpdatedAtUtc = nowUtc,
                Status = JobStatus.Failed,
            },
        };
    }

    private static CancellationTokenSource CreateAttemptCancellation(Job job, CancellationToken outerToken)
    {
        TimeSpan? attemptTimeout = job.Definition.AttemptTimeout;
        if (attemptTimeout is null || attemptTimeout.Value <= TimeSpan.Zero)
        {
            return CancellationTokenSource.CreateLinkedTokenSource(outerToken);
        }

        var cts = CancellationTokenSource.CreateLinkedTokenSource(outerToken);
        cts.CancelAfter(attemptTimeout.Value);
        return cts;
    }

    private async Task<JobExecutionResult> ExecuteWithLeaseRenewalAsync(
        Job job,
        JobLease lease,
        IJobHandler handler,
        CancellationToken cancellationToken)
    {
        using CancellationTokenSource cts = CreateAttemptCancellation(job, cancellationToken);
        CancellationToken attemptToken = cts.Token;

        Task<JobExecutionResult> executionTask = handler.HandleAsync(job, attemptToken);

        if (_options.LeaseRenewInterval <= TimeSpan.Zero)
        {
            return await executionTask.ConfigureAwait(false);
        }

        while (!executionTask.IsCompleted)
        {
            var delayTask = Task.Delay(_options.LeaseRenewInterval, cancellationToken);
            Task completed = await Task.WhenAny(executionTask, delayTask).ConfigureAwait(false);

            if (completed == executionTask)
            {
                break;
            }

            bool ok = await _store.RenewLeaseAsync(
                    job.Id,
                    lease.LeaseToken,
                    _options.DefaultLeaseDuration,
                    cancellationToken)
                .ConfigureAwait(false);

            if (!ok)
            {
                throw new JobLeaseLostException(job.Id, lease.LeaseToken);
            }

            _events.Emit(job.Id, "job.lease_renewed", _clock.UtcNow, $"worker={_options.WorkerId}");
        }

        return await executionTask.ConfigureAwait(false);
    }

    private async Task HandleSuccessAsync(
        Job job,
        JobLease lease,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken)
    {
        Job finalJob = MarkAttemptSucceeded(job, nowUtc).TransitionTo(JobStatus.Succeeded, nowUtc);

        bool ok = await _store.CompleteAsync(finalJob.Id, lease.LeaseToken, finalJob, cancellationToken)
            .ConfigureAwait(false);
        if (!ok)
        {
            throw new JobLeaseLostException(finalJob.Id, lease.LeaseToken);
        }

        _events.Emit(finalJob.Id, "job.succeeded", nowUtc, $"attempt={finalJob.State.AttemptCount}");
    }

    private async Task HandleFailureAsync(
        Job job,
        JobLease lease,
        DateTimeOffset nowUtc,
        string error,
        string details,
        CancellationToken cancellationToken)
    {
        Job failedJob = MarkAttemptFailed(job, nowUtc, error, details);

        int attempts = failedJob.State.AttemptCount;
        RetryPolicy policy = failedJob.Definition.RetryPolicy ?? RetryPolicy.Default;

        bool canRetry = attempts < policy.MaxAttempts;

        Job nextJob;
        if (canRetry)
        {
            int nextAttemptNumber = attempts + 1;
            TimeSpan delay = policy.ComputeDelay(nextAttemptNumber);

            if (delay > TimeSpan.Zero)
            {
                DateTimeOffset runAt = nowUtc.Add(delay);
                failedJob.EnsureCanTransitionTo(JobStatus.Failed);
                nextJob = failedJob with
                {
                    State = failedJob.State with
                    {
                        Status = JobStatus.Scheduled,
                        UpdatedAtUtc = nowUtc,
                        RunAtUtc = runAt,
                    },
                };

                _events.Emit(nextJob.Id, "job.retry_scheduled", nowUtc, $"runAtUtc={runAt:O}");
            }
            else
            {
                failedJob.EnsureCanTransitionTo(JobStatus.Failed);
                nextJob = failedJob with
                {
                    State = failedJob.State with
                    {
                        Status = JobStatus.Queued,
                        UpdatedAtUtc = nowUtc,
                        RunAtUtc = null,
                    },
                };

                _events.Emit(nextJob.Id, "job.retry_queued", nowUtc, "immediate");
            }
        }
        else
        {
            failedJob.EnsureCanTransitionTo(JobStatus.Failed);
            nextJob = failedJob with
            {
                State = failedJob.State with
                {
                    Status = JobStatus.DeadLettered,
                    UpdatedAtUtc = nowUtc,
                },
            };

            _events.Emit(nextJob.Id, "job.deadlettered", nowUtc, $"attempts={attempts}");
        }

        bool ok = await _store.FailAsync(nextJob.Id, lease.LeaseToken, nextJob, cancellationToken)
            .ConfigureAwait(false);
        if (!ok)
        {
            throw new JobLeaseLostException(nextJob.Id, lease.LeaseToken);
        }

        _events.Emit(nextJob.Id, "job.failed_attempt", nowUtc, $"attempt={attempts}");
    }
}