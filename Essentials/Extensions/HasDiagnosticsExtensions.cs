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

      return value.Diagnostics?.HasAnyDiagnostics() == true;
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

      return value.Diagnostics?.HasErrorOrHigherDiagnostics() == true;
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

      return value.Diagnostics?.HasWarningOrHigherDiagnostics() == true;
   }

   /// <summary>
   /// Determines whether the object contains at least one diagnostic message
   /// with severity <see cref="IssueSeverity.Critical"/> or higher.
   /// </summary>
   /// <param name="value">The object carrying diagnostics.</param>
   /// <returns><c>true</c> if the object contains a critical or fatal diagnostic; otherwise, <c>false</c>.</returns>
   public static bool HasCriticalOrHigherDiagnostics(this IHasDiagnostics value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Diagnostics?.HasCriticalOrHigherDiagnostics() == true;
   }

   /// <summary>
   /// Gets the highest diagnostic severity attached to the object.
   /// </summary>
   /// <param name="value">The object carrying diagnostics.</param>
   /// <returns>The highest diagnostic severity, or <see cref="IssueSeverity.None"/> when the object has no diagnostics.</returns>
   public static IssueSeverity GetHighestDiagnosticSeverity(this IHasDiagnostics value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Diagnostics?.GetHighestSeverity() ?? IssueSeverity.None;
   }

   /// <summary>
   /// Determines whether the object contains only informational or lower severity diagnostics.
   /// </summary>
   /// <param name="value">The object carrying diagnostics.</param>
   /// <returns><c>true</c> if all diagnostics are informational or lower severity; otherwise, <c>false</c>.</returns>
   public static bool HasOnlyInformationalOrLowerDiagnostics(this IHasDiagnostics value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Diagnostics?.HasOnlyInformationalOrLowerDiagnostics() ?? true;
   }
}