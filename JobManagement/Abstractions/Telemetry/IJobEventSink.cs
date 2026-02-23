using JobManagement.Domain.Identifiers;

namespace JobManagement.Abstractions.Telemetry;

/// <summary>
///     Lightweight event sink for observability without forcing any telemetry dependency.
///     Implementations may forward events to logs, OpenTelemetry, Application Insights, etc.
/// </summary>
public interface IJobEventSink
{
    /// <summary>
    ///     Called when a job lifecycle event occurs.
    /// </summary>
    /// <param name="jobId">Job id.</param>
    /// <param name="eventName">Event name (e.g., "job.dequeued", "job.succeeded").</param>
    /// <param name="utcTimestamp">Event timestamp in UTC.</param>
    /// <param name="details">Optional details.</param>
    void Emit(JobId jobId, string eventName, DateTimeOffset utcTimestamp, string details = null);
}