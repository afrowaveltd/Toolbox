using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for objects implementing <see cref="IResponse"/>.
/// </summary>
public static class ResponseExtensions
{
    /// <summary>
    /// Determines whether the response has no data payload.
    /// </summary>
    /// <param name="response">The response.</param>
    /// <returns><c>true</c> if the response does not contain data; otherwise, <c>false</c>.</returns>
    public static bool HasNoData(this IResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return !response.HasData;
    }

    /// <summary>
    /// Determines whether the response is successful and contains data.
    /// </summary>
    /// <param name="response">The response.</param>
    /// <returns><c>true</c> if the response is successful and contains data; otherwise, <c>false</c>.</returns>
    public static bool IsSuccessWithData(this IResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return response.IsSuccess && response.HasData;
    }

    /// <summary>
    /// Determines whether the response is successful but contains no data.
    /// </summary>
    /// <param name="response">The response.</param>
    /// <returns><c>true</c> if the response is successful and contains no data; otherwise, <c>false</c>.</returns>
    public static bool IsSuccessWithoutData(this IResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return response.IsSuccess && !response.HasData;
    }

    /// <summary>
    /// Determines whether the response failed and contains no data.
    /// </summary>
    /// <param name="response">The response.</param>
    /// <returns><c>true</c> if the response failed and contains no data; otherwise, <c>false</c>.</returns>
    public static bool IsFailureWithoutData(this IResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return response.IsFailure && !response.HasData;
    }
}