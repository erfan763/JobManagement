namespace JobManagement.Abstractions.Serialization;

/// <summary>
///     Provides an abstraction for serializing and deserializing job payload bodies.
///     This allows the library to remain independent of any specific JSON or
///     serialization framework.
/// </summary>
public interface IJobPayloadSerializer
{
    /// <summary>
    ///     Serializes a value into a string payload suitable for storage or transport.
    /// </summary>
    /// <typeparam name="T">The type of the value to serialize.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <returns>
    ///     A string representation of the provided <paramref name="value" /> that can
    ///     later be deserialized back into the original type.
    /// </returns>
    string Serialize<T>(T value);

    /// <summary>
    ///     Deserializes a string payload into a strongly typed value.
    /// </summary>
    /// <typeparam name="T">The expected type of the deserialized value.</typeparam>
    /// <param name="body">The serialized payload string.</param>
    /// <returns>
    ///     An instance of <typeparamref name="T" /> created from the provided
    ///     <paramref name="body" />.
    /// </returns>
    T Deserialize<T>(string body);
}