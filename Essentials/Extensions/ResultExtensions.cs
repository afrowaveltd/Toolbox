using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for objects implementing <see cref="IResult"/>.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Determines whether the result has the specified status.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="status">The status to compare with.</param>
    /// <returns><c>true</c> if the result has the specified status; otherwise, <c>false</c>.</returns>
    public static bool HasStatus(
        this IResult result,
        ResultStatus status)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Status == status;
    }

    /// <summary>
    /// Determines whether the result status is unknown.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns><c>true</c> if the status is unknown; otherwise, <c>false</c>.</returns>
    public static bool IsUnknown(this IResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Status == ResultStatus.Unknown;
    }

    /// <summary>
    /// Determines whether the result is invalid.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns><c>true</c> if the status is invalid; otherwise, <c>false</c>.</returns>
    public static bool IsInvalid(this IResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Status == ResultStatus.Invalid;
    }

    /// <summary>
    /// Determines whether the result represents a not found state.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns><c>true</c> if the status is not found; otherwise, <c>false</c>.</returns>
    public static bool IsNotFound(this IResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Status == ResultStatus.NotFound;
    }

    /// <summary>
    /// Determines whether the result has any issues.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns><c>true</c> if the result has at least one issue; otherwise, <c>false</c>.</returns>
    public static bool HasIssues(this IResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Issues.HasAnyIssues();
    }

    /// <summary>
    /// Determines whether the result has error issues.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns><c>true</c> if the result has error or more severe issues; otherwise, <c>false</c>.</returns>
    public static bool HasErrors(this IResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Issues.HasErrors();
    }

    /// <summary>
    /// Determines whether the result has a non-empty message.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns><c>true</c> if the result message is not null, empty, or whitespace; otherwise, <c>false</c>.</returns>
    public static bool HasMessage(this IResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return !string.IsNullOrWhiteSpace(result.Message);
    }
}