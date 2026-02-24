using FluentAssertions;
using JobManagement.Domain.Enums;
using JobManagement.Domain.Models;
using JobManagement.Domain.ValueObjects;
using Xunit;

namespace JobManagement.Tests.Unit.Abstractions;

public sealed class JobTests
{
    [Fact]
    public void CreateQueuedShouldStartInQueued()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var def = new JobDefinition(new JobPayload("X", "{}"));
        var job = Job.CreateQueued(def, now);

        job.State.Status.Should().Be(JobStatus.Queued);
        job.State.CreatedAtUtc.Should().Be(now);
    }

    [Fact]
    public void CreateScheduledShouldStartInScheduledWithRunAtUtc()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset runAt = now.AddMinutes(10);
        var def = new JobDefinition(new JobPayload("X", "{}"));
        var job = Job.CreateScheduled(def, runAt, now);

        job.State.Status.Should().Be(JobStatus.Scheduled);
        job.State.RunAtUtc.Should().Be(runAt);
    }
}