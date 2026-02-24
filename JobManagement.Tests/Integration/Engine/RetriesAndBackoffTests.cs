using FluentAssertions;
using JobManagement.Abstractions.Execution;
using JobManagement.Abstractions.Handlers;
using JobManagement.Abstractions.Persistence;
using JobManagement.Abstractions.Telemetry;
using JobManagement.DependencyInjection;
using JobManagement.Domain.Enums;
using JobManagement.Domain.Models;
using JobManagement.Domain.Requests;
using JobManagement.Domain.Results;
using JobManagement.Domain.ValueObjects;
using JobManagement.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JobManagement.Tests.Integration.Engine;

public sealed class RetriesAndBackoffTests
{
    [Fact]
    public async Task FailTwiceThenSucceedShouldRetryAndThenSucceed()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IJobEventSink, NullJobEventSink>();

        services
            .AddJobManagement(o =>
            {
                o.WorkerId = "t1";
                o.DefaultLeaseDuration = TimeSpan.FromSeconds(10);
                o.LeaseRenewInterval = TimeSpan.FromSeconds(1);
            })
            .AddInMemoryJobStore();

        services.AddSingleton<IJobHandler, FailTwiceThenSucceedHandler>();

        ServiceProvider sp = services.BuildServiceProvider();
        IJobEngine engine = sp.GetRequiredService<IJobEngine>();
        IJobStore store = sp.GetRequiredService<IJobStore>();

        EnqueueJobResult enqueue = await engine.EnqueueAsync(
            new EnqueueJobRequest(new JobPayload("RetryThenOk", "{}"))
            {
                RetryPolicy = new RetryPolicy(
                    5,
                    TimeSpan.FromMilliseconds(10),
                    TimeSpan.FromMilliseconds(50),
                    false),
            });

        // Run enough cycles for retries to happen (jobs become scheduled, we wait)
        for (int i = 0; i < 20; i++)
        {
            await engine.TryProcessOneAsync();
            await Task.Delay(20);
        }

        Job job = await store.GetAsync(enqueue.JobId);
        job.Should().NotBeNull();
        job!.State.AttemptCount.Should().BeGreaterThanOrEqualTo(3);
        job.ShouldBeStatus(JobStatus.Succeeded);
    }

    [Fact]
    public async Task AlwaysFailShouldDeadletterWhenAttemptsExhausted()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IJobEventSink, NullJobEventSink>();

        services
            .AddJobManagement(o =>
            {
                o.WorkerId = "t1";
                o.DefaultLeaseDuration = TimeSpan.FromSeconds(10);
                o.LeaseRenewInterval = TimeSpan.FromSeconds(1);
            })
            .AddInMemoryJobStore();

        services.AddSingleton<IJobHandler, AlwaysFailHandler>();

        ServiceProvider sp = services.BuildServiceProvider();
        IJobEngine engine = sp.GetRequiredService<IJobEngine>();
        IJobStore store = sp.GetRequiredService<IJobStore>();

        EnqueueJobResult enqueue = await engine.EnqueueAsync(
            new EnqueueJobRequest(new JobPayload("AlwaysFail", "{}"))
            {
                RetryPolicy = new RetryPolicy(
                    2,
                    TimeSpan.FromMilliseconds(10),
                    TimeSpan.FromMilliseconds(20),
                    false),
            });

        for (int i = 0; i < 20; i++)
        {
            await engine.TryProcessOneAsync();
            await Task.Delay(20);
        }

        Job job = await store.GetAsync(enqueue.JobId);
        job.ShouldBeStatus(JobStatus.DeadLettered);
        job!.State.AttemptCount.Should().Be(2);
    }
}