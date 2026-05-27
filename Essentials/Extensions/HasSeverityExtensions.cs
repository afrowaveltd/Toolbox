using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for objects carrying a severity level.
/// </summary>
public static class HasSeverityExtensions
{
   /// <summary>
   /// Determines whether the object's severity matches the specified severity.
   /// </summary>
   /// <param name="value">The object carrying a severity level.</param>
   /// <param name="severity">The severity to compare with.</param>
   /// <returns><c>true</c> if the object has the specified severity; otherwise, <c>false</c>.</returns>
   public static bool HasSeverity(
       this IHasSeverity value,
       IssueSeverity severity)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Severity == severity;
   }

   /// <summary>
   /// Determines whether the object's severity represents an error or anything more severe.
   /// </summary>
   /// <param name="value">The object carrying a severity level.</param>
   /// <returns><c>true</c> if the severity is <see cref="IssueSeverity.Error"/> or higher; otherwise, <c>false</c>.</returns>
   public static bool HasErrorOrHigherSeverity(this IHasSeverity value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Severity.IsErrorOrHigher();
   }

   /// <summary>
   /// Determines whether the object's severity represents a warning or anything more severe.
   /// </summary>
   /// <param name="value">The object carrying a severity level.</param>
   /// <returns><c>true</c> if the severity is <see cref="IssueSeverity.Warning"/> or higher; otherwise, <c>false</c>.</returns>
   public static bool HasWarningOrHigherSeverity(this IHasSeverity value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Severity.IsWarningOrHigher();
   }

   /// <summary>
   /// Determines whether the object's severity represents informational output or anything less severe.
   /// </summary>
   /// <param name="value">The object carrying a severity level.</param>
   /// <returns><c>true</c> if the severity is informational or less severe; otherwise, <c>false</c>.</returns>
   public static bool HasInformationOrLowerSeverity(this IHasSeverity value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Severity.IsInformationOrLower();
   }

   /// <summary>
   /// Determines whether the object's severity represents a critical or fatal issue.
   /// </summary>
   /// <param name="value">The object carrying a severity level.</param>
   /// <returns><c>true</c> if the severity is <see cref="IssueSeverity.Critical"/> or higher; otherwise, <c>false</c>.</returns>
   public static bool HasCriticalOrHigherSeverity(this IHasSeverity value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Severity.IsCriticalOrHigher();
   }
}