using JobManagement.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SampleWorker.Cases;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(o =>
        {
            o.SingleLine = true;
            o.TimestampFormat = "HH:mm:ss.fff ";
            o.IncludeScopes = true;
        });

        logging.SetMinimumLevel(LogLevel.Information);
        logging.AddFilter("JobManagement", LogLevel.Information);
        logging.AddFilter("Microsoft", LogLevel.Warning);
    })
    .ConfigureServices(services =>
    {
        services
            .AddJobManagement(o =>
            {
                o.WorkerId = "sample-worker-1";
                o.DefaultLeaseDuration = TimeSpan.FromSeconds(20);
                o.LeaseRenewInterval = TimeSpan.FromSeconds(5);
                o.IdleDelay = TimeSpan.FromMilliseconds(200);
            })
            .AddJobManagementLogging()
            .AddInMemoryJobStore()
            .AddJobWorker(o =>
            {
                o.IdleDelay = TimeSpan.FromMilliseconds(200);
                o.ErrorDelay = TimeSpan.FromSeconds(1);
            });

        // Register handlers
        services.AddJobHandler<SuccessJobHandler>();
        services.AddJobHandler<FailTwiceThenSucceedHandler>();
        services.AddJobHandler<AlwaysFailHandler>();
        services.AddJobHandler<LongRunningHandler>();

        // Enqueue test suite on startup
        services.AddHostedService<TestSuiteSeeder>();
    })
    .Build();

await host.RunAsync();