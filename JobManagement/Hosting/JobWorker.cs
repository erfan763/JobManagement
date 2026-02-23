using JobManagement.Abstractions.Execution;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobManagement.Hosting;

/// <summary>
///     Background worker that continuously processes jobs using <see cref="IJobEngine" />.
/// </summary>
/// <remarks>
///     Creates a new <see cref="JobWorker" />.
/// </remarks>
public sealed class JobWorker(IJobEngine engine, JobWorkerOptions options, ILogger<JobWorker> logger)
    : BackgroundService
{
    private readonly IJobEngine _engine = engine ?? throw new ArgumentNullException(nameof(engine));
    private readonly ILogger<JobWorker> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly JobWorkerOptions _options = options ?? throw new ArgumentNullException(nameof(options));

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("JobWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                bool processed = await _engine.TryProcessOneAsync(stoppingToken).ConfigureAwait(false);

                if (!processed)
                {
                    await Task.Delay(_options.IdleDelay, stoppingToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // graceful stop
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JobWorker loop error.");
                await Task.Delay(_options.ErrorDelay, stoppingToken).ConfigureAwait(false);
            }
        }

        _logger.LogInformation("JobWorker stopped.");
    }
}