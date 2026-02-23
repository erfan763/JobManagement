namespace JobManagement.Stores.InMemory;

/// <summary>
///     Options for
///     <see>
///         <cref>InMemoryJobStore</cref>
///     </see>
///     .
/// </summary>
public sealed record InMemoryJobStoreOptions
{
    /// <summary>
    ///     Gets how often expired leases should be cleaned up (best-effort).
    /// </summary>
    public TimeSpan LeaseCleanupInterval { get; init; } = TimeSpan.FromSeconds(30);
}