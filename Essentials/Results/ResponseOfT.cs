using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Results;

/// <summary>
/// Represents a response with a typed data payload.
/// </summary>
/// <typeparam name="T">The response data type.</typeparam>
public sealed class Response<T> : IResponse<T>
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
    /// Gets or sets the response data payload.
    /// </summary>
    public T? Data { get; set; }

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
    public bool HasData => Data is not null;

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
    /// Creates a successful response with data.
    /// </summary>
    /// <param name="data">The response data.</param>
    /// <returns>A successful response.</returns>
    public static Response<T> Ok(T? data)
    {
        return new Response<T>
        {
            Status = ResultStatus.Success,
            Data = data
        };
    }

    /// <summary>
    /// Creates a successful response with data and a message.
    /// </summary>
    /// <param name="data">The response data.</param>
    /// <param name="message">The response message.</param>
    /// <returns>A successful response.</returns>
    public static Response<T> Ok(
        T? data,
        string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        return new Response<T>
        {
            Status = ResultStatus.Success,
            Data = data,
            Message = message
        };
    }

    /// <summary>
    /// Creates a successful response with data and warning issues.
    /// </summary>
    /// <param name="data">The response data.</param>
    /// <param name="issues">The warning issues.</param>
    /// <returns>A successful response with warnings.</returns>
    public static Response<T> OkWithWarnings(
        T? data,
        IReadOnlyList<IssueInfo> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        return new Response<T>
        {
            Status = ResultStatus.SuccessWithWarnings,
            Data = data,
            Issues = issues
        };
    }

    /// <summary>
    /// Creates a failed response.
    /// </summary>
    /// <param name="code">The stable issue code.</param>
    /// <param name="message">The failure message.</param>
    /// <returns>A failed response.</returns>
    public static Response<T> Fail(
        string code,
        string message)
    {
        return new Response<T>
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
    public static Response<T> Fail(IReadOnlyList<IssueInfo> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        return new Response<T>
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
    public static Response<T> Invalid(
        string code,
        string message)
    {
        return new Response<T>
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
    public static Response<T> NotFound(
        string code,
        string message)
    {
        return new Response<T>
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
    public static Response<T> WithMessage(
        Response<T> response,
        string message)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        return new Response<T>
        {
            Status = response.Status,
            Message = message,
            Data = response.Data,
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
    public static Response<T> WithStatus(
        Response<T> response,
        ResultStatus status)
    {
        ArgumentNullException.ThrowIfNull(response);

        return new Response<T>
        {
            Status = status,
            Message = response.Message,
            Data = response.Data,
            Issues = response.Issues,
            Metadata = response.Metadata
        };
    }

    /// <summary>
    /// Creates a copy of a response with data.
    /// </summary>
    /// <param name="response">The source response.</param>
    /// <param name="data">The response data.</param>
    /// <returns>A new response with copied values and the specified data.</returns>
    public static Response<T> WithData(
        Response<T> response,
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
    /// Creates a copy of a response with issues.
    /// </summary>
    /// <param name="response">The source response.</param>
    /// <param name="issues">The issues to attach.</param>
    /// <returns>A new response with copied values and the specified issues.</returns>
    public static Response<T> WithIssues(
        Response<T> response,
        IReadOnlyList<IssueInfo> issues)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(issues);

        return new Response<T>
        {
            Status = response.Status,
            Message = response.Message,
            Data = response.Data,
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
    public static Response<T> WithMetadata(
        Response<T> response,
        MetadataBag metadata)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(metadata);

        return new Response<T>
        {
            Status = response.Status,
            Message = response.Message,
            Data = response.Data,
            Issues = response.Issues,
            Metadata = metadata
        };
    }
    /// <summary>
    /// Creates a copy of a response with one appended issue.
    /// </summary>
    /// <param name="response">The source response.</param>
    /// <param name="issue">The issue to append.</param>
    /// <returns>A new response with copied values and the appended issue.</returns>
    public static Response<T> AddIssue(
        Response<T> response,
        IssueInfo issue)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(issue);

        return WithIssues(
            response,
            response.Issues.AppendIssue(issue));
    }

    /// <summary>
    /// Creates a copy of a response with appended issues.
    /// </summary>
    /// <param name="response">The source response.</param>
    /// <param name="issues">The issues to append.</param>
    /// <returns>A new response with copied values and the appended issues.</returns>
    public static Response<T> AddIssues(
        Response<T> response,
        IEnumerable<IssueInfo> issues)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(issues);

        return WithIssues(
            response,
            response.Issues.AppendIssues(issues));
    }

}