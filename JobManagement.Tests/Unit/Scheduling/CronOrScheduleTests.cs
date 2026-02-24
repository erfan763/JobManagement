using FluentAssertions;
using JobManagement.Abstractions.Persistence;
using JobManagement.Domain.Enums;
using JobManagement.Domain.Models;
using JobManagement.Domain.ValueObjects;
using JobManagement.Tests.Infrastructure;
using Xunit;

namespace JobManagement.Tests.Unit.Scheduling;

public sealed class CronOrScheduleTests
{
    [Fact]
    public async Task ScheduledJobShouldNotBeDequeuedBeforeRunAtUtc()
    {
        var clock = new TestClock(new DateTimeOffset(2026, 02, 24, 0, 0, 0, TimeSpan.Zero));
        var store = new TestJobStore(clock);

        var def = new JobDefinition(new JobPayload("Success", "{}"));
        DateTimeOffset runAt = clock.UtcNow.AddMinutes(10);
        var job = Job.CreateScheduled(def, runAt, clock.UtcNow);

        await store.CreateAsync(job);

        DequeueJobResult res = await store.DequeueAsync(
            new DequeueJobRequest
            {
                WorkerId = "w1",
                LeaseDuration = TimeSpan.FromSeconds(30),
                ScanLimit = 100,
                Statuses = [JobStatus.Queued, JobStatus.Scheduled],
            });

        res.Found.Should().BeFalse();
    }

    [Fact]
    public async Task ScheduledJobShouldBeDequeuedAfterRunAtUtc()
    {
        var clock = new TestClock(new DateTimeOffset(2026, 02, 24, 0, 0, 0, TimeSpan.Zero));
        var store = new TestJobStore(clock);

        var def = new JobDefinition(new JobPayload("Success", "{}"));
        DateTimeOffset runAt = clock.UtcNow.AddSeconds(10);
        var job = Job.CreateScheduled(def, runAt, clock.UtcNow);

        await store.CreateAsync(job);

        clock.Advance(TimeSpan.FromSeconds(11));

        DequeueJobResult res = await store.DequeueAsync(
            new DequeueJobRequest
            {
                WorkerId = "w1",
                LeaseDuration = TimeSpan.FromSeconds(30),
                ScanLimit = 100,
                Statuses = [JobStatus.Queued, JobStatus.Scheduled],
            });

        res.Found.Should().BeTrue();
        res.Job!.State.Status.Should().Be(JobStatus.Queued);
    }
}