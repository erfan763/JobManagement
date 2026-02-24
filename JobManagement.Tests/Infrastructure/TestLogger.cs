using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace JobManagement.Tests.Infrastructure;

internal sealed class NullScope : IDisposable
{
    public static readonly NullScope Instance = new();

    private NullScope()
    {
    }

    public void Dispose()
    {
    }
}

/// <inheritdoc />
internal sealed class TestLogger<T> : ILogger<T>
{
    private readonly ConcurrentQueue<string> _messages = new();

    public IReadOnlyCollection<string> Messages => _messages.ToArray();

    public IDisposable BeginScope<TState>(TState state)
    {
        return NullScope.Instance;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        _messages.Enqueue($"{logLevel}: {formatter(state, exception)}");
    }
}