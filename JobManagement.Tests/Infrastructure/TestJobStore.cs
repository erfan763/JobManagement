using JobManagement.Abstractions.Clock;
using JobManagement.Abstractions.Persistence;
using JobManagement.Domain.Enums;
using JobManagement.Domain.Identifiers;
using JobManagement.Domain.Models;
using JobManagement.Domain.Requests;
using JobManagement.Domain.Results;
using JobManagement.Stores.InMemory;

namespace JobManagement.Tests.Infrastructure;

/// <summary>
///     Test store that uses the real InMemoryJobStore but adds helpers.
/// </summary>
public sealed class TestJobStore(IClock clock) : IJobStore
{
    private readonly InMemoryJobStore _inner = new(clock);

    public Task CreateAsync(Job job, CancellationToken cancellationToken = default)
    {
        return _inner.CreateAsync(job, cancellationToken);
    }

    public Task<Job> GetAsync(JobId jobId, CancellationToken cancellationToken = default)
    {
        return _inner.GetAsync(jobId, cancellationToken);
    }

    public Task UpdateAsync(Job job, CancellationToken cancellationToken = default)
    {
        return _inner.UpdateAsync(job, cancellationToken);
    }

    public Task<PagedResult<Job>> QueryAsync(JobQuery query, CancellationToken cancellationToken = default)
    {
        return _inner.QueryAsync(query, cancellationToken);
    }

    public Task<DequeueJobResult> DequeueAsync(DequeueJobRequest request, CancellationToken cancellationToken = default)
    {
        return _inner.DequeueAsync(request, cancellationToken);
    }

    public Task<bool> RenewLeaseAsync(
        JobId jobId,
        string leaseToken,
        TimeSpan additionalTime,
        CancellationToken cancellationToken = default)
    {
        return _inner.RenewLeaseAsync(jobId, leaseToken, additionalTime, cancellationToken);
    }

    public Task<bool> CompleteAsync(
        JobId jobId,
        string leaseToken,
        Job jobSnapshot,
        CancellationToken cancellationToken = default)
    {
        return _inner.CompleteAsync(jobId, leaseToken, jobSnapshot, cancellationToken);
    }

    public Task<bool> FailAsync(
        JobId jobId,
        string leaseToken,
        Job jobSnapshot,
        CancellationToken cancellationToken = default)
    {
        return _inner.FailAsync(jobId, leaseToken, jobSnapshot, cancellationToken);
    }

    public Task<bool> CancelAsync(JobId jobId, CancellationToken cancellationToken = default)
    {
        return _inner.CancelAsync(jobId, cancellationToken);
    }

    public async Task<Job> GetSingleByStatusAsync(JobStatus status, CancellationToken ct = default)
    {
        PagedResult<Job> res = await QueryAsync(
            new JobQuery
            {
                Statuses = [status],
                PageSize = 50,
            },
            ct);

        return res.Items.FirstOrDefault(item => item != null);
    }

    public async Task<int> CountByStatusAsync(JobStatus status, CancellationToken ct = default)
    {
        PagedResult<Job> res = await QueryAsync(
            new JobQuery
            {
                Statuses = [status],
                PageSize = 500,
            },
            ct);
        return res.Items.Count;
    }
}