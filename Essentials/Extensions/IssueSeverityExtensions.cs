using Afrowave.Toolbox.Essentials.Enums;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for working with <see cref="IssueSeverity"/> values.
/// </summary>
public static class IssueSeverityExtensions
{
   /// <summary>
   /// Determines whether the severity represents an error or anything more severe.
   /// </summary>
   /// <param name="severity">The issue severity.</param>
   /// <returns><c>true</c> if the severity is <see cref="IssueSeverity.Error"/> or higher; otherwise, <c>false</c>.</returns>
   public static bool IsErrorOrHigher(this IssueSeverity severity)
   {
      return severity >= IssueSeverity.Error;
   }

   /// <summary>
   /// Determines whether the severity represents a warning or anything more severe.
   /// </summary>
   /// <param name="severity">The issue severity.</param>
   /// <returns><c>true</c> if the severity is <see cref="IssueSeverity.Warning"/> or higher; otherwise, <c>false</c>.</returns>
   public static bool IsWarningOrHigher(this IssueSeverity severity)
   {
      return severity >= IssueSeverity.Warning;
   }

   /// <summary>
   /// Determines whether the severity represents informational output or anything less severe.
   /// </summary>
   /// <param name="severity">The issue severity.</param>
   /// <returns><c>true</c> if the severity is informational or less severe; otherwise, <c>false</c>.</returns>
   public static bool IsInformationOrLower(this IssueSeverity severity)
   {
      return severity <= IssueSeverity.Information;
   }

   /// <summary>
   /// Determines whether the severity represents a critical or fatal issue.
   /// </summary>
   /// <param name="severity">The issue severity.</param>
   /// <returns><c>true</c> if the severity is <see cref="IssueSeverity.Critical"/> or <see cref="IssueSeverity.Fatal"/>; otherwise, <c>false</c>.</returns>
   public static bool IsCriticalOrHigher(this IssueSeverity severity)
   {
      return severity >= IssueSeverity.Critical;
   }
}