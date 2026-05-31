using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Issues;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for working with collections of <see cref="IssueInfo"/>.
/// </summary>
public static class IssueInfoCollectionExtensions
{
   /// <summary>
   /// Determines whether the issue collection contains any issues.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <returns><c>true</c> if the collection contains at least one issue; otherwise, <c>false</c>.</returns>
   public static bool HasAnyIssues(this IReadOnlyList<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return issues.Count > 0;
   }

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
   /// Determines whether the issue collection contains any critical or fatal issue.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <returns><c>true</c> if the collection contains a critical or fatal issue; otherwise, <c>false</c>.</returns>
   public static bool HasCriticalOrFatalIssues(this IReadOnlyList<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return issues.Any(issue => issue.Severity.IsCriticalOrHigher());
   }

   /// <summary>
   /// Determines whether the issue collection contains an issue with the specified severity.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <param name="severity">The severity to search for.</param>
   /// <returns><c>true</c> if the collection contains an issue with the specified severity; otherwise, <c>false</c>.</returns>
   public static bool HasIssueWithSeverity(
       this IReadOnlyList<IssueInfo> issues,
       IssueSeverity severity)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return issues.Any(issue => issue.Severity == severity);
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

   /// <summary>
   /// Determines whether the issue collection contains an issue with the specified code.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <param name="code">The issue code.</param>
   /// <returns><c>true</c> if the collection contains an issue with the specified code; otherwise, <c>false</c>.</returns>
   public static bool HasIssueCode(
       this IReadOnlyList<IssueInfo> issues,
       string code)
   {
      ArgumentNullException.ThrowIfNull(issues);
      ArgumentException.ThrowIfNullOrWhiteSpace(code);

      return issues.Any(issue =>
          string.Equals(
              issue.Code,
              code,
              StringComparison.OrdinalIgnoreCase));
   }

   /// <summary>
   /// Attempts to get the first issue with the specified code.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <param name="code">The issue code.</param>
   /// <param name="issue">The issue, if found.</param>
   /// <returns><c>true</c> if an issue with the specified code was found; otherwise, <c>false</c>.</returns>
   public static bool TryGetIssueByCode(
       this IReadOnlyList<IssueInfo> issues,
       string code,
       out IssueInfo? issue)
   {
      ArgumentNullException.ThrowIfNull(issues);
      ArgumentException.ThrowIfNullOrWhiteSpace(code);

      issue = issues.FirstOrDefault(currentIssue =>
          string.Equals(
              currentIssue.Code,
              code,
              StringComparison.OrdinalIgnoreCase));

      return issue is not null;
   }
   /// <summary>
   /// Creates a new issue collection containing only issues with the specified severity.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <param name="severity">The issue severity.</param>
   /// <returns>A new issue collection containing only matching issues.</returns>
   public static IReadOnlyList<IssueInfo> WhereSeverity(
       this IReadOnlyList<IssueInfo> issues,
       IssueSeverity severity)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return [.. issues.Where(issue => issue.Severity == severity)];
   }

   /// <summary>
   /// Creates a new issue collection containing only warning or more severe issues.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <returns>A new issue collection containing warning, error, critical, and fatal issues.</returns>
   public static IReadOnlyList<IssueInfo> WhereWarningOrHigher(
       this IReadOnlyList<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return [.. issues.Where(issue => issue.Severity.IsWarningOrHigher())];
   }

   /// <summary>
   /// Creates a new issue collection containing only error or more severe issues.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <returns>A new issue collection containing error, critical, and fatal issues.</returns>
   public static IReadOnlyList<IssueInfo> WhereErrorOrHigher(
       this IReadOnlyList<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return [.. issues.Where(issue => issue.Severity.IsErrorOrHigher())];
   }

   /// <summary>
   /// Creates a new issue collection containing only critical or fatal issues.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <returns>A new issue collection containing critical and fatal issues.</returns>
   public static IReadOnlyList<IssueInfo> WhereCriticalOrHigher(
       this IReadOnlyList<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return [.. issues.Where(issue => issue.Severity.IsCriticalOrHigher())];
   }
   /// <summary>
   /// Counts issues with the specified severity.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <param name="severity">The issue severity.</param>
   /// <returns>The number of issues with the specified severity.</returns>
   public static int CountSeverity(
       this IReadOnlyList<IssueInfo> issues,
       IssueSeverity severity)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return issues.Count(issue => issue.Severity == severity);
   }

   /// <summary>
   /// Counts warning or more severe issues.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <returns>The number of warning, error, critical, and fatal issues.</returns>
   public static int CountWarningOrHigher(
       this IReadOnlyList<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return issues.Count(issue => issue.Severity.IsWarningOrHigher());
   }

   /// <summary>
   /// Counts error or more severe issues.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <returns>The number of error, critical, and fatal issues.</returns>
   public static int CountErrorOrHigher(
       this IReadOnlyList<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return issues.Count(issue => issue.Severity.IsErrorOrHigher());
   }

   /// <summary>
   /// Counts critical or fatal issues.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <returns>The number of critical and fatal issues.</returns>
   public static int CountCriticalOrHigher(
       this IReadOnlyList<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return issues.Count(issue => issue.Severity.IsCriticalOrHigher());
   }

   /// <summary>
   /// Gets the highest severity from the issue collection.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <returns>The highest issue severity, or <see cref="IssueSeverity.None"/> when the collection is empty.</returns>
   public static IssueSeverity GetHighestSeverity(
       this IReadOnlyList<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      if (issues.Count == 0)
      {
         return IssueSeverity.None;
      }

      var highestSeverity = IssueSeverity.None;
      var highestRank = GetSeverityRank(highestSeverity);

      foreach (var issue in issues)
      {
         var rank = GetSeverityRank(issue.Severity);

         if (rank > highestRank)
         {
            highestSeverity = issue.Severity;
            highestRank = rank;
         }
      }

      return highestSeverity;
   }

   private static int GetSeverityRank(IssueSeverity severity)
   {
      return severity switch
      {
         IssueSeverity.None => 0,
         IssueSeverity.Trace => 1,
         IssueSeverity.Debug => 2,
         IssueSeverity.Information => 3,
         IssueSeverity.Warning => 4,
         IssueSeverity.Error => 5,
         IssueSeverity.Critical => 6,
         IssueSeverity.Fatal => 7,
         _ => 0
      };
   }

   /// <summary>
   /// Determines a result status from the highest severity in the issue collection.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <returns>The result status inferred from the issue severities.</returns>
   public static ResultStatus ToResultStatus(
       this IReadOnlyList<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      var highestSeverity = issues.GetHighestSeverity();

      if (highestSeverity.IsErrorOrHigher())
      {
         return ResultStatus.Failed;
      }

      if (highestSeverity.IsWarningOrHigher())
      {
         return ResultStatus.SuccessWithWarnings;
      }

      return ResultStatus.Success;
   }
   /// <summary>
   /// Determines whether the issue collection contains only informational or lower severity issues.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <returns><c>true</c> if all issues are informational or lower severity; otherwise, <c>false</c>.</returns>
   public static bool HasOnlyInformationalOrLowerIssues(
       this IReadOnlyList<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return issues.All(issue =>
          !issue.Severity.IsWarningOrHigher());
   }

   /// <summary>
   /// Determines whether the issue collection contains any warning or more severe issue.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <returns><c>true</c> if the collection contains a warning, error, critical, or fatal issue; otherwise, <c>false</c>.</returns>
   public static bool HasWarningOrHigherIssues(
       this IReadOnlyList<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return issues.HasWarningsOrErrors();
   }

   /// <summary>
   /// Determines whether the issue collection contains any error or more severe issue.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <returns><c>true</c> if the collection contains an error, critical, or fatal issue; otherwise, <c>false</c>.</returns>
   public static bool HasErrorOrHigherIssues(
       this IReadOnlyList<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return issues.HasErrors();
   }

   /// <summary>
   /// Determines whether the issue collection contains any critical or fatal issue.
   /// </summary>
   /// <param name="issues">The issue collection.</param>
   /// <returns><c>true</c> if the collection contains a critical or fatal issue; otherwise, <c>false</c>.</returns>
   public static bool HasCriticalOrHigherIssues(
       this IReadOnlyList<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return issues.HasCriticalOrFatalIssues();
   }
}
