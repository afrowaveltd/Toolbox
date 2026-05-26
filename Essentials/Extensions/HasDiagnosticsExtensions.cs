using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for objects carrying diagnostics.
/// </summary>
public static class HasDiagnosticsExtensions
{
   /// <summary>
   /// Determines whether the object contains at least one diagnostic message.
   /// </summary>
   /// <param name="value">The object carrying diagnostics.</param>
   /// <returns><c>true</c> if the object contains diagnostics; otherwise, <c>false</c>.</returns>
   public static bool HasAnyDiagnostics(this IHasDiagnostics value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Diagnostics.Count > 0;
   }

   /// <summary>
   /// Determines whether the object contains at least one diagnostic message
   /// with severity <see cref="IssueSeverity.Error"/> or higher.
   /// </summary>
   /// <param name="value">The object carrying diagnostics.</param>
   /// <returns><c>true</c> if the object contains an error or more severe diagnostic; otherwise, <c>false</c>.</returns>
   public static bool HasDiagnosticErrors(this IHasDiagnostics value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Diagnostics.Any(diagnostic => diagnostic.Severity.IsErrorOrHigher());
   }

   /// <summary>
   /// Determines whether the object contains at least one diagnostic message
   /// with severity <see cref="IssueSeverity.Warning"/> or higher.
   /// </summary>
   /// <param name="value">The object carrying diagnostics.</param>
   /// <returns><c>true</c> if the object contains a warning or more severe diagnostic; otherwise, <c>false</c>.</returns>
   public static bool HasDiagnosticWarningsOrErrors(this IHasDiagnostics value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Diagnostics.Any(diagnostic => diagnostic.Severity.IsWarningOrHigher());
   }
}