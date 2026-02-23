namespace JobManagement.Abstractions.Handlers;

/// <summary>
///     Simple in-memory handler registry.
///     You can replace this with a DI-based registry in your host application.
/// </summary>
public sealed class InMemoryJobHandlerRegistry : IJobHandlerRegistry
{
    /// <summary>
    ///     Job handler.
    /// </summary>
    private readonly Dictionary<string, IJobHandler> _handlers;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InMemoryJobHandlerRegistry" /> class.
    ///     Creates a registry from a set of handlers.
    /// </summary>
    public InMemoryJobHandlerRegistry(IEnumerable<IJobHandler> handlers)
    {
        ArgumentNullException.ThrowIfNull(handlers);
        _handlers = new Dictionary<string, IJobHandler>(StringComparer.OrdinalIgnoreCase);

        foreach (IJobHandler h in handlers)
        {
            if (string.IsNullOrWhiteSpace(h.Type))
            {
                throw new ArgumentException("Handler type cannot be null/empty.", nameof(handlers));
            }

            _handlers[h.Type.Trim()] = h;
        }
    }

    /// <inheritdoc />
    public IReadOnlyCollection<IJobHandler> Handlers => _handlers.Values;

    /// <inheritdoc />
    public bool TryResolve(string type, out IJobHandler handler)
    {
        handler = null!;
        return !string.IsNullOrWhiteSpace(type) && _handlers.TryGetValue(type.Trim(), out handler!);
    }
}