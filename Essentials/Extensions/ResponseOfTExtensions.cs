using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for objects implementing <see cref="IResponse{T}"/>.
/// </summary>
public static class ResponseOfTExtensions
{
    /// <summary>
    /// Gets the response data or throws an exception when the response contains no data.
    /// </summary>
    /// <typeparam name="T">The response data type.</typeparam>
    /// <param name="response">The response.</param>
    /// <returns>The response data.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the response contains no data.</exception>
    public static T GetDataOrThrow<T>(this IResponse<T> response)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (!response.HasData || response.Data is null)
        {
            throw new InvalidOperationException(
                "The response does not contain data.");
        }

        return response.Data;
    }

    /// <summary>
    /// Gets the response data or returns the specified fallback value when the response contains no data.
    /// </summary>
    /// <typeparam name="T">The response data type.</typeparam>
    /// <param name="response">The response.</param>
    /// <param name="fallback">The fallback value.</param>
    /// <returns>The response data if available; otherwise, the fallback value.</returns>
    public static T? GetDataOrDefault<T>(
        this IResponse<T> response,
        T? fallback = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        return response.HasData
            ? response.Data
            : fallback;
    }

    /// <summary>
    /// Determines whether the response is successful and contains data matching the specified predicate.
    /// </summary>
    /// <typeparam name="T">The response data type.</typeparam>
    /// <param name="response">The response.</param>
    /// <param name="predicate">The predicate used to test the data.</param>
    /// <returns><c>true</c> if the response is successful and its data matches the predicate; otherwise, <c>false</c>.</returns>
    public static bool IsSuccessWithData<T>(
        this IResponse<T> response,
        Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(predicate);

        return response.IsSuccess
            && response.HasData
            && response.Data is not null
            && predicate(response.Data);
    }

    /// <summary>
    /// Determines whether the response contains data matching the specified predicate.
    /// </summary>
    /// <typeparam name="T">The response data type.</typeparam>
    /// <param name="response">The response.</param>
    /// <param name="predicate">The predicate used to test the data.</param>
    /// <returns><c>true</c> if the response contains data matching the predicate; otherwise, <c>false</c>.</returns>
    public static bool HasDataMatching<T>(
        this IResponse<T> response,
        Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(predicate);

        return response.HasData
            && response.Data is not null
            && predicate(response.Data);
    }
}