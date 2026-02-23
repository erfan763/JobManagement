using JobManagement.Abstractions.Clock;
using JobManagement.Abstractions.Persistence;
using JobManagement.Domain.Enums;
using JobManagement.Domain.Identifiers;
using JobManagement.Domain.Models;
using JobManagement.Domain.Requests;
using JobManagement.Domain.Results;
using System.Collections.Concurrent;

namespace JobManagement.Stores.InMemory;

/// <summary>
///     In-memory implementation of <see cref="IJobStore" />.
///     Intended for demos, tests, and local development.
///     Not suitable for distributed or multiprocess production usage.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="InMemoryJobStore" /> class.
/// </remarks>
/// <param name="clock">Clock used for scheduling and lease expiration.</param>
/// <param name="options">Optional configuration options.</param>
public sealed class InMemoryJobStore(
    IClock clock,
    long cleanupTicks,
    InMemoryJobStoreOptions options = null) : IJobStore
{
    private readonly IClock _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    private readonly ConcurrentDictionary<JobId, Job> _jobs = new();
    private readonly ConcurrentDictionary<JobId, LeaseEntry> _leases = new();

    public InMemoryJobStoreOptions Options { get; } = options ?? new InMemoryJobStoreOptions();

    public long CleanupTicks { get; } = cleanupTicks;

    public Task<Job> GetAsync(JobId jobId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _jobs.TryGetValue(jobId, out Job job);
        return Task.FromResult(job);
    }

    /// <inheritdoc />
    public Task CreateAsync(Job job, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_jobs.TryAdd(job.Id, job))
        {
            throw new InvalidOperationException($"Job with id '{job.Id}' already exists.");
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UpdateAsync(Job job, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_jobs.ContainsKey(job.Id))
        {
            throw new InvalidOperationException($"Cannot update job '{job.Id}' because it does not exist.");
        }

        _jobs[job.Id] = job;

        return Task.CompletedTask;
    }

    public Task<PagedResult<Job>> QueryAsync(
        JobQuery query,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IEnumerable<Job> q = _jobs.Values;

        if (query.Statuses?.Count > 0)
        {
            q = q.Where(j => query.Statuses.Contains(j.State.Status));
        }

        if (query.Types?.Count > 0)
        {
            q = q.Where(j => query.Types.Contains(j.Definition.Payload.Type, StringComparer.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.CorrelationId))
        {
            q = q.Where(j => j.State.CorrelationId == query.CorrelationId);
        }

        if (query.CreatedFromUtc is not null)
        {
            q = q.Where(j => j.State.CreatedAtUtc >= query.CreatedFromUtc.Value);
        }

        if (query.CreatedToUtc is not null)
        {
            q = q.Where(j => j.State.CreatedAtUtc <= query.CreatedToUtc.Value);
        }

        var items = q
            .OrderByDescending(j => j.State.CreatedAtUtc)
            .Take(Math.Max(1, query.PageSize))
            .ToList();

        return Task.FromResult(
            new PagedResult<Job>
            {
                Items = items,
                ContinuationToken = null,
            });
    }

    public Task<DequeueJobResult> DequeueAsync(
        DequeueJobRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CleanupExpiredLeases();

        DateTimeOffset now = _clock.UtcNow;

        var candidates = _jobs.Values
            .Where(j => IsEligible(j, now))
            .OrderByDescending(j => j.Definition.Priority)
            .ThenBy(j => j.State.RunAtUtc ?? j.State.CreatedAtUtc)
            .Take(Math.Max(1, request.ScanLimit))
            .ToList();

        foreach (Job job in candidates)
        {
            if (IsLeased(job.Id, now))
            {
                continue;
            }

            string leaseToken = Guid.NewGuid().ToString("N");
            DateTimeOffset expires = now.Add(request.LeaseDuration);

            if (!_leases.TryAdd(job.Id, new LeaseEntry(leaseToken, request.WorkerId, expires)))
            {
                continue;
            }

            return Task.FromResult(
                DequeueJobResult.Success(
                    job,
                    new JobLease
                    {
                        JobId = job.Id,
                        LeaseToken = leaseToken,
                        WorkerId = request.WorkerId,
                        ExpiresAtUtc = expires,
                    }));
        }

        return Task.FromResult(DequeueJobResult.Empty());
    }

    public Task<bool> RenewLeaseAsync(
        JobId jobId,
        string leaseToken,
        TimeSpan additionalTime,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<bool> CompleteAsync(
        JobId jobId,
        string leaseToken,
        Job jobSnapshot,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!ValidateLease(jobId, leaseToken))
        {
            return Task.FromResult(false);
        }

        _jobs[jobId] = jobSnapshot;

        // Always release lease after successful completion
        _leases.TryRemove(jobId, out _);

        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<bool> FailAsync(
        JobId jobId,
        string leaseToken,
        Job jobSnapshot,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!ValidateLease(jobId, leaseToken))
        {
            return Task.FromResult(false);
        }

        // Ensure job is in Failed state (defensive consistency)
        if (jobSnapshot.State.Status != JobStatus.Failed)
        {
            throw new InvalidOperationException($"FailAsync requires job state to be '{JobStatus.Failed}'.");
        }

        _jobs[jobId] = jobSnapshot;

        // Release lease so job can be retried if engine re-queues it
        _leases.TryRemove(jobId, out _);

        return Task.FromResult(true);
    }

    public Task<bool> CancelAsync(JobId jobId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_jobs.TryGetValue(jobId, out Job job))
        {
            return Task.FromResult(false);
        }

        Job cancelled = job with
        {
            State = job.State with
            {
                Status = JobStatus.Cancelled,
                UpdatedAtUtc = _clock.UtcNow,
            },
        };

        _jobs[jobId] = cancelled;
        _leases.TryRemove(jobId, out _);

        return Task.FromResult(true);
    }

    private static bool IsEligible(Job job, DateTimeOffset now)
    {
        return job.State.Status == JobStatus.Queued ||
               (job.State.Status == JobStatus.Scheduled &&
                job.State.RunAtUtc is not null &&
                job.State.RunAtUtc <= now);
    }

    private bool IsLeased(JobId jobId, DateTimeOffset now)
    {
        if (!_leases.TryGetValue(jobId, out LeaseEntry lease))
        {
            return false;
        }

        if (now < lease.ExpiresAtUtc) return true;
        _leases.TryRemove(jobId, out _);
        return false;
    }

    private bool ValidateLease(JobId jobId, string token)
    {
        if (!_leases.TryGetValue(jobId, out LeaseEntry lease))
        {
            return false;
        }

        if (!string.Equals(lease.Token, token, StringComparison.Ordinal))
        {
            return false;
        }

        return _clock.UtcNow < lease.ExpiresAtUtc;
    }

    private void CleanupExpiredLeases()
    {
        DateTimeOffset now = _clock.UtcNow;

        foreach (KeyValuePair<JobId, LeaseEntry> kv in _leases)
        {
            if (now >= kv.Value.ExpiresAtUtc)
            {
                _leases.TryRemove(kv.Key, out _);
            }
        }
    }

    private sealed record LeaseEntry(
        string Token,
        string WorkerId,
        DateTimeOffset ExpiresAtUtc);
}