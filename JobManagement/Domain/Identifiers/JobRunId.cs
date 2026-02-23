namespace JobManagement.Domain.Identifiers;

/// <summary>
///     Strongly-typed identifier for a single execution/run of a job.
///     Useful for correlating attempts and logs.
/// </summary>
public readonly record struct JobRunId
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="JobRunId" /> struct.
    ///     Creates a new <see cref="JobRunId" />.
    /// </summary>
    /// <param name="value">The underlying GUID value.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value" /> is empty.</exception>
    public JobRunId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("JobRunId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>
    ///     Gets the raw identifier value.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    ///     Creates a new random <see cref="JobRunId" />.
    /// </summary>
    /// <returns>Rturn Guid. </returns>
    public static JobRunId New()
    {
        return new JobRunId(Guid.NewGuid());
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString("D");
    }
}