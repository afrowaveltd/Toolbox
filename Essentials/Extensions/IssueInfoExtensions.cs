using Afrowave.Toolbox.Essentials.Issues;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for working with <see cref="IssueInfo"/>.
/// </summary>
public static class IssueInfoExtensions
{
   /// <summary>
   /// Determines whether the issue has a non-empty code.
   /// </summary>
   /// <param name="issue">The issue information.</param>
   /// <returns><c>true</c> if the issue code is not null, empty, or whitespace; otherwise, <c>false</c>.</returns>
   public static bool HasCode(this IssueInfo issue)
   {
      ArgumentNullException.ThrowIfNull(issue);

      return !string.IsNullOrWhiteSpace(issue.Code);
   }

   /// <summary>
   /// Determines whether the issue has a numeric identifier.
   /// </summary>
   /// <param name="issue">The issue information.</param>
   /// <returns><c>true</c> if the issue has a numeric identifier; otherwise, <c>false</c>.</returns>
   public static bool HasNumber(this IssueInfo issue)
   {
      ArgumentNullException.ThrowIfNull(issue);

      return issue.Number.HasValue;
   }

   /// <summary>
   /// Determines whether the issue has a non-empty message.
   /// </summary>
   /// <param name="issue">The issue information.</param>
   /// <returns><c>true</c> if the issue message is not null, empty, or whitespace; otherwise, <c>false</c>.</returns>
   public static bool HasMessage(this IssueInfo issue)
   {
      ArgumentNullException.ThrowIfNull(issue);

      return !string.IsNullOrWhiteSpace(issue.Message);
   }

   /// <summary>
   /// Determines whether the issue has non-empty details.
   /// </summary>
   /// <param name="issue">The issue information.</param>
   /// <returns><c>true</c> if the issue details are not null, empty, or whitespace; otherwise, <c>false</c>.</returns>
   public static bool HasDetails(this IssueInfo issue)
   {
      ArgumentNullException.ThrowIfNull(issue);

      return !string.IsNullOrWhiteSpace(issue.Details);
   }

   /// <summary>
   /// Determines whether the issue represents an error or anything more severe.
   /// </summary>
   /// <param name="issue">The issue information.</param>
   /// <returns><c>true</c> if the issue severity is error or higher; otherwise, <c>false</c>.</returns>
   public static bool IsErrorOrHigher(this IssueInfo issue)
   {
      ArgumentNullException.ThrowIfNull(issue);

      return issue.Severity.IsErrorOrHigher();
   }

   /// <summary>
   /// Determines whether the issue represents a warning or anything more severe.
   /// </summary>
   /// <param name="issue">The issue information.</param>
   /// <returns><c>true</c> if the issue severity is warning or higher; otherwise, <c>false</c>.</returns>
   public static bool IsWarningOrHigher(this IssueInfo issue)
   {
      ArgumentNullException.ThrowIfNull(issue);

      return issue.Severity.IsWarningOrHigher();
   }

   /// <summary>
   /// Determines whether the issue represents informational output or anything less severe.
   /// </summary>
   /// <param name="issue">The issue information.</param>
   /// <returns><c>true</c> if the issue severity is informational or lower; otherwise, <c>false</c>.</returns>
   public static bool IsInformationOrLower(this IssueInfo issue)
   {
      ArgumentNullException.ThrowIfNull(issue);

      return issue.Severity.IsInformationOrLower();
   }

   /// <summary>
   /// Determines whether the issue represents a critical or fatal issue.
   /// </summary>
   /// <param name="issue">The issue information.</param>
   /// <returns><c>true</c> if the issue severity is critical or higher; otherwise, <c>false</c>.</returns>
   public static bool IsCriticalOrHigher(this IssueInfo issue)
   {
      ArgumentNullException.ThrowIfNull(issue);

      return issue.Severity.IsCriticalOrHigher();
   }
}