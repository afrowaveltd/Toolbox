using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Issues;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for working with collections of <see cref="IssueInfo"/>.
/// </summary>
public static class IssueInfoCollectionExtensions
{
   /// <summary>
   /// Determines whether the collection contains at least one issue with severity
   /// <see cref="IssueSeverity.Error"/> or higher.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <returns><c>true</c> if the collection contains an error or more severe issue; otherwise, <c>false</c>.</returns>
   public static bool HasErrors(this IEnumerable<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return issues.Any(issue => issue.Severity.IsErrorOrHigher());
   }

   /// <summary>
   /// Determines whether the collection contains at least one issue with severity
   /// <see cref="IssueSeverity.Warning"/> or higher.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <returns><c>true</c> if the collection contains a warning or more severe issue; otherwise, <c>false</c>.</returns>
   public static bool HasWarningsOrErrors(this IEnumerable<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return issues.Any(issue => issue.Severity.IsWarningOrHigher());
   }

   /// <summary>
   /// Returns issues with severity <see cref="IssueSeverity.Error"/> or higher.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <returns>Issues with error or higher severity.</returns>
   public static IEnumerable<IssueInfo> Errors(this IEnumerable<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return issues.Where(issue => issue.Severity.IsErrorOrHigher());
   }

   /// <summary>
   /// Returns issues with severity <see cref="IssueSeverity.Warning"/>.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <returns>Issues with warning severity.</returns>
   public static IEnumerable<IssueInfo> Warnings(this IEnumerable<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return issues.Where(issue => issue.Severity == IssueSeverity.Warning);
   }

   /// <summary>
   /// Returns issues with severity <see cref="IssueSeverity.Information"/> or lower.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <returns>Informational, debug, trace, or none-severity issues.</returns>
   public static IEnumerable<IssueInfo> Informational(this IEnumerable<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return issues.Where(issue => issue.Severity.IsInformationOrLower());
   }

   /// <summary>
   /// Returns issues with the specified severity.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <param name="severity">The severity to filter by.</param>
   /// <returns>Issues with the specified severity.</returns>
   public static IEnumerable<IssueInfo> WithSeverity(
       this IEnumerable<IssueInfo> issues,
       IssueSeverity severity)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return issues.Where(issue => issue.Severity == severity);
   }

   /// <summary>
/// Creates a new issue collection by appending one issue.
/// </summary>
/// <param name="issues">The source issue collection.</param>
/// <param name="issue">The issue to append.</param>
/// <returns>A new issue collection containing the source issues and the appended issue.</returns>
public static IReadOnlyList<IssueInfo> AppendIssue(
    this IReadOnlyList<IssueInfo> issues,
    IssueInfo issue)
{
    ArgumentNullException.ThrowIfNull(issues);
    ArgumentNullException.ThrowIfNull(issue);

    return [.. issues, issue];
}

/// <summary>
/// Creates a new issue collection by appending multiple issues.
/// </summary>
/// <param name="issues">The source issue collection.</param>
/// <param name="additionalIssues">The issues to append.</param>
/// <returns>A new issue collection containing the source issues and the appended issues.</returns>
public static IReadOnlyList<IssueInfo> AppendIssues(
    this IReadOnlyList<IssueInfo> issues,
    IEnumerable<IssueInfo> additionalIssues)
{
    ArgumentNullException.ThrowIfNull(issues);
    ArgumentNullException.ThrowIfNull(additionalIssues);

    return [.. issues, .. additionalIssues];
}
}