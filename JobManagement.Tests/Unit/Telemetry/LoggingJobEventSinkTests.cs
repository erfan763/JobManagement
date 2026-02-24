using FluentAssertions;
using JobManagement.Abstractions.Telemetry;
using JobManagement.Domain.Identifiers;
using JobManagement.Tests.Infrastructure;
using Xunit;

namespace JobManagement.Tests.Unit.Telemetry;

public sealed class LoggingJobEventSinkTests
{
    [Fact]
    public void EmitShouldWriteMessage()
    {
        var logger = new TestLogger<LoggingJobEventSink>();
        var sink = new LoggingJobEventSink(logger);

        var id = JobId.New();
        sink.Emit(id, "job.test", DateTimeOffset.UtcNow, "hello");

        logger.Messages.Should().Contain(m => m.Contains("job.test") && m.Contains(id.ToString()));
    }
}