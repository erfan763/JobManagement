using JobManagement.Abstractions.Clock;
using JobManagement.Abstractions.Execution;
using JobManagement.Abstractions.Handlers;
using JobManagement.Abstractions.Persistence;
using JobManagement.Abstractions.Telemetry;
using JobManagement.Execution;
using JobManagement.Hosting;
using JobManagement.Stores.InMemory;
using Microsoft.Extensions.DependencyInjection;

namespace JobManagement.DependencyInjection;

/// <summary>
///     Dependency injection extensions for JobManagement.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers JobManagement core services (engine, clock, telemetry, handler registry).
    ///     You must also register an <see cref="IJobStore" /> (e.g. via <see cref="AddInMemoryJobStore" />).
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Optional engine options configuration.</param>
    public static IServiceCollection AddJobManagement(
        this IServiceCollection services,
        Action<JobEngineOptions> configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new JobEngineOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);

        services.AddSingleton<IClock, SystemClock>();

        // Default telemetry is no-op; user can override by registering IJobEventSink before/after.
        services.TryAddSingleton<IJobEventSink, NullJobEventSink>();

        // Handler registry is built from registered IJobHandler instances.
        services.AddSingleton<IJobHandlerRegistry>(sp =>
        {
            IEnumerable<IJobHandler> handlers = sp.GetServices<IJobHandler>();
            return new InMemoryJobHandlerRegistry(handlers);
        });

        services.AddSingleton<IJobEngine, JobEngine>();

        return services;
    }

    /// <summary>
    ///     Registers the in-memory store as <see cref="IJobStore" />.
    ///     Intended for demos/tests; not production multi-node persistence.
    /// </summary>
    public static IServiceCollection AddInMemoryJobStore(
        this IServiceCollection services,
        Action<InMemoryJobStoreOptions> configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new InMemoryJobStoreOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<IJobStore, InMemoryJobStore>();

        return services;
    }

    /// <summary>
    ///     Registers a job handler implementation.
    /// </summary>
    public static IServiceCollection AddJobHandler<THandler>(this IServiceCollection services)
        where THandler : class, IJobHandler
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IJobHandler, THandler>();
        return services;
    }

    /// <summary>
    ///     Adds the background worker that continuously processes jobs.
    /// </summary>
    public static IServiceCollection AddJobWorker(
        this IServiceCollection services,
        Action<JobWorkerOptions> configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new JobWorkerOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddHostedService<JobWorker>();

        return services;
    }
}

internal static class ServiceCollectionTryAddExtensions
{
    internal static IServiceCollection TryAddSingleton<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        if (services.Any(d => d.ServiceType == typeof(TService)))
        {
            return services;
        }

        services.AddSingleton<TService, TImplementation>();
        return services;
    }
}