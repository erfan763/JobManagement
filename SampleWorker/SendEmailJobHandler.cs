using JobManagement.Abstractions.Handlers;
using JobManagement.Domain.Models;
using Microsoft.Extensions.Logging;

namespace SampleWorker;

public sealed class SendEmailJobHandler(ILogger<SendEmailJobHandler> logger) : IJobHandler
{
    private readonly ILogger<SendEmailJobHandler> _logger = logger;

    public string Type => "SendEmail";

    public async Task<JobExecutionResult> HandleAsync(Job job, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handler started. job={JobId} payloadType={Type} body={Body}",
            job.Id.ToString(),
            job.Definition.Payload.Type,
            job.Definition.Payload.Body);

        await Task.Delay(500, cancellationToken); // simulate work

        _logger.LogInformation("Handler done. job={JobId}", job.Id.ToString());
        return JobExecutionResult.Success();
    }
}