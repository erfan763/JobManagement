using JobManagement.Domain.Enums;

namespace JobManagement.Domain.Requests;

/// <summary>
///     Represents a storage-agnostic query model for searching jobs.
///     Your persistence implementation can map this to SQL/Mongo/etc.
/// </summary>
public sealed record JobQuery
{
    /// <summary>
    ///     Gets Optional status filter.
    /// </summary>
    public IReadOnlyCollection<JobStatus> Statuses { get; init; }

    /// <summary>
    ///     Gets Optional payload type filter.
    /// </summary>
    public IReadOnlyCollection<string> Types { get; init; }

    /// <summary>
    ///     Gets Optional correlation id filter.
    /// </summary>
    public string CorrelationId { get; init; }

    /// <summary>
    ///     Gets Optional tag filters (must match all provided key/value pairs).
    /// </summary>
    public IReadOnlyDictionary<string, string> Tags { get; init; }

    /// <summary>
    ///     Gets Optional created-from filter (UTC).
    /// </summary>
    public DateTimeOffset? CreatedFromUtc { get; init; }

    /// <summary>
    ///     Gets Optional created-to filter (UTC).
    /// </summary>
    public DateTimeOffset? CreatedToUtc { get; init; }

    /// <summary>
    ///     Gets Paging size (default: 50).
    /// </summary>
    public int PageSize { get; init; } = 50;

    /// <summary>
    ///     Gets Paging continuation token (storage-defined).
    /// </summary>
    public string ContinuationToken { get; init; }
}