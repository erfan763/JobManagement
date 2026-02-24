using JobManagement.Abstractions.Handlers;
using JobManagement.Domain.Models;
using Microsoft.Extensions.Logging;

namespace SampleWorker.Cases;

public sealed class AlwaysFailHandler(ILogger<AlwaysFailHandler> logger) : IJobHandler
{
    private readonly ILogger<AlwaysFailHandler> _logger = logger;

    public string Type => "AlwaysFail";

    public Task<JobExecutionResult> HandleAsync(Job job, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "❌ AlwaysFail invoked. job={JobId} attempt={Attempt}",
            job.Id.ToString(),
            job.State.AttemptCount);

        return Task.FromResult(JobExecutionResult.Fail("AlwaysFail", "Always failing by design"));
    }
}