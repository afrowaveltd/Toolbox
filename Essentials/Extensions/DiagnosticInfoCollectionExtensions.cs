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
   /// <summary>
   /// Determines whether the collection contains at least one diagnostic message.
   /// </summary>
   /// <param name="diagnostics">The diagnostic collection.</param>
   /// <returns><c>true</c> if the collection contains diagnostics; otherwise, <c>false</c>.</returns>
   public static bool HasAnyDiagnostics(this IEnumerable<DiagnosticInfo> diagnostics)
   {
      ArgumentNullException.ThrowIfNull(diagnostics);

      return diagnostics.Any();
   }
   /// <summary>
   /// Determines whether the collection contains at least one diagnostic message
   /// with severity <see cref="IssueSeverity.Warning"/> or higher.
   /// </summary>
   /// <param name="diagnostics">The diagnostic collection.</param>
   /// <returns><c>true</c> if the collection contains a warning, error, critical, or fatal diagnostic; otherwise, <c>false</c>.</returns>
   public static bool HasWarningOrHigherDiagnostics(this IEnumerable<DiagnosticInfo> diagnostics)
   {
      ArgumentNullException.ThrowIfNull(diagnostics);

      return diagnostics.HasWarningsOrErrors();
   }

   /// <summary>
   /// Determines whether the collection contains at least one diagnostic message
   /// with severity <see cref="IssueSeverity.Error"/> or higher.
   /// </summary>
   /// <param name="diagnostics">The diagnostic collection.</param>
   /// <returns><c>true</c> if the collection contains an error, critical, or fatal diagnostic; otherwise, <c>false</c>.</returns>
   public static bool HasErrorOrHigherDiagnostics(this IEnumerable<DiagnosticInfo> diagnostics)
   {
      ArgumentNullException.ThrowIfNull(diagnostics);

      return diagnostics.HasErrors();
   }

   /// <summary>
   /// Determines whether the collection contains at least one diagnostic message
   /// with severity <see cref="IssueSeverity.Critical"/> or higher.
   /// </summary>
   /// <param name="diagnostics">The diagnostic collection.</param>
   /// <returns><c>true</c> if the collection contains a critical or fatal diagnostic; otherwise, <c>false</c>.</returns>
   public static bool HasCriticalOrHigherDiagnostics(this IEnumerable<DiagnosticInfo> diagnostics)
   {
      ArgumentNullException.ThrowIfNull(diagnostics);

      return diagnostics.Any(diagnostic => diagnostic.Severity.IsCriticalOrHigher());
   }

   /// <summary>
   /// Returns diagnostics with the specified severity.
   /// </summary>
   /// <param name="diagnostics">The diagnostic collection.</param>
   /// <param name="severity">The severity to filter by.</param>
   /// <returns>Diagnostics with the specified severity.</returns>
   public static IEnumerable<DiagnosticInfo> WhereSeverity(
      this IEnumerable<DiagnosticInfo> diagnostics,
      IssueSeverity severity)
   {
      ArgumentNullException.ThrowIfNull(diagnostics);

      return diagnostics.WithSeverity(severity);
   }

   /// <summary>
   /// Returns diagnostics with severity <see cref="IssueSeverity.Warning"/> or higher.
   /// </summary>
   /// <param name="diagnostics">The diagnostic collection.</param>
   /// <returns>Diagnostics with warning or higher severity.</returns>
   public static IEnumerable<DiagnosticInfo> WhereWarningOrHigher(
      this IEnumerable<DiagnosticInfo> diagnostics)
   {
      ArgumentNullException.ThrowIfNull(diagnostics);

      return diagnostics.Where(diagnostic => diagnostic.Severity.IsWarningOrHigher());
   }

   /// <summary>
   /// Returns diagnostics with severity <see cref="IssueSeverity.Error"/> or higher.
   /// </summary>
   /// <param name="diagnostics">The diagnostic collection.</param>
   /// <returns>Diagnostics with error or higher severity.</returns>
   public static IEnumerable<DiagnosticInfo> WhereErrorOrHigher(
      this IEnumerable<DiagnosticInfo> diagnostics)
   {
      ArgumentNullException.ThrowIfNull(diagnostics);

      return diagnostics.Errors();
   }

   /// <summary>
   /// Returns diagnostics with severity <see cref="IssueSeverity.Critical"/> or higher.
   /// </summary>
   /// <param name="diagnostics">The diagnostic collection.</param>
   /// <returns>Diagnostics with critical or fatal severity.</returns>
   public static IEnumerable<DiagnosticInfo> WhereCriticalOrHigher(
      this IEnumerable<DiagnosticInfo> diagnostics)
   {
      ArgumentNullException.ThrowIfNull(diagnostics);

      return diagnostics.Where(diagnostic => diagnostic.Severity.IsCriticalOrHigher());
   }

   /// <summary>
   /// Counts diagnostics with the specified severity.
   /// </summary>
   /// <param name="diagnostics">The diagnostic collection.</param>
   /// <param name="severity">The diagnostic severity.</param>
   /// <returns>The number of diagnostics with the specified severity.</returns>
   public static int CountSeverity(
      this IEnumerable<DiagnosticInfo> diagnostics,
      IssueSeverity severity)
   {
      ArgumentNullException.ThrowIfNull(diagnostics);

      return diagnostics.Count(diagnostic => diagnostic.Severity == severity);
   }

   /// <summary>
   /// Counts diagnostics with severity <see cref="IssueSeverity.Warning"/> or higher.
   /// </summary>
   /// <param name="diagnostics">The diagnostic collection.</param>
   /// <returns>The number of warning, error, critical, and fatal diagnostics.</returns>
   public static int CountWarningOrHigher(this IEnumerable<DiagnosticInfo> diagnostics)
   {
      ArgumentNullException.ThrowIfNull(diagnostics);

      return diagnostics.Count(diagnostic => diagnostic.Severity.IsWarningOrHigher());
   }

   /// <summary>
   /// Counts diagnostics with severity <see cref="IssueSeverity.Error"/> or higher.
   /// </summary>
   /// <param name="diagnostics">The diagnostic collection.</param>
   /// <returns>The number of error, critical, and fatal diagnostics.</returns>
   public static int CountErrorOrHigher(this IEnumerable<DiagnosticInfo> diagnostics)
   {
      ArgumentNullException.ThrowIfNull(diagnostics);

      return diagnostics.Count(diagnostic => diagnostic.Severity.IsErrorOrHigher());
   }

   /// <summary>
   /// Counts diagnostics with severity <see cref="IssueSeverity.Critical"/> or higher.
   /// </summary>
   /// <param name="diagnostics">The diagnostic collection.</param>
   /// <returns>The number of critical and fatal diagnostics.</returns>
   public static int CountCriticalOrHigher(this IEnumerable<DiagnosticInfo> diagnostics)
   {
      ArgumentNullException.ThrowIfNull(diagnostics);

      return diagnostics.Count(diagnostic => diagnostic.Severity.IsCriticalOrHigher());
   }

   /// <summary>
   /// Gets the highest severity from the diagnostic collection.
   /// </summary>
   /// <param name="diagnostics">The diagnostic collection.</param>
   /// <returns>The highest diagnostic severity, or <see cref="IssueSeverity.None"/> when the collection is empty.</returns>
   public static IssueSeverity GetHighestSeverity(this IEnumerable<DiagnosticInfo> diagnostics)
   {
      ArgumentNullException.ThrowIfNull(diagnostics);

      var highestSeverity = IssueSeverity.None;
      var highestRank = GetSeverityRank(highestSeverity);

      foreach(var diagnostic in diagnostics)
      {
         var rank = GetSeverityRank(diagnostic.Severity);

         if(rank > highestRank)
         {
            highestSeverity = diagnostic.Severity;
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
   /// Determines whether the diagnostic collection contains only informational or lower severity diagnostics.
   /// </summary>
   /// <param name="diagnostics">The diagnostic collection.</param>
   /// <returns><c>true</c> if all diagnostics are informational or lower severity; otherwise, <c>false</c>.</returns>
   public static bool HasOnlyInformationalOrLowerDiagnostics(
      this IEnumerable<DiagnosticInfo> diagnostics)
   {
      ArgumentNullException.ThrowIfNull(diagnostics);

      return diagnostics.All(diagnostic =>
         !diagnostic.Severity.IsWarningOrHigher());
   }
}