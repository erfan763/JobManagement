using JobManagement.Abstractions.Handlers;
using JobManagement.Domain.Models;
using Microsoft.Extensions.Logging;

namespace SampleWorker.Cases;

public sealed class SuccessJobHandler(ILogger<SuccessJobHandler> logger) : IJobHandler
{
    private readonly ILogger<SuccessJobHandler> _logger = logger;

    public string Type => "Success";

    public Task<JobExecutionResult> HandleAsync(Job job, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "✅ Success handler executed. job={JobId} body={Body}",
            job.Id.ToString(),
            job.Definition.Payload.Body);

        return Task.FromResult(JobExecutionResult.Success());
    }
}