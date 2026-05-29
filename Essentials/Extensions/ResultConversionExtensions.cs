using Afrowave.Toolbox.Essentials.Interfaces;
using Afrowave.Toolbox.Essentials.Results;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides conversion helper methods for result and response contracts.
/// </summary>
public static class ResultConversionExtensions
{
    /// <summary>
    /// Converts a result to a non-generic response.
    /// </summary>
    /// <param name="result">The source result.</param>
    /// <returns>A response with copied result values.</returns>
    public static Response ToResponse(this IResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new Response
        {
            Status = result.Status,
            Message = result.Message,
            Issues = result.Issues,
            Metadata = result.Metadata
        };
    }

    /// <summary>
    /// Converts a result to a typed response with data.
    /// </summary>
    /// <typeparam name="T">The response data type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="data">The response data.</param>
    /// <returns>A typed response with copied result values and the specified data.</returns>
    public static Response<T> ToResponse<T>(
        this IResult result,
        T? data)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new Response<T>
        {
            Status = result.Status,
            Message = result.Message,
            Data = data,
            Issues = result.Issues,
            Metadata = result.Metadata
        };
    }

    /// <summary>
    /// Converts a response to a result without data.
    /// </summary>
    /// <param name="response">The source response.</param>
    /// <returns>A result with copied response values.</returns>
    public static Result ToResult(this IResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return new Result
        {
            Status = response.Status,
            Message = response.Message,
            Issues = response.Issues,
            Metadata = response.Metadata
        };
    }
/// <summary>
/// Converts a non-generic response to a typed response with data.
/// </summary>
/// <typeparam name="T">The response data type.</typeparam>
/// <param name="response">The source response.</param>
/// <param name="data">The response data.</param>
/// <returns>A typed response with copied response values and the specified data.</returns>
public static Response<T> ToTypedResponse<T>(
    this IResponse response,
    T? data)
{
    ArgumentNullException.ThrowIfNull(response);

    return new Response<T>
    {
        Status = response.Status,
        Message = response.Message,
        Data = data,
        Issues = response.Issues,
        Metadata = response.Metadata
    };
}

/// <summary>
/// Converts a typed response to a non-generic response by dropping the data payload.
/// </summary>
/// <typeparam name="T">The response data type.</typeparam>
/// <param name="response">The source typed response.</param>
/// <returns>A non-generic response with copied response values.</returns>
public static Response ToNonGenericResponse<T>(
    this IResponse<T> response)
{
    ArgumentNullException.ThrowIfNull(response);

    return new Response
    {
        Status = response.Status,
        Message = response.Message,
        Issues = response.Issues,
        Metadata = response.Metadata
    };
}

}