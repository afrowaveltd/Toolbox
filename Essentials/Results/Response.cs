using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Results;

/// <summary>
/// Represents a response without a typed data payload.
/// </summary>
public sealed class Response : IResponse
{
    /// <summary>
    /// Gets or sets the response status.
    /// </summary>
    public ResultStatus Status { get; set; } = ResultStatus.Unknown;

    /// <summary>
    /// Gets or sets the response message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the issues attached to the response.
    /// </summary>
    public IReadOnlyList<IssueInfo> Issues { get; set; } = IssueInfoListFactory.Empty();

    /// <summary>
    /// Gets or sets the metadata attached to the response.
    /// </summary>
    public MetadataBag Metadata { get; set; } = MetadataBagFactory.Empty();

    /// <summary>
    /// Gets a value indicating whether the response contains a data payload.
    /// </summary>
    public bool HasData => false;

    /// <summary>
    /// Gets a value indicating whether the response completed successfully.
    /// </summary>
    public bool IsSuccess =>
        Status is ResultStatus.Success or ResultStatus.SuccessWithWarnings;

    /// <summary>
    /// Gets a value indicating whether the response failed.
    /// </summary>
    public bool IsFailure =>
        Status is ResultStatus.Failed or ResultStatus.Invalid or ResultStatus.NotFound;

    /// <summary>
    /// Gets a value indicating whether the response completed with warnings.
    /// </summary>
    public bool HasWarnings =>
        Status == ResultStatus.SuccessWithWarnings
        || Issues.HasWarningsOrErrors();

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    /// <returns>A successful response.</returns>
    public static Response Ok()
    {
        return new Response
        {
            Status = ResultStatus.Success
        };
    }

    /// <summary>
    /// Creates a successful response with a message.
    /// </summary>
    /// <param name="message">The response message.</param>
    /// <returns>A successful response.</returns>
    public static Response Ok(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        return new Response
        {
            Status = ResultStatus.Success,
            Message = message
        };
    }

    /// <summary>
    /// Creates a successful response with warning issues.
    /// </summary>
    /// <param name="issues">The warning issues.</param>
    /// <returns>A successful response with warnings.</returns>
    public static Response OkWithWarnings(IReadOnlyList<IssueInfo> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        return new Response
        {
            Status = ResultStatus.SuccessWithWarnings,
            Issues = issues
        };
    }

    /// <summary>
    /// Creates a failed response.
    /// </summary>
    /// <param name="code">The stable issue code.</param>
    /// <param name="message">The failure message.</param>
    /// <returns>A failed response.</returns>
    public static Response Fail(
        string code,
        string message)
    {
        return new Response
        {
            Status = ResultStatus.Failed,
            Message = message,
            Issues = IssueInfoListFactory.Error(code, message)
        };
    }

    /// <summary>
    /// Creates a failed response from issues.
    /// </summary>
    /// <param name="issues">The issues.</param>
    /// <returns>A failed response.</returns>
    public static Response Fail(IReadOnlyList<IssueInfo> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        return new Response
        {
            Status = ResultStatus.Failed,
            Issues = issues
        };
    }

    /// <summary>
    /// Creates an invalid response.
    /// </summary>
    /// <param name="code">The stable issue code.</param>
    /// <param name="message">The invalid response message.</param>
    /// <returns>An invalid response.</returns>
    public static Response Invalid(
        string code,
        string message)
    {
        return new Response
        {
            Status = ResultStatus.Invalid,
            Message = message,
            Issues = IssueInfoListFactory.Error(code, message)
        };
    }

    /// <summary>
    /// Creates a not found response.
    /// </summary>
    /// <param name="code">The stable issue code.</param>
    /// <param name="message">The not found response message.</param>
    /// <returns>A not found response.</returns>
    public static Response NotFound(
        string code,
        string message)
    {
        return new Response
        {
            Status = ResultStatus.NotFound,
            Message = message,
            Issues = IssueInfoListFactory.Error(code, message)
        };
    }
    /// <summary>
/// Creates a copy of a response with a message.
/// </summary>
/// <param name="response">The source response.</param>
/// <param name="message">The response message.</param>
/// <returns>A new response with copied values and the specified message.</returns>
public static Response WithMessage(
    Response response,
    string message)
{
    ArgumentNullException.ThrowIfNull(response);
    ArgumentException.ThrowIfNullOrWhiteSpace(message);

    return new Response
    {
        Status = response.Status,
        Message = message,
        Issues = response.Issues,
        Metadata = response.Metadata
    };
}

/// <summary>
/// Creates a copy of a response with a status.
/// </summary>
/// <param name="response">The source response.</param>
/// <param name="status">The response status.</param>
/// <returns>A new response with copied values and the specified status.</returns>
public static Response WithStatus(
    Response response,
    ResultStatus status)
{
    ArgumentNullException.ThrowIfNull(response);

    return new Response
    {
        Status = status,
        Message = response.Message,
        Issues = response.Issues,
        Metadata = response.Metadata
    };
}

/// <summary>
/// Creates a copy of a response with issues.
/// </summary>
/// <param name="response">The source response.</param>
/// <param name="issues">The issues to attach.</param>
/// <returns>A new response with copied values and the specified issues.</returns>
public static Response WithIssues(
    Response response,
    IReadOnlyList<IssueInfo> issues)
{
    ArgumentNullException.ThrowIfNull(response);
    ArgumentNullException.ThrowIfNull(issues);

    return new Response
    {
        Status = response.Status,
        Message = response.Message,
        Issues = issues,
        Metadata = response.Metadata
    };
}

/// <summary>
/// Creates a copy of a response with metadata.
/// </summary>
/// <param name="response">The source response.</param>
/// <param name="metadata">The metadata to attach.</param>
/// <returns>A new response with copied values and the specified metadata.</returns>
public static Response WithMetadata(
    Response response,
    MetadataBag metadata)
{
    ArgumentNullException.ThrowIfNull(response);
    ArgumentNullException.ThrowIfNull(metadata);

    return new Response
    {
        Status = response.Status,
        Message = response.Message,
        Issues = response.Issues,
        Metadata = metadata
    };
}
}