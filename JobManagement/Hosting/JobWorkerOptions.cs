namespace JobManagement.Hosting;

/// <summary>
///     Options for <see cref="JobWorker" />.
/// </summary>
public sealed record JobWorkerOptions
{
    /// <summary>
    ///     Gets Delay when no job is found.
    /// </summary>
    public TimeSpan IdleDelay { get; set; } = TimeSpan.FromMilliseconds(300);

    /// <summary>
    ///     Gets Delay after an unexpected error.
    /// </summary>
    public TimeSpan ErrorDelay { get; set; } = TimeSpan.FromSeconds(2);
}