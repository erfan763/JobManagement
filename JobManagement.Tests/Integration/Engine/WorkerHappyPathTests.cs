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

public sealed class WorkerHappyPathTests
{
    [Fact]
    public async Task EnqueueAndProcessSuccessJobShouldSucceed()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IJobEventSink, NullJobEventSink>();

        services
            .AddJobManagement(o =>
            {
                o.WorkerId = "t1";
                o.DefaultLeaseDuration = TimeSpan.FromSeconds(5);
                o.LeaseRenewInterval = TimeSpan.FromSeconds(1);
            })
            .AddInMemoryJobStore();

        services.AddSingleton<IJobHandler, SuccessHandler>();

        ServiceProvider sp = services.BuildServiceProvider();
        IJobEngine engine = sp.GetRequiredService<IJobEngine>();

        EnqueueJobResult res = await engine.EnqueueAsync(
            new EnqueueJobRequest(new JobPayload("Success", "{}"))
            {
                Priority = JobPriority.Normal,
            });

        bool processed = await engine.TryProcessOneAsync();
        processed.Should().BeTrue();

        IJobStore store = sp.GetRequiredService<IJobStore>();
        Job job = await store.GetAsync(res.JobId);

        job.ShouldBeStatus(JobStatus.Succeeded);
    }
}