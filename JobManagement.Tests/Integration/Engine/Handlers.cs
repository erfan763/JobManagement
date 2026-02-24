using JobManagement.Abstractions.Handlers;
using JobManagement.Domain.Models;

namespace JobManagement.Tests.Integration.Engine;

internal sealed class SuccessHandler : IJobHandler
{
    public string Type => "Success";

    public Task<JobExecutionResult> HandleAsync(Job job, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(JobExecutionResult.Success());
    }
}

internal sealed class AlwaysFailHandler : IJobHandler
{
    public string Type => "AlwaysFail";

    public Task<JobExecutionResult> HandleAsync(Job job, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(JobExecutionResult.Fail("AlwaysFail", "fail"));
    }
}

internal sealed class FailTwiceThenSucceedHandler : IJobHandler
{
    public string Type => "RetryThenOk";

    public Task<JobExecutionResult> HandleAsync(Job job, CancellationToken cancellationToken = default)
    {
        int attempt = job.State.AttemptCount;
        return Task.FromResult(
            attempt <= 2 ? JobExecutionResult.Fail("Planned", $"attempt={attempt}") : JobExecutionResult.Success());
    }
}

internal sealed class LongRunningHandler : IJobHandler
{
    public string Type => "LongRunning";

    public async Task<JobExecutionResult> HandleAsync(Job job, CancellationToken cancellationToken = default)
    {
        for (int i = 0; i < 6; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(TimeSpan.FromMilliseconds(150), cancellationToken);
        }

        return JobExecutionResult.Success();
    }
}