using Afrowave.Toolbox.Essentials.Diagnostics;
using Afrowave.Toolbox.Essentials.Enums;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for working with collections of <see cref="DiagnosticInfo"/>.
/// </summary>
public static class DiagnosticInfoCollectionExtensions
{
   /// <summary>
   /// Determines whether the collection contains at least one diagnostic message
   /// with severity <see cref="IssueSeverity.Error"/> or higher.
   /// </summary>
   /// <param name="diagnostics">The diagnostic collection.</param>
   /// <returns><c>true</c> if the collection contains an error or more severe diagnostic; otherwise, <c>false</c>.</returns>
   public static bool HasErrors(this IEnumerable<DiagnosticInfo> diagnostics)
   {
      ArgumentNullException.ThrowIfNull(diagnostics);

      return diagnostics.Any(diagnostic => diagnostic.Severity.IsErrorOrHigher());
   }

   /// <summary>
   /// Determines whether the collection contains at least one diagnostic message
   /// with severity <see cref="IssueSeverity.Warning"/> or higher.
   /// </summary>
   /// <param name="diagnostics">The diagnostic collection.</param>
   /// <returns><c>true</c> if the collection contains a warning or more severe diagnostic; otherwise, <c>false</c>.</returns>
   public static bool HasWarningsOrErrors(this IEnumerable<DiagnosticInfo> diagnostics)
   {
      ArgumentNullException.ThrowIfNull(diagnostics);

      return diagnostics.Any(diagnostic => diagnostic.Severity.IsWarningOrHigher());
   }

   /// <summary>
   /// Returns diagnostics with severity <see cref="IssueSeverity.Error"/> or higher.
   /// </summary>
   /// <param name="diagnostics">The diagnostic collection.</param>
   /// <returns>Diagnostics with error or higher severity.</returns>
   public static IEnumerable<DiagnosticInfo> Errors(this IEnumerable<DiagnosticInfo> diagnostics)
   {
      ArgumentNullException.ThrowIfNull(diagnostics);

      return diagnostics.Where(diagnostic => diagnostic.Severity.IsErrorOrHigher());
   }

   /// <summary>
   /// Returns diagnostics with severity <see cref="IssueSeverity.Warning"/>.
   /// </summary>
   /// <param name="diagnostics">The diagnostic collection.</param>
   /// <returns>Diagnostics with warning severity.</returns>
   public static IEnumerable<DiagnosticInfo> Warnings(this IEnumerable<DiagnosticInfo> diagnostics)
   {
      ArgumentNullException.ThrowIfNull(diagnostics);

      return diagnostics.Where(diagnostic => diagnostic.Severity == IssueSeverity.Warning);
   }

   /// <summary>
   /// Returns diagnostics with severity <see cref="IssueSeverity.Information"/> or lower.
   /// </summary>
   /// <param name="diagnostics">The diagnostic collection.</param>
   /// <returns>Informational, debug, trace, or none-severity diagnostics.</returns>
   public static IEnumerable<DiagnosticInfo> Informational(this IEnumerable<DiagnosticInfo> diagnostics)
   {
      ArgumentNullException.ThrowIfNull(diagnostics);

      return diagnostics.Where(diagnostic => diagnostic.Severity.IsInformationOrLower());
   }

   /// <summary>
   /// Returns diagnostics with the specified severity.
   /// </summary>
   /// <param name="diagnostics">The diagnostic collection.</param>
   /// <param name="severity">The severity to filter by.</param>
   /// <returns>Diagnostics with the specified severity.</returns>
   public static IEnumerable<DiagnosticInfo> WithSeverity(
       this IEnumerable<DiagnosticInfo> diagnostics,
       IssueSeverity severity)
   {
      ArgumentNullException.ThrowIfNull(diagnostics);

      return diagnostics.Where(diagnostic => diagnostic.Severity == severity);
   }
}