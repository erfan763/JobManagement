namespace JobManagement.Domain.ValueObjects;

/// <summary>
///     Represents the data required to execute a job.
///     This is intentionally storage-agnostic and serialization-agnostic.
/// </summary>
public sealed record JobPayload
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="JobPayload" /> class.
    ///     Creates a new <see cref="JobPayload" />.
    /// </summary>
    /// <param name="type">Job type key.</param>
    /// <param name="body">Raw body content.</param>
    /// <param name="contentType">Optional content type.</param>
    /// <param name="headers">Optional headers/metadata.</param>
    /// <exception cref="ArgumentException">Thrown when required values are invalid.</exception>
    public JobPayload(
        string type,
        string body,
        string contentType = null,
        IReadOnlyDictionary<string, string> headers = null)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Payload type is required.", nameof(type));
        }

        Type = type.Trim();
        Body = body ?? throw new ArgumentException("Payload body cannot be null.", nameof(body));
        ContentType = string.IsNullOrWhiteSpace(contentType) ? null : contentType.Trim();
        Headers = headers ?? new Dictionary<string, string>();
    }

    /// <summary>
    ///     Gets logical job type (e.g., "SendEmail", "RebuildIndex").
    /// </summary>
    public string Type { get; }

    /// <summary>
    ///     Gets optional content type describing how <see cref="Body" /> is encoded (e.g., "application/json").
    /// </summary>
    public string ContentType { get; }

    /// <summary>
    ///     Gets the raw payload body (e.g., JSON, msgpack, or plain text).
    /// </summary>
    public string Body { get; }

    /// <summary>
    ///     Gets optional headers/metadata associated with the payload.
    ///     This is a convenient extension point for user applications.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; }
}