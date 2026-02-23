namespace JobManagement.Abstractions.Handlers;

/// <summary>
///     Resolves handlers by payload type.
/// </summary>
public interface IJobHandlerRegistry
{
    /// <summary>
    ///     Gets all registered handlers.
    /// </summary>
    IReadOnlyCollection<IJobHandler> Handlers { get; }

    /// <summary>
    ///     Tries to resolve a handler for a given payload type.
    /// </summary>
    /// <param name="type">Payload type string.</param>
    /// <param name="handler">Resolved handler.</param>
    /// <returns>Return Boolean That Resolved Or Not. </returns>
    bool TryResolve(string type, out IJobHandler handler);
}