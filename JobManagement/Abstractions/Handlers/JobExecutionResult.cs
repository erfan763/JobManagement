namespace JobManagement.Abstractions.Handlers;

/// <summary>
///     Represents the outcome of a job handler execution.
/// </summary>
public sealed record JobExecutionResult
{
    /// <summary>
    ///     Gets a value indicating whether gets True if the execution succeeded.
    /// </summary>
    public bool Succeeded { get; init; }

    /// <summary>
    ///     Gets Optional error message for failures.
    /// </summary>
    public string Error { get; init; }

    /// <summary>
    ///     Gets Optional error details (stack trace, etc).
    /// </summary>
    public string ErrorDetails { get; init; }

    /// <summary>
    ///     Gets Creates a success result.
    /// </summary>
    /// <returns>Return Success JobExecutionResult. </returns>
    public static JobExecutionResult Success()
    {
        return new JobExecutionResult
        {
            Succeeded = true,
        };
    }

    /// <summary>
    ///     Gets Creates a failure result.
    /// </summary>
    /// <returns>Return Fail JobExecutionResult. </returns>
    public static JobExecutionResult Fail(string error, string details = null)
    {
        return new JobExecutionResult
        {
            Succeeded = false,
            Error = error,
            ErrorDetails = details,
        };
    }
}