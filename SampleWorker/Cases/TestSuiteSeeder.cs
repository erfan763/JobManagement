using JobManagement.Abstractions.Execution;
using JobManagement.Domain.Enums;
using JobManagement.Domain.Requests;
using JobManagement.Domain.ValueObjects;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SampleWorker.Cases;

public sealed class TestSuiteSeeder(IJobEngine engine, ILogger<TestSuiteSeeder> logger) : IHostedService
{
    private readonly IJobEngine _engine = engine;
    private readonly ILogger<TestSuiteSeeder> _logger = logger;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== Enqueuing JobManagement full test suite ===");

        await _engine.EnqueueAsync(
            new EnqueueJobRequest(new JobPayload("Success", """{"case":"success"}""", "application/json"))
            {
                Priority = JobPriority.Normal,
                CorrelationId = "suite-001",
            },
            cancellationToken);

        await _engine.EnqueueAsync(
            new EnqueueJobRequest(new JobPayload("Success", """{"case":"scheduled-success"}""", "application/json"))
            {
                RunAtUtc = DateTimeOffset.UtcNow.AddSeconds(10),
                Priority = JobPriority.Low,
                CorrelationId = "suite-002",
            },
            cancellationToken);

        await _engine.EnqueueAsync(
            new EnqueueJobRequest(
                new JobPayload("FailTwiceThenSucceed", """{"case":"retry-then-success"}""", "application/json"))
            {
                RetryPolicy = new RetryPolicy(
                    5,
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(6)),
                Priority = JobPriority.High,
                CorrelationId = "suite-003",
            },
            cancellationToken);

        await _engine.EnqueueAsync(
            new EnqueueJobRequest(new JobPayload("AlwaysFail", """{"case":"dead-letter"}""", "application/json"))
            {
                RetryPolicy = new RetryPolicy(
                    3,
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(3),
                    false),
                Priority = JobPriority.High,
                CorrelationId = "suite-004",
            },
            cancellationToken);

        await _engine.EnqueueAsync(
            new EnqueueJobRequest(new JobPayload("NoSuchHandler", """{"case":"missing-handler"}""", "application/json"))
            {
                RetryPolicy = new RetryPolicy(
                    2,
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    false),
                Priority = JobPriority.Critical,
                CorrelationId = "suite-005",
            },
            cancellationToken);

        await _engine.EnqueueAsync(
            new EnqueueJobRequest(new JobPayload("LongRunning", """{"case":"lease-renew"}""", "application/json"))
            {
                AttemptTimeout = TimeSpan.FromSeconds(60),
                RetryPolicy = RetryPolicy.NoRetry,
                Priority = JobPriority.Normal,
                CorrelationId = "suite-006",
            },
            cancellationToken);

        _logger.LogInformation("=== Test suite enqueued. Watch logs for processing ===");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}