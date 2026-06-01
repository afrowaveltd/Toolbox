using Afrowave.Toolbox.Essentials.Diagnostics;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for working with <see cref="DiagnosticSpan"/>.
/// </summary>
public static class DiagnosticSpanExtensions
{
   /// <summary>
   /// Determines whether the diagnostic span has an end location.
   /// </summary>
   /// <param name="span">The diagnostic span.</param>
   /// <returns><c>true</c> if the span has an end location; otherwise, <c>false</c>.</returns>
   public static bool HasEnd(this DiagnosticSpan span)
   {
      ArgumentNullException.ThrowIfNull(span);

      return span.End is not null;
   }

   /// <summary>
   /// Determines whether the diagnostic span has a non-empty label.
   /// </summary>
   /// <param name="span">The diagnostic span.</param>
   /// <returns><c>true</c> if the span has a label; otherwise, <c>false</c>.</returns>
   public static bool HasLabel(this DiagnosticSpan span)
   {
      ArgumentNullException.ThrowIfNull(span);

      return !string.IsNullOrWhiteSpace(span.Label);
   }

   /// <summary>
   /// Determines whether the diagnostic span has any useful span information.
   /// </summary>
   /// <param name="span">The diagnostic span.</param>
   /// <returns><c>true</c> if the span has start information, end information, or a label; otherwise, <c>false</c>.</returns>
   public static bool HasAnySpanInfo(this DiagnosticSpan span)
   {
      ArgumentNullException.ThrowIfNull(span);

      return span.Start?.HasAnyLocationInfo() == true
          || span.End?.HasAnyLocationInfo() == true
          || span.HasLabel();
   }

   /// <summary>
   /// Determines whether the diagnostic span contains no useful span information.
   /// </summary>
   /// <param name="span">The diagnostic span.</param>
   /// <returns><c>true</c> if the span is empty; otherwise, <c>false</c>.</returns>
   public static bool IsEmpty(this DiagnosticSpan span)
   {
      ArgumentNullException.ThrowIfNull(span);

      return !span.HasAnySpanInfo();
   }
}