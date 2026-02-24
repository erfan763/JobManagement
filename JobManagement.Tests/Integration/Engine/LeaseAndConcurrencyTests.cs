using FluentAssertions;
using JobManagement.Abstractions.Clock;
using JobManagement.Abstractions.Execution;
using JobManagement.Abstractions.Handlers;
using JobManagement.Abstractions.Persistence;
using JobManagement.Abstractions.Telemetry;
using JobManagement.DependencyInjection;
using JobManagement.Domain.Models;
using JobManagement.Domain.Requests;
using JobManagement.Domain.Results;
using JobManagement.Domain.ValueObjects;
using JobManagement.Execution;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JobManagement.Tests.Integration.Engine;

public sealed class LeaseAndConcurrencyTests
{
    [Fact]
    public async Task TwoEnginesShouldNotProcessSameJobTwice()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IJobEventSink, NullJobEventSink>();
        services
            .AddJobManagement(o =>
            {
                o.WorkerId = "worker-A";
                o.DefaultLeaseDuration = TimeSpan.FromSeconds(2);
                o.LeaseRenewInterval = TimeSpan.FromSeconds(0);
            })
            .AddInMemoryJobStore();

        services.AddSingleton<IJobHandler, SuccessHandler>();
        ServiceProvider sp = services.BuildServiceProvider();
        IJobStore store = sp.GetRequiredService<IJobStore>();
        IJobEngine engineA = sp.GetRequiredService<IJobEngine>();
        var engineB = new JobEngine(
            store,
            sp.GetRequiredService<IJobHandlerRegistry>(),
            sp.GetRequiredService<IClock>(),
            sp.GetRequiredService<IJobEventSink>(),
            new JobEngineOptions
            {
                WorkerId = "worker-B",
            });

        EnqueueJobResult enqueue = await engineA.EnqueueAsync(new EnqueueJobRequest(new JobPayload("Success", "{}")));

        Task<bool> t1 = engineA.TryProcessOneAsync();
        Task<bool> t2 = engineB.TryProcessOneAsync();
        bool[] results = await Task.WhenAll(t1, t2);
        results.Count(r => r).Should().Be(1);
        Job job = await store.GetAsync(enqueue.JobId);
        job!.State.AttemptCount.Should().Be(1);
    }
}