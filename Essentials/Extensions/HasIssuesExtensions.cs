using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for objects carrying issues.
/// </summary>
public static class HasIssuesExtensions
{
   /// <summary>
   /// Determines whether the object has any issues.
   /// </summary>
   /// <param name="value">The object carrying issues.</param>
   /// <returns><c>true</c> if the object has at least one issue; otherwise, <c>false</c>.</returns>
   public static bool HasAnyIssues(this IHasIssues value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Issues.HasAnyIssues();
   }

   /// <summary>
   /// Determines whether the object contains at least one issue with warning or higher severity.
   /// </summary>
   /// <param name="value">The object carrying issues.</param>
   /// <returns><c>true</c> if the object contains a warning or more severe issue; otherwise, <c>false</c>.</returns>
   public static bool HasWarningsOrErrors(this IHasIssues value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Issues.HasWarningOrHigherIssues();
   }

   /// <summary>
   /// Determines whether the object contains at least one issue with error or higher severity.
   /// </summary>
   /// <param name="value">The object carrying issues.</param>
   /// <returns><c>true</c> if the object contains an error or more severe issue; otherwise, <c>false</c>.</returns>
   public static bool HasErrors(this IHasIssues value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Issues.HasErrorOrHigherIssues();
   }

   /// <summary>
   /// Determines whether the object contains at least one issue with warning or higher severity.
   /// </summary>
   /// <param name="value">The object carrying issues.</param>
   /// <returns><c>true</c> if the object contains a warning, error, critical, or fatal issue; otherwise, <c>false</c>.</returns>
   public static bool HasWarningOrHigherIssues(this IHasIssues value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Issues.HasWarningOrHigherIssues();
   }

   /// <summary>
   /// Determines whether the object contains at least one issue with error or higher severity.
   /// </summary>
   /// <param name="value">The object carrying issues.</param>
   /// <returns><c>true</c> if the object contains an error, critical, or fatal issue; otherwise, <c>false</c>.</returns>
   public static bool HasErrorOrHigherIssues(this IHasIssues value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Issues.HasErrorOrHigherIssues();
   }

   /// <summary>
   /// Determines whether the object contains at least one issue with critical or higher severity.
   /// </summary>
   /// <param name="value">The object carrying issues.</param>
   /// <returns><c>true</c> if the object contains a critical or fatal issue; otherwise, <c>false</c>.</returns>
   public static bool HasCriticalOrHigherIssues(this IHasIssues value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Issues.HasCriticalOrHigherIssues();
   }

   /// <summary>
   /// Determines whether the object contains only informational or lower severity issues.
   /// </summary>
   /// <param name="value">The object carrying issues.</param>
   /// <returns><c>true</c> if all issues are informational or lower severity; otherwise, <c>false</c>.</returns>
   public static bool HasOnlyInformationalOrLowerIssues(this IHasIssues value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Issues.HasOnlyInformationalOrLowerIssues();
   }

   /// <summary>
   /// Gets the highest issue severity attached to the object.
   /// </summary>
   /// <param name="value">The object carrying issues.</param>
   /// <returns>The highest issue severity, or <see cref="IssueSeverity.None"/> when the object has no issues.</returns>
   public static IssueSeverity GetHighestIssueSeverity(this IHasIssues value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Issues.GetHighestSeverity();
   }
}