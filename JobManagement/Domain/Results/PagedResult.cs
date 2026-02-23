namespace JobManagement.Domain.Results;

/// <summary>
///     Represents a storage-agnostic paged result.
/// </summary>
/// <typeparam name="T">Item type.</typeparam>
public sealed record PagedResult<T>
{
    /// <summary>
    ///     Gets Returned items.
    /// </summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>
    ///     Gets Continuation token (storage-defined). Null means no more pages.
    /// </summary>
    public string ContinuationToken { get; init; }
}