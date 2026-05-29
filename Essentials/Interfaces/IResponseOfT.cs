namespace Afrowave.Toolbox.Essentials.Interfaces;

/// <summary>
/// Represents a common response contract with a typed data payload.
/// </summary>
/// <typeparam name="T">The response data type.</typeparam>
public interface IResponse<out T> : IResponse
{
    /// <summary>
    /// Gets the response data payload.
    /// </summary>
    T? Data { get; }
}