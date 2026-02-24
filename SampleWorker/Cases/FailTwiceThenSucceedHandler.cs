using JobManagement.Abstractions.Handlers;
using JobManagement.Domain.Models;
using Microsoft.Extensions.Logging;

namespace SampleWorker.Cases;

public sealed class FailTwiceThenSucceedHandler(ILogger<FailTwiceThenSucceedHandler> logger) : IJobHandler
{
    private readonly ILogger<FailTwiceThenSucceedHandler> _logger = logger;

    /// <inheritdoc />
    public string Type => "FailTwiceThenSucceed";

    public Task<JobExecutionResult> HandleAsync(Job job, CancellationToken cancellationToken = default)
    {
        int attempt = job.State.AttemptCount;
        _logger.LogInformation(
            "🔁 Retry handler invoked. job={JobId} attempt={Attempt} body={Body}",
            job.Id.ToString(),
            attempt,
            job.Definition.Payload.Body);

        if (attempt <= 2)
        {
            return Task.FromResult(JobExecutionResult.Fail("PlannedFailure", $"Failing attempt {attempt} on purpose"));
        }

        _logger.LogInformation("✅ Now succeeding on attempt {Attempt}. job={JobId}", attempt, job.Id.ToString());
        return Task.FromResult(JobExecutionResult.Success());
    }
}