using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Issues;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for issue information collections.
/// </summary>
public static class IssueInfoListExtensions
{
    /// <summary>
    /// Determines whether the issue list contains any issues.
    /// </summary>
    /// <param name="issues">The issue list.</param>
    /// <returns><c>true</c> if the list contains at least one issue; otherwise, <c>false</c>.</returns>
    public static bool HasAnyIssues(this IReadOnlyList<IssueInfo> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        return issues.Count > 0;
    }

    /// <summary>
    /// Determines whether the issue list contains any warning or more severe issue.
    /// </summary>
    /// <param name="issues">The issue list.</param>
    /// <returns><c>true</c> if the list contains a warning, error, critical, or fatal issue; otherwise, <c>false</c>.</returns>
    public static bool HasWarningsOrErrors(this IReadOnlyList<IssueInfo> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        return issues.Any(issue => issue.Severity.IsWarningOrHigher());
    }

    /// <summary>
    /// Determines whether the issue list contains any error or more severe issue.
    /// </summary>
    /// <param name="issues">The issue list.</param>
    /// <returns><c>true</c> if the list contains an error, critical, or fatal issue; otherwise, <c>false</c>.</returns>
    public static bool HasErrors(this IReadOnlyList<IssueInfo> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        return issues.Any(issue => issue.Severity.IsErrorOrHigher());
    }

    /// <summary>
    /// Determines whether the issue list contains any critical or fatal issue.
    /// </summary>
    /// <param name="issues">The issue list.</param>
    /// <returns><c>true</c> if the list contains a critical or fatal issue; otherwise, <c>false</c>.</returns>
    public static bool HasCriticalOrFatalIssues(this IReadOnlyList<IssueInfo> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        return issues.Any(issue => issue.Severity.IsCriticalOrHigher());
    }

    /// <summary>
    /// Determines whether the issue list contains an issue with the specified severity.
    /// </summary>
    /// <param name="issues">The issue list.</param>
    /// <param name="severity">The severity to search for.</param>
    /// <returns><c>true</c> if the list contains an issue with the specified severity; otherwise, <c>false</c>.</returns>
    public static bool HasIssueWithSeverity(
        this IReadOnlyList<IssueInfo> issues,
        IssueSeverity severity)
    {
        ArgumentNullException.ThrowIfNull(issues);

        return issues.Any(issue => issue.Severity == severity);
    }
}