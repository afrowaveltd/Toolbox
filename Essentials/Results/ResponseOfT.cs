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
      || Issues?.HasWarningOrHigherIssues() == true;

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
         Issues = NormalizeIssues(issues)
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
         Issues = NormalizeIssues(issues)
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
         Issues = NormalizeIssues(response.Issues),
         Metadata = CopyMetadataOrEmpty(response.Metadata)
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
         Issues = NormalizeIssues(response.Issues),
         Metadata = CopyMetadataOrEmpty(response.Metadata)
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
         Issues = NormalizeIssues(response.Issues),
         Metadata = CopyMetadataOrEmpty(response.Metadata)
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
         Issues = NormalizeIssues(issues),
         Metadata = CopyMetadataOrEmpty(response.Metadata)
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
         Issues = NormalizeIssues(response.Issues),
         Metadata = CopyMetadataOrEmpty(metadata)
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
         NormalizeIssues(response.Issues).AppendIssue(issue));
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
         NormalizeIssues(response.Issues).AppendIssues(issues));
   }

   /// <summary>
   /// Creates a copy of a response with one metadata value added or updated.
   /// </summary>
   /// <param name="response">The source response.</param>
   /// <param name="key">The metadata key.</param>
   /// <param name="value">The metadata value.</param>
   /// <returns>A new response with copied values and the specified metadata value set.</returns>
   public static Response<T> AddMetadata(
      Response<T> response,
      string key,
      string value)
   {
      ArgumentNullException.ThrowIfNull(response);

      return WithMetadata(
         response,
         MetadataBagFactory.CopyWith(
            response.Metadata ?? MetadataBagFactory.Empty(),
            key,
            value));
   }

   /// <summary>
   /// Creates a typed response from issue severities with data.
   /// </summary>
   /// <param name="data">The response data.</param>
   /// <param name="issues">The issues to attach.</param>
   /// <returns>A typed response with status inferred from issue severities and the specified data.</returns>
   public static Response<T> FromIssues(
      T? data,
      IReadOnlyList<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return new Response<T>
      {
         Status = issues.ToResultStatus(),
         Data = data,
         Issues = NormalizeIssues(issues)
      };
   }

   /// <summary>
   /// Creates a typed response from issue severities with data and a message.
   /// </summary>
   /// <param name="data">The response data.</param>
   /// <param name="issues">The issues to attach.</param>
   /// <param name="message">The response message.</param>
   /// <returns>A typed response with status inferred from issue severities, the specified data, and the specified message.</returns>
   public static Response<T> FromIssues(
      T? data,
      IReadOnlyList<IssueInfo> issues,
      string message)
   {
      ArgumentNullException.ThrowIfNull(issues);
      ArgumentException.ThrowIfNullOrWhiteSpace(message);

      return new Response<T>
      {
         Status = issues.ToResultStatus(),
         Data = data,
         Message = message,
         Issues = NormalizeIssues(issues)
      };
   }

   /// <summary>
   /// Creates a typed response from one issue with data.
   /// </summary>
   /// <param name="data">The response data.</param>
   /// <param name="issue">The issue to attach.</param>
   /// <returns>A typed response with status inferred from the issue severity and the specified data.</returns>
   public static Response<T> FromIssue(
      T? data,
      IssueInfo issue)
   {
      ArgumentNullException.ThrowIfNull(issue);

      return FromIssues(
         data,
         [
            issue
         ]);
   }

   /// <summary>
   /// Creates a typed response from one issue with data and a message.
   /// </summary>
   /// <param name="data">The response data.</param>
   /// <param name="issue">The issue to attach.</param>
   /// <param name="message">The response message.</param>
   /// <returns>A typed response with status inferred from the issue severity, the specified data, and the specified message.</returns>
   public static Response<T> FromIssue(
      T? data,
      IssueInfo issue,
      string message)
   {
      ArgumentNullException.ThrowIfNull(issue);
      ArgumentException.ThrowIfNullOrWhiteSpace(message);

      return FromIssues(
         data,
         [
            issue
         ],
         message);
   }

   /// <summary>
   /// Creates an informational typed response with data.
   /// </summary>
   /// <param name="data">The response data.</param>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The informational message.</param>
   /// <returns>A successful typed response with one informational issue.</returns>
   public static Response<T> Information(
      T? data,
      string code,
      string message)
   {
      return FromIssue(
         data,
         IssueInfoFactory.Information(
            code,
            message));
   }

   /// <summary>
   /// Creates a warning typed response with data.
   /// </summary>
   /// <param name="data">The response data.</param>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The warning message.</param>
   /// <returns>A successful typed response with warnings.</returns>
   public static Response<T> Warning(
      T? data,
      string code,
      string message)
   {
      return FromIssue(
         data,
         IssueInfoFactory.Warning(
            code,
            message));
   }

   /// <summary>
   /// Creates a partial typed response with data.
   /// </summary>
   /// <param name="data">The response data.</param>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The partial response message.</param>
   /// <returns>A partial typed response.</returns>
   public static Response<T> Partial(
      T? data,
      string code,
      string message)
   {
      return new Response<T>
      {
         Status = ResultStatus.Partial,
         Data = data,
         Message = message,
         Issues = IssueInfoListFactory.Warning(code, message)
      };
   }

   /// <summary>
   /// Creates a not supported typed response with data.
   /// </summary>
   /// <param name="data">The response data.</param>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The not supported response message.</param>
   /// <returns>A not supported typed response.</returns>
   public static Response<T> NotSupported(
      T? data,
      string code,
      string message)
   {
      return new Response<T>
      {
         Status = ResultStatus.NotSupported,
         Data = data,
         Message = message,
         Issues = IssueInfoListFactory.Error(code, message)
      };
   }

   /// <summary>
   /// Creates a cancelled typed response with data.
   /// </summary>
   /// <param name="data">The response data.</param>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The cancelled response message.</param>
   /// <returns>A cancelled typed response.</returns>
   public static Response<T> Cancelled(
      T? data,
      string code,
      string message)
   {
      return new Response<T>
      {
         Status = ResultStatus.Cancelled,
         Data = data,
         Message = message,
         Issues = IssueInfoListFactory.Warning(code, message)
      };
   }

   private static IReadOnlyList<IssueInfo> NormalizeIssues(
      IReadOnlyList<IssueInfo>? issues)
   {
      return issues ?? IssueInfoListFactory.Empty();
   }

   private static MetadataBag CopyMetadataOrEmpty(MetadataBag? metadata)
   {
      return metadata?.Copy() ?? MetadataBagFactory.Empty();
   }
}