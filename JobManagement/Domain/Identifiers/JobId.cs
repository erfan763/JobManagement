namespace JobManagement.Domain.Identifiers;

/// <summary>
///     Strongly-typed identifier for a job.
/// </summary>
public readonly record struct JobId
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="JobId" /> struct.
    ///     Creates a new <see cref="JobId" />.
    /// </summary>
    /// <param name="value">The underlying GUID value.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value" /> is empty.</exception>
    public JobId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("JobId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>
    ///     Gets the raw identifier value.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    ///     Creates a new random <see cref="JobId" />.
    /// </summary>
    /// <returns>Return Guid. </returns>
    public static JobId New()
    {
        return new JobId(Guid.NewGuid());
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString("D");
    }

    /// <summary>
    ///     Tries to parse a string into a <see cref="JobId" />.
    /// </summary>
    /// <returns>Boolean value. </returns>
    public static bool TryParse(string value, out JobId jobId)
    {
        jobId = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!Guid.TryParse(value, out Guid guid) || guid == Guid.Empty)
        {
            return false;
        }

        jobId = new JobId(guid);
        return true;
    }

    /// <summary>
    ///     Parses a <see cref="JobId" /> from its string representation.
    /// </summary>
    /// <param name="value">String representation (GUID format).</param>
    /// <returns>A parsed <see cref="JobId" />.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="value" /> is null.</exception>
    /// <exception cref="FormatException">If <paramref name="value" /> is not a valid GUID.</exception>
    public static JobId Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return !Guid.TryParse(value, out Guid guid)
            ? throw new FormatException($"'{value}' is not a valid JobId (GUID).")
            : new JobId(guid);
    }
}