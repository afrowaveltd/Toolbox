using Afrowave.Toolbox.Essentials.Diagnostics;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for working with <see cref="DiagnosticInfo"/>.
/// </summary>
public static class DiagnosticInfoExtensions
{
   /// <summary>
   /// Determines whether the diagnostic has a non-empty code.
   /// </summary>
   /// <param name="diagnostic">The diagnostic information.</param>
   /// <returns><c>true</c> if the diagnostic code is not null, empty, or whitespace; otherwise, <c>false</c>.</returns>
   public static bool HasCode(this DiagnosticInfo diagnostic)
   {
      ArgumentNullException.ThrowIfNull(diagnostic);

      return !string.IsNullOrWhiteSpace(diagnostic.Code);
   }

   /// <summary>
   /// Determines whether the diagnostic has a non-empty message.
   /// </summary>
   /// <param name="diagnostic">The diagnostic information.</param>
   /// <returns><c>true</c> if the diagnostic message is not null, empty, or whitespace; otherwise, <c>false</c>.</returns>
   public static bool HasMessage(this DiagnosticInfo diagnostic)
   {
      ArgumentNullException.ThrowIfNull(diagnostic);

      return !string.IsNullOrWhiteSpace(diagnostic.Message);
   }

   /// <summary>
   /// Determines whether the diagnostic has non-empty details.
   /// </summary>
   /// <param name="diagnostic">The diagnostic information.</param>
   /// <returns><c>true</c> if the diagnostic details are not null, empty, or whitespace; otherwise, <c>false</c>.</returns>
   public static bool HasDetails(this DiagnosticInfo diagnostic)
   {
      ArgumentNullException.ThrowIfNull(diagnostic);

      return !string.IsNullOrWhiteSpace(diagnostic.Details);
   }

   /// <summary>
   /// Determines whether the diagnostic has a primary location.
   /// </summary>
   /// <param name="diagnostic">The diagnostic information.</param>
   /// <returns><c>true</c> if the diagnostic has a location; otherwise, <c>false</c>.</returns>
   public static bool HasLocation(this DiagnosticInfo diagnostic)
   {
      ArgumentNullException.ThrowIfNull(diagnostic);

      return diagnostic.Location is not null;
   }

   /// <summary>
   /// Determines whether the diagnostic has a primary location with useful location information.
   /// </summary>
   /// <param name="diagnostic">The diagnostic information.</param>
   /// <returns><c>true</c> if the diagnostic location contains useful information; otherwise, <c>false</c>.</returns>
   public static bool HasLocationInfo(this DiagnosticInfo diagnostic)
   {
      ArgumentNullException.ThrowIfNull(diagnostic);

      return diagnostic.Location?.HasAnyLocationInfo() == true;
   }

   /// <summary>
   /// Determines whether the diagnostic has any spans.
   /// </summary>
   /// <param name="diagnostic">The diagnostic information.</param>
   /// <returns><c>true</c> if the diagnostic contains at least one span; otherwise, <c>false</c>.</returns>
   public static bool HasSpans(this DiagnosticInfo diagnostic)
   {
      ArgumentNullException.ThrowIfNull(diagnostic);

      return diagnostic.Spans.Count > 0;
   }

   /// <summary>
   /// Determines whether the diagnostic has any spans with useful span information.
   /// </summary>
   /// <param name="diagnostic">The diagnostic information.</param>
   /// <returns><c>true</c> if at least one diagnostic span contains useful information; otherwise, <c>false</c>.</returns>
   public static bool HasSpanInfo(this DiagnosticInfo diagnostic)
   {
      ArgumentNullException.ThrowIfNull(diagnostic);

      return diagnostic.Spans.Any(span => !span.IsEmpty());
   }

   /// <summary>
   /// Determines whether the diagnostic has any hints.
   /// </summary>
   /// <param name="diagnostic">The diagnostic information.</param>
   /// <returns><c>true</c> if the diagnostic contains at least one hint; otherwise, <c>false</c>.</returns>
   public static bool HasHints(this DiagnosticInfo diagnostic)
   {
      ArgumentNullException.ThrowIfNull(diagnostic);

      return diagnostic.Hints.Count > 0;
   }

   /// <summary>
   /// Determines whether the diagnostic has any hints with non-empty messages.
   /// </summary>
   /// <param name="diagnostic">The diagnostic information.</param>
   /// <returns><c>true</c> if at least one diagnostic hint has a message; otherwise, <c>false</c>.</returns>
   public static bool HasHintMessages(this DiagnosticInfo diagnostic)
   {
      ArgumentNullException.ThrowIfNull(diagnostic);

      return diagnostic.Hints.Any(hint => hint.HasMessage());
   }
}