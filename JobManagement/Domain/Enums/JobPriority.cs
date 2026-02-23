namespace JobManagement.Domain.Enums;

/// <summary>
///     Defines the relative priority of jobs when selecting the next work item.
/// </summary>
public enum JobPriority
{
    /// <summary>
    ///     Lowest priority.
    /// </summary>
    Low = 0,

    /// <summary>
    ///     Normal priority.
    /// </summary>
    Normal = 1,

    /// <summary>
    ///     High priority.
    /// </summary>
    High = 2,

    /// <summary>
    ///     Critical priority, should be processed before everything else.
    /// </summary>
    Critical = 3,
}