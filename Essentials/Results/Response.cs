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
    /// Gets a value indicating whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess =>
        Status.IsSuccess();

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure =>
        Status.IsFailure();

    /// <summary>
    /// Gets a value indicating whether the operation completed with warnings.
    /// </summary>
    public bool HasWarnings =>
        Status.HasWarnings()
        || Issues.HasWarningOrHigherIssues();

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

    /// <summary>
    /// Creates a copy of a response with one appended issue.
    /// </summary>
    /// <param name="response">The source response.</param>
    /// <param name="issue">The issue to append.</param>
    /// <returns>A new response with copied values and the appended issue.</returns>
    public static Response AddIssue(
        Response response,
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
    public static Response AddIssues(
        Response response,
        IEnumerable<IssueInfo> issues)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(issues);

        return WithIssues(
            response,
            response.Issues.AppendIssues(issues));
    }

    /// <summary>
    /// Creates a copy of a response with one metadata value added or updated.
    /// </summary>
    /// <param name="response">The source response.</param>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>A new response with copied values and the specified metadata value set.</returns>
    public static Response AddMetadata(
        Response response,
        string key,
        string value)
    {
        ArgumentNullException.ThrowIfNull(response);

        return WithMetadata(
            response,
            MetadataBagFactory.CopyWith(
                response.Metadata,
                key,
                value));
    }
    /// <summary>
    /// Creates a response from issue severities.
    /// </summary>
    /// <param name="issues">The issues to attach.</param>
    /// <returns>A response with status inferred from issue severities.</returns>
    public static Response FromIssues(IReadOnlyList<IssueInfo> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        return new Response
        {
            Status = issues.ToResultStatus(),
            Issues = issues
        };
    }

    /// <summary>
    /// Creates a response from issue severities with a message.
    /// </summary>
    /// <param name="issues">The issues to attach.</param>
    /// <param name="message">The response message.</param>
    /// <returns>A response with status inferred from issue severities and the specified message.</returns>
    public static Response FromIssues(
        IReadOnlyList<IssueInfo> issues,
        string message)
    {
        ArgumentNullException.ThrowIfNull(issues);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        return new Response
        {
            Status = issues.ToResultStatus(),
            Message = message,
            Issues = issues
        };
    }
    /// <summary>
    /// Creates a response from one issue.
    /// </summary>
    /// <param name="issue">The issue to attach.</param>
    /// <returns>A response with status inferred from the issue severity.</returns>
    public static Response FromIssue(IssueInfo issue)
    {
        ArgumentNullException.ThrowIfNull(issue);

        return FromIssues(
        [
            issue
        ]);
    }

    /// <summary>
    /// Creates a response from one issue with a message.
    /// </summary>
    /// <param name="issue">The issue to attach.</param>
    /// <param name="message">The response message.</param>
    /// <returns>A response with status inferred from the issue severity and the specified message.</returns>
    public static Response FromIssue(
        IssueInfo issue,
        string message)
    {
        ArgumentNullException.ThrowIfNull(issue);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        return FromIssues(
            [
                issue
            ],
            message);
    }
    /// <summary>
    /// Creates an informational response.
    /// </summary>
    /// <param name="code">The stable issue code.</param>
    /// <param name="message">The informational message.</param>
    /// <returns>A successful response with one informational issue.</returns>
    public static Response Information(
        string code,
        string message)
    {
        return FromIssue(
            IssueInfoFactory.Information(
                code,
                message));
    }

    /// <summary>
    /// Creates a warning response.
    /// </summary>
    /// <param name="code">The stable issue code.</param>
    /// <param name="message">The warning message.</param>
    /// <returns>A successful response with warnings.</returns>
    public static Response Warning(
        string code,
        string message)
    {
        return FromIssue(
            IssueInfoFactory.Warning(
                code,
                message));
    }

    /// <summary>
    /// Creates a partial response.
    /// </summary>
    /// <param name="code">The stable issue code.</param>
    /// <param name="message">The partial response message.</param>
    /// <returns>A partial response.</returns>
    public static Response Partial(
        string code,
        string message)
    {
        return new Response
        {
            Status = ResultStatus.Partial,
            Message = message,
            Issues = IssueInfoListFactory.Warning(code, message)
        };
    }

    /// <summary>
    /// Creates a not supported response.
    /// </summary>
    /// <param name="code">The stable issue code.</param>
    /// <param name="message">The not supported response message.</param>
    /// <returns>A not supported response.</returns>
    public static Response NotSupported(
        string code,
        string message)
    {
        return new Response
        {
            Status = ResultStatus.NotSupported,
            Message = message,
            Issues = IssueInfoListFactory.Error(code, message)
        };
    }

    /// <summary>
    /// Creates a cancelled response.
    /// </summary>
    /// <param name="code">The stable issue code.</param>
    /// <param name="message">The cancelled response message.</param>
    /// <returns>A cancelled response.</returns>
    public static Response Cancelled(
        string code,
        string message)
    {
        return new Response
        {
            Status = ResultStatus.Cancelled,
            Message = message,
            Issues = IssueInfoListFactory.Warning(code, message)
        };
    }
}