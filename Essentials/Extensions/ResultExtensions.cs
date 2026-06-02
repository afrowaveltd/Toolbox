using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Interfaces;
using Afrowave.Toolbox.Essentials.Issues;

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

      return result.Status.IsUnknown();
   }

   /// <summary>
   /// Determines whether the result is invalid.
   /// </summary>
   /// <param name="result">The result.</param>
   /// <returns><c>true</c> if the status is invalid; otherwise, <c>false</c>.</returns>
   public static bool IsInvalid(this IResult result)
   {
      ArgumentNullException.ThrowIfNull(result);

      return result.Status.IsInvalid();
   }

   /// <summary>
   /// Determines whether the result represents a not found state.
   /// </summary>
   /// <param name="result">The result.</param>
   /// <returns><c>true</c> if the status is not found; otherwise, <c>false</c>.</returns>
   public static bool IsNotFound(this IResult result)
   {
      ArgumentNullException.ThrowIfNull(result);

      return result.Status.IsNotFound();
   }

   /// <summary>
   /// Determines whether the result has any issues.
   /// </summary>
   /// <param name="result">The result.</param>
   /// <returns><c>true</c> if the result has at least one issue; otherwise, <c>false</c>.</returns>
   public static bool HasIssues(this IResult result)
   {
      ArgumentNullException.ThrowIfNull(result);

      return result.Issues?.HasAnyIssues() == true;
   }

   /// <summary>
   /// Determines whether the result has error issues.
   /// </summary>
   /// <param name="result">The result.</param>
   /// <returns><c>true</c> if the result has error or more severe issues; otherwise, <c>false</c>.</returns>
   public static bool HasErrors(this IResult result)
   {
      ArgumentNullException.ThrowIfNull(result);

      return result.Issues?.HasErrorOrHigherIssues() == true;
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

   /// <summary>
   /// Determines whether the result contains an issue with the specified code.
   /// </summary>
   /// <param name="result">The result.</param>
   /// <param name="code">The issue code.</param>
   /// <returns><c>true</c> if the result contains an issue with the specified code; otherwise, <c>false</c>.</returns>
   public static bool HasIssueCode(
      this IResult result,
      string code)
   {
      ArgumentNullException.ThrowIfNull(result);
      ArgumentException.ThrowIfNullOrWhiteSpace(code);

      return result.Issues?.HasIssueCode(code) == true;
   }

   /// <summary>
   /// Attempts to get the first issue with the specified code.
   /// </summary>
   /// <param name="result">The result.</param>
   /// <param name="code">The issue code.</param>
   /// <param name="issue">The issue, if found.</param>
   /// <returns><c>true</c> if an issue with the specified code was found; otherwise, <c>false</c>.</returns>
   public static bool TryGetIssueByCode(
      this IResult result,
      string code,
      out IssueInfo? issue)
   {
      ArgumentNullException.ThrowIfNull(result);
      ArgumentException.ThrowIfNullOrWhiteSpace(code);

      if(result.Issues is null)
      {
         issue = null;
         return false;
      }

      return result.Issues.TryGetIssueByCode(
         code,
         out issue);
   }

   /// <summary>
   /// Determines whether the result is successful without warnings or issues.
   /// </summary>
   /// <param name="result">The result.</param>
   /// <returns><c>true</c> if the result is successful and has no warnings or issues; otherwise, <c>false</c>.</returns>
   public static bool IsCleanSuccess(this IResult result)
   {
      ArgumentNullException.ThrowIfNull(result);

      return result.IsSuccess
         && !result.HasWarnings
         && result.Issues?.HasAnyIssues() != true;
   }

   /// <summary>
   /// Determines whether the result is successful but contains warnings or issues.
   /// </summary>
   /// <param name="result">The result.</param>
   /// <returns><c>true</c> if the result is successful and contains warnings or issues; otherwise, <c>false</c>.</returns>
   public static bool IsDirtySuccess(this IResult result)
   {
      ArgumentNullException.ThrowIfNull(result);

      return result.IsSuccess
         && (
            result.HasWarnings
            || result.Issues?.HasAnyIssues() == true
         );
   }

   /// <summary>
   /// Determines whether the result needs attention because it failed or completed with warnings/issues.
   /// </summary>
   /// <param name="result">The result.</param>
   /// <returns><c>true</c> if the result failed or completed successfully with issues; otherwise, <c>false</c>.</returns>
   public static bool NeedsAttention(this IResult result)
   {
      ArgumentNullException.ThrowIfNull(result);

      return result.IsFailure
         || result.IsDirtySuccess();
   }

   /// <summary>
   /// Determines whether the result has warning or more severe issues.
   /// </summary>
   /// <param name="result">The result.</param>
   /// <returns><c>true</c> if the result has warning, error, critical, or fatal issues; otherwise, <c>false</c>.</returns>
   public static bool HasWarningOrHigherIssues(this IResult result)
   {
      ArgumentNullException.ThrowIfNull(result);

      return result.Issues?.HasWarningOrHigherIssues() == true;
   }

   /// <summary>
   /// Determines whether the result has error or more severe issues.
   /// </summary>
   /// <param name="result">The result.</param>
   /// <returns><c>true</c> if the result has error, critical, or fatal issues; otherwise, <c>false</c>.</returns>
   public static bool HasErrorOrHigherIssues(this IResult result)
   {
      ArgumentNullException.ThrowIfNull(result);

      return result.Issues?.HasErrorOrHigherIssues() == true;
   }

   /// <summary>
   /// Determines whether the result has critical or fatal issues.
   /// </summary>
   /// <param name="result">The result.</param>
   /// <returns><c>true</c> if the result has critical or fatal issues; otherwise, <c>false</c>.</returns>
   public static bool HasCriticalOrHigherIssues(this IResult result)
   {
      ArgumentNullException.ThrowIfNull(result);

      return result.Issues?.HasCriticalOrHigherIssues() == true;
   }

   /// <summary>
   /// Gets the highest issue severity attached to the result.
   /// </summary>
   /// <param name="result">The result.</param>
   /// <returns>The highest issue severity, or <see cref="IssueSeverity.None"/> when the result has no issues.</returns>
   public static IssueSeverity GetHighestIssueSeverity(this IResult result)
   {
      ArgumentNullException.ThrowIfNull(result);

      return result.Issues?.GetHighestSeverity() ?? IssueSeverity.None;
   }

   /// <summary>
   /// Gets a result status inferred from the issues attached to the result.
   /// </summary>
   /// <param name="result">The result.</param>
   /// <returns>The result status inferred from the issue severities.</returns>
   public static ResultStatus GetStatusFromIssues(this IResult result)
   {
      ArgumentNullException.ThrowIfNull(result);

      return result.Issues?.ToResultStatus() ?? ResultStatus.Success;
   }

   /// <summary>
   /// Determines whether the current result status matches the status inferred from attached issues.
   /// </summary>
   /// <param name="result">The result.</param>
   /// <returns><c>true</c> if the current status matches the status inferred from issues; otherwise, <c>false</c>.</returns>
   public static bool HasStatusMatchingIssues(this IResult result)
   {
      ArgumentNullException.ThrowIfNull(result);

      return result.Status == result.GetStatusFromIssues();
   }

   /// <summary>
   /// Determines whether the current result status differs from the status inferred from attached issues.
   /// </summary>
   /// <param name="result">The result.</param>
   /// <returns><c>true</c> if the current status differs from the status inferred from issues; otherwise, <c>false</c>.</returns>
   public static bool HasStatusMismatchWithIssues(this IResult result)
   {
      ArgumentNullException.ThrowIfNull(result);

      return !result.HasStatusMatchingIssues();
   }

   /// <summary>
   /// Determines whether the result status represents a partial result.
   /// </summary>
   /// <param name="result">The result.</param>
   /// <returns><c>true</c> if the result status is partial; otherwise, <c>false</c>.</returns>
   public static bool IsPartial(this IResult result)
   {
      ArgumentNullException.ThrowIfNull(result);

      return result.Status.IsPartial();
   }

   /// <summary>
   /// Determines whether the result status represents a failed result.
   /// </summary>
   /// <param name="result">The result.</param>
   /// <returns><c>true</c> if the result status is failed; otherwise, <c>false</c>.</returns>
   public static bool IsFailed(this IResult result)
   {
      ArgumentNullException.ThrowIfNull(result);

      return result.Status.IsFailed();
   }

   /// <summary>
   /// Determines whether the result status represents an unsupported operation.
   /// </summary>
   /// <param name="result">The result.</param>
   /// <returns><c>true</c> if the result status is not supported; otherwise, <c>false</c>.</returns>
   public static bool IsNotSupported(this IResult result)
   {
      ArgumentNullException.ThrowIfNull(result);

      return result.Status.IsNotSupported();
   }

   /// <summary>
   /// Determines whether the result status represents a cancelled operation.
   /// </summary>
   /// <param name="result">The result.</param>
   /// <returns><c>true</c> if the result status is cancelled; otherwise, <c>false</c>.</returns>
   public static bool IsCancelled(this IResult result)
   {
      ArgumentNullException.ThrowIfNull(result);

      return result.Status.IsCancelled();
   }

   /// <summary>
   /// Determines whether the result status is final.
   /// </summary>
   /// <param name="result">The result.</param>
   /// <returns><c>true</c> if the result status is final; otherwise, <c>false</c>.</returns>
   public static bool IsFinal(this IResult result)
   {
      ArgumentNullException.ThrowIfNull(result);

      return result.Status.IsFinal();
   }

   /// <summary>
   /// Determines whether the result status does not represent a successful result.
   /// </summary>
   /// <param name="result">The result.</param>
   /// <returns><c>true</c> if the result status is not success or success with warnings; otherwise, <c>false</c>.</returns>
   public static bool IsNonSuccess(this IResult result)
   {
      ArgumentNullException.ThrowIfNull(result);

      return result.Status.IsNonSuccess();
   }
}