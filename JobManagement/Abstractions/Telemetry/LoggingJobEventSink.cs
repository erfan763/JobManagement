using JobManagement.Domain.Identifiers;
using Microsoft.Extensions.Logging;

namespace JobManagement.Abstractions.Telemetry;

/// <summary>
///     Emits JobManagement events into <see cref="ILogger" /> so applications can observe engine activity.
/// </summary>
/// <remarks>
///     Creates a new <see cref="LoggingJobEventSink" />.
/// </remarks>
public sealed class LoggingJobEventSink(ILogger<LoggingJobEventSink> logger) : IJobEventSink
{
    private readonly ILogger<LoggingJobEventSink> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public void Emit(JobId jobId, string eventName, DateTimeOffset utcTimestamp, string details = null)
    {
        LogLevel level;

        if (eventName.Contains("deadlettered", StringComparison.OrdinalIgnoreCase))
        {
            level = LogLevel.Error;
        }
        else if (eventName.Contains("failed", StringComparison.OrdinalIgnoreCase))
        {
            level = LogLevel.Warning;
        }
        else
        {
            level = LogLevel.Information;
        }

        _logger.Log(
            level,
            "[JobManagement] {Event} job={JobId} at={Utc:O} {Details}",
            eventName,
            jobId.ToString(),
            utcTimestamp,
            string.IsNullOrWhiteSpace(details) ? string.Empty : $"| {details}");
    }
}