namespace JobManagement.Abstractions.Execution;

/// <summary>
///     Engine runtime options.
/// </summary>
public sealed record JobEngineOptions
{
    /// <summary>
    ///     Gets Worker id used for leasing and tracing.
    /// </summary>
    public string WorkerId { get; set; } = Environment.MachineName;

    /// <summary>
    ///     Gets Default lease duration for dequeued jobs.
    /// </summary>
    public TimeSpan DefaultLeaseDuration { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    ///     Gets How often the engine should renew leases while executing (if supported by the engine impl).
    /// </summary>
    public TimeSpan LeaseRenewInterval { get; set; } = TimeSpan.FromSeconds(20);

    /// <summary>
    ///     Gets Default idle delay when no jobs are found (polling engine scenario).
    /// </summary>
    public TimeSpan IdleDelay { get; set; } = TimeSpan.FromSeconds(1);
}