using JobManagement.Domain.Identifiers;

namespace JobManagement.Abstractions.Telemetry;

/// <summary>
///     No-op telemetry sink.
/// </summary>
public sealed class NullJobEventSink : IJobEventSink
{
    /// <inheritdoc />
    public void Emit(JobId jobId, string eventName, DateTimeOffset utcTimestamp, string details = null)
    {
        // intentionally no-op
    }
}