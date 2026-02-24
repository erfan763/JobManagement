using JobManagement.Abstractions.Handlers;
using JobManagement.Domain.Models;
using Microsoft.Extensions.Logging;

namespace SampleWorker.Cases;

public sealed class LongRunningHandler(ILogger<LongRunningHandler> logger) : IJobHandler
{
    private readonly ILogger<LongRunningHandler> _logger = logger;

    public string Type => "LongRunning";

    public async Task<JobExecutionResult> HandleAsync(Job job, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "⏳ LongRunning started. job={JobId} willWorkSeconds=18",
            job.Id.ToString());

        // long enough to renew lease multiple times (engine renew every 5s in Program.cs)
        for (int i = 1; i <= 6; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            _logger.LogInformation("⏳ LongRunning heartbeat {I}/6 job={JobId}", i, job.Id.ToString());
        }

        _logger.LogInformation("✅ LongRunning finished. job={JobId}", job.Id.ToString());
        return JobExecutionResult.Success();
    }
}