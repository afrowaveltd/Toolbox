using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for objects carrying issues.
/// </summary>
public static class HasIssuesExtensions
{
   /// <summary>
   /// Determines whether the object contains at least one issue with error or higher severity.
   /// </summary>
   /// <param name="value">The object carrying issues.</param>
   /// <returns><c>true</c> if the object contains an error or more severe issue; otherwise, <c>false</c>.</returns>
   public static bool HasErrors(this IHasIssues value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Issues.HasErrors();
   }

   /// <summary>
   /// Determines whether the object contains at least one issue with warning or higher severity.
   /// </summary>
   /// <param name="value">The object carrying issues.</param>
   /// <returns><c>true</c> if the object contains a warning or more severe issue; otherwise, <c>false</c>.</returns>
   public static bool HasWarningsOrErrors(this IHasIssues value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Issues.HasWarningsOrErrors();
   }

   /// <summary>
   /// Determines whether the object has any issues.
   /// </summary>
   /// <param name="value">The object carrying issues.</param>
   /// <returns><c>true</c> if the object has at least one issue; otherwise, <c>false</c>.</returns>
   public static bool HasAnyIssues(this IHasIssues value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Issues.Count > 0;
   }
}