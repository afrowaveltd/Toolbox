using Afrowave.Toolbox.Essentials.Diagnostics;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for working with <see cref="DiagnosticLocation"/>.
/// </summary>
public static class DiagnosticLocationExtensions
{
   /// <summary>
   /// Determines whether the diagnostic location has a source.
   /// </summary>
   /// <param name="location">The diagnostic location.</param>
   /// <returns><c>true</c> if the location has a source; otherwise, <c>false</c>.</returns>
   public static bool HasSource(this DiagnosticLocation location)
   {
      ArgumentNullException.ThrowIfNull(location);

      return !string.IsNullOrWhiteSpace(location.Source);
   }

   /// <summary>
   /// Determines whether the diagnostic location has a line number.
   /// </summary>
   /// <param name="location">The diagnostic location.</param>
   /// <returns><c>true</c> if the location has a line number; otherwise, <c>false</c>.</returns>
   public static bool HasLine(this DiagnosticLocation location)
   {
      ArgumentNullException.ThrowIfNull(location);

      return location.Line.HasValue;
   }

   /// <summary>
   /// Determines whether the diagnostic location has a column number.
   /// </summary>
   /// <param name="location">The diagnostic location.</param>
   /// <returns><c>true</c> if the location has a column number; otherwise, <c>false</c>.</returns>
   public static bool HasColumn(this DiagnosticLocation location)
   {
      ArgumentNullException.ThrowIfNull(location);

      return location.Column.HasValue;
   }

   /// <summary>
   /// Determines whether the diagnostic location has an absolute offset.
   /// </summary>
   /// <param name="location">The diagnostic location.</param>
   /// <returns><c>true</c> if the location has an offset; otherwise, <c>false</c>.</returns>
   public static bool HasOffset(this DiagnosticLocation location)
   {
      ArgumentNullException.ThrowIfNull(location);

      return location.Offset.HasValue;
   }

   /// <summary>
   /// Determines whether the diagnostic location contains any location information.
   /// </summary>
   /// <param name="location">The diagnostic location.</param>
   /// <returns><c>true</c> if the location contains any information; otherwise, <c>false</c>.</returns>
   public static bool HasAnyLocationInfo(this DiagnosticLocation location)
   {
      ArgumentNullException.ThrowIfNull(location);

      return location.HasSource()
          || location.HasLine()
          || location.HasColumn()
          || location.HasOffset();
   }

   /// <summary>
   /// Determines whether the diagnostic location contains no location information.
   /// </summary>
   /// <param name="location">The diagnostic location.</param>
   /// <returns><c>true</c> if the location is empty; otherwise, <c>false</c>.</returns>
   public static bool IsEmpty(this DiagnosticLocation location)
   {
      ArgumentNullException.ThrowIfNull(location);

      return !location.HasAnyLocationInfo();
   }
}