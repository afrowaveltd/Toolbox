using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Results;

/// <summary>
/// Represents the result of an operation without a data payload.
/// </summary>
public sealed class Result : IResult
{
   /// <summary>
   /// Gets or sets the result status.
   /// </summary>
   public ResultStatus Status { get; set; } = ResultStatus.Unknown;

   /// <summary>
   /// Gets or sets the result message.
   /// </summary>
   public string Message { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the issues attached to the result.
   /// </summary>
   public IReadOnlyList<IssueInfo> Issues { get; set; } = IssueInfoListFactory.Empty();

   /// <summary>
   /// Gets or sets the metadata attached to the result.
   /// </summary>
   public MetadataBag Metadata { get; set; } = MetadataBagFactory.Empty();

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
   /// Creates a successful result.
   /// </summary>
   /// <returns>A successful result.</returns>
   public static Result Ok()
   {
      return new Result
      {
         Status = ResultStatus.Success
      };
   }

   /// <summary>
   /// Creates a successful result with a message.
   /// </summary>
   /// <param name="message">The result message.</param>
   /// <returns>A successful result.</returns>
   public static Result Ok(string message)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(message);

      return new Result
      {
         Status = ResultStatus.Success,
         Message = message
      };
   }

   /// <summary>
   /// Creates a successful result with warning issues.
   /// </summary>
   /// <param name="issues">The warning issues.</param>
   /// <returns>A successful result with warnings.</returns>
   public static Result OkWithWarnings(IReadOnlyList<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return new Result
      {
         Status = ResultStatus.SuccessWithWarnings,
         Issues = NormalizeIssues(issues)
      };
   }

   /// <summary>
   /// Creates a failed result.
   /// </summary>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The failure message.</param>
   /// <returns>A failed result.</returns>
   public static Result Fail(
      string code,
      string message)
   {
      return new Result
      {
         Status = ResultStatus.Failed,
         Message = message,
         Issues = IssueInfoListFactory.Error(code, message)
      };
   }

   /// <summary>
   /// Creates a failed result from issues.
   /// </summary>
   /// <param name="issues">The issues.</param>
   /// <returns>A failed result.</returns>
   public static Result Fail(IReadOnlyList<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return new Result
      {
         Status = ResultStatus.Failed,
         Issues = NormalizeIssues(issues)
      };
   }

   /// <summary>
   /// Creates an invalid result.
   /// </summary>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The invalid result message.</param>
   /// <returns>An invalid result.</returns>
   public static Result Invalid(
      string code,
      string message)
   {
      return new Result
      {
         Status = ResultStatus.Invalid,
         Message = message,
         Issues = IssueInfoListFactory.Error(code, message)
      };
   }

   /// <summary>
   /// Creates a not found result.
   /// </summary>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The not found result message.</param>
   /// <returns>A not found result.</returns>
   public static Result NotFound(
      string code,
      string message)
   {
      return new Result
      {
         Status = ResultStatus.NotFound,
         Message = message,
         Issues = IssueInfoListFactory.Error(code, message)
      };
   }

   /// <summary>
   /// Creates a copy of a result with a message.
   /// </summary>
   /// <param name="result">The source result.</param>
   /// <param name="message">The result message.</param>
   /// <returns>A new result with copied values and the specified message.</returns>
   public static Result WithMessage(
      Result result,
      string message)
   {
      ArgumentNullException.ThrowIfNull(result);
      ArgumentException.ThrowIfNullOrWhiteSpace(message);

      return new Result
      {
         Status = result.Status,
         Message = message,
         Issues = NormalizeIssues(result.Issues),
         Metadata = CopyMetadataOrEmpty(result.Metadata)
      };
   }

   /// <summary>
   /// Creates a copy of a result with a status.
   /// </summary>
   /// <param name="result">The source result.</param>
   /// <param name="status">The result status.</param>
   /// <returns>A new result with copied values and the specified status.</returns>
   public static Result WithStatus(
      Result result,
      ResultStatus status)
   {
      ArgumentNullException.ThrowIfNull(result);

      return new Result
      {
         Status = status,
         Message = result.Message,
         Issues = NormalizeIssues(result.Issues),
         Metadata = CopyMetadataOrEmpty(result.Metadata)
      };
   }

   /// <summary>
   /// Creates a copy of a result with issues.
   /// </summary>
   /// <param name="result">The source result.</param>
   /// <param name="issues">The issues to attach.</param>
   /// <returns>A new result with copied values and the specified issues.</returns>
   public static Result WithIssues(
      Result result,
      IReadOnlyList<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(result);
      ArgumentNullException.ThrowIfNull(issues);

      return new Result
      {
         Status = result.Status,
         Message = result.Message,
         Issues = NormalizeIssues(issues),
         Metadata = CopyMetadataOrEmpty(result.Metadata)
      };
   }

   /// <summary>
   /// Creates a copy of a result with metadata.
   /// </summary>
   /// <param name="result">The source result.</param>
   /// <param name="metadata">The metadata to attach.</param>
   /// <returns>A new result with copied values and the specified metadata.</returns>
   public static Result WithMetadata(
      Result result,
      MetadataBag metadata)
   {
      ArgumentNullException.ThrowIfNull(result);
      ArgumentNullException.ThrowIfNull(metadata);

      return new Result
      {
         Status = result.Status,
         Message = result.Message,
         Issues = NormalizeIssues(result.Issues),
         Metadata = CopyMetadataOrEmpty(metadata)
      };
   }

   /// <summary>
   /// Creates a copy of a result with one appended issue.
   /// </summary>
   /// <param name="result">The source result.</param>
   /// <param name="issue">The issue to append.</param>
   /// <returns>A new result with copied values and the appended issue.</returns>
   public static Result AddIssue(
      Result result,
      IssueInfo issue)
   {
      ArgumentNullException.ThrowIfNull(result);
      ArgumentNullException.ThrowIfNull(issue);

      return WithIssues(
         result,
         NormalizeIssues(result.Issues).AppendIssue(issue));
   }

   /// <summary>
   /// Creates a copy of a result with appended issues.
   /// </summary>
   /// <param name="result">The source result.</param>
   /// <param name="issues">The issues to append.</param>
   /// <returns>A new result with copied values and the appended issues.</returns>
   public static Result AddIssues(
      Result result,
      IEnumerable<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(result);
      ArgumentNullException.ThrowIfNull(issues);

      return WithIssues(
         result,
         NormalizeIssues(result.Issues).AppendIssues(issues));
   }

   /// <summary>
   /// Creates a copy of a result with one metadata value added or updated.
   /// </summary>
   /// <param name="result">The source result.</param>
   /// <param name="key">The metadata key.</param>
   /// <param name="value">The metadata value.</param>
   /// <returns>A new result with copied values and the specified metadata value set.</returns>
   public static Result AddMetadata(
      Result result,
      string key,
      string value)
   {
      ArgumentNullException.ThrowIfNull(result);

      return WithMetadata(
         result,
         MetadataBagFactory.CopyWith(
            result.Metadata ?? MetadataBagFactory.Empty(),
            key,
            value));
   }

   /// <summary>
   /// Creates a result from issue severities.
   /// </summary>
   /// <param name="issues">The issues to attach.</param>
   /// <returns>A result with status inferred from issue severities.</returns>
   public static Result FromIssues(IReadOnlyList<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return new Result
      {
         Status = issues.ToResultStatus(),
         Issues = NormalizeIssues(issues)
      };
   }

   /// <summary>
   /// Creates a result from issue severities with a message.
   /// </summary>
   /// <param name="issues">The issues to attach.</param>
   /// <param name="message">The result message.</param>
   /// <returns>A result with status inferred from issue severities and the specified message.</returns>
   public static Result FromIssues(
      IReadOnlyList<IssueInfo> issues,
      string message)
   {
      ArgumentNullException.ThrowIfNull(issues);
      ArgumentException.ThrowIfNullOrWhiteSpace(message);

      return new Result
      {
         Status = issues.ToResultStatus(),
         Message = message,
         Issues = NormalizeIssues(issues)
      };
   }

   /// <summary>
   /// Creates a result from one issue.
   /// </summary>
   /// <param name="issue">The issue to attach.</param>
   /// <returns>A result with status inferred from the issue severity.</returns>
   public static Result FromIssue(IssueInfo issue)
   {
      ArgumentNullException.ThrowIfNull(issue);

      return FromIssues(
      [
         issue
      ]);
   }

   /// <summary>
   /// Creates a result from one issue with a message.
   /// </summary>
   /// <param name="issue">The issue to attach.</param>
   /// <param name="message">The result message.</param>
   /// <returns>A result with status inferred from the issue severity and the specified message.</returns>
   public static Result FromIssue(
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
   /// Creates an informational result.
   /// </summary>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The informational message.</param>
   /// <returns>A successful result with one informational issue.</returns>
   public static Result Information(
      string code,
      string message)
   {
      return FromIssue(
         IssueInfoFactory.Information(
            code,
            message));
   }

   /// <summary>
   /// Creates a warning result.
   /// </summary>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The warning message.</param>
   /// <returns>A successful result with warnings.</returns>
   public static Result Warning(
      string code,
      string message)
   {
      return FromIssue(
         IssueInfoFactory.Warning(
            code,
            message));
   }

   /// <summary>
   /// Creates a partial result.
   /// </summary>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The partial result message.</param>
   /// <returns>A partial result.</returns>
   public static Result Partial(
      string code,
      string message)
   {
      return new Result
      {
         Status = ResultStatus.Partial,
         Message = message,
         Issues = IssueInfoListFactory.Warning(code, message)
      };
   }

   /// <summary>
   /// Creates a not supported result.
   /// </summary>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The not supported result message.</param>
   /// <returns>A not supported result.</returns>
   public static Result NotSupported(
      string code,
      string message)
   {
      return new Result
      {
         Status = ResultStatus.NotSupported,
         Message = message,
         Issues = IssueInfoListFactory.Error(code, message)
      };
   }

   /// <summary>
   /// Creates a cancelled result.
   /// </summary>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The cancelled result message.</param>
   /// <returns>A cancelled result.</returns>
   public static Result Cancelled(
      string code,
      string message)
   {
      return new Result
      {
         Status = ResultStatus.Cancelled,
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