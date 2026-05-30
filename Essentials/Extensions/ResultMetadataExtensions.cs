using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides metadata helper methods for objects implementing <see cref="IResult"/>.
/// </summary>
public static class ResultMetadataExtensions
{
    /// <summary>
    /// Determines whether the result has any metadata values.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns><c>true</c> if the result has at least one metadata value; otherwise, <c>false</c>.</returns>
    public static bool HasMetadata(this IResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return !result.Metadata.IsEmpty;
    }

    /// <summary>
    /// Determines whether the result metadata contains the specified key.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="key">The metadata key.</param>
    /// <returns><c>true</c> if the metadata contains the specified key; otherwise, <c>false</c>.</returns>
    public static bool HasMetadataKey(
        this IResult result,
        string key)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return result.Metadata.TryGet(key, out _);
    }

    /// <summary>
    /// Attempts to get a metadata value from the result.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value, if found.</param>
    /// <returns><c>true</c> if the metadata value was found; otherwise, <c>false</c>.</returns>
    public static bool TryGetMetadata(
        this IResult result,
        string key,
        out string? value)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return result.Metadata.TryGet(key, out value);
    }
}