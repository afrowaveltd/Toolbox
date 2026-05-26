using Afrowave.Toolbox.Essentials.ValueObjects;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for working with <see cref="CultureCode"/> values.
/// </summary>
public static class CultureCodeExtensions
{
   /// <summary>
   /// Determines whether the culture code has the same neutral culture part as another culture code.
   /// </summary>
   /// <param name="cultureCode">The culture code.</param>
   /// <param name="other">The other culture code.</param>
   /// <returns><c>true</c> if both culture codes have the same neutral part; otherwise, <c>false</c>.</returns>
   public static bool HasSameNeutralPartAs(
       this CultureCode cultureCode,
       CultureCode other)
   {
      return string.Equals(
          cultureCode.GetNeutralPart(),
          other.GetNeutralPart(),
          StringComparison.OrdinalIgnoreCase);
   }

   /// <summary>
   /// Determines whether the culture code matches another culture code exactly, ignoring case.
   /// </summary>
   /// <param name="cultureCode">The culture code.</param>
   /// <param name="other">The other culture code.</param>
   /// <returns><c>true</c> if both culture codes are equal ignoring case; otherwise, <c>false</c>.</returns>
   public static bool EqualsIgnoreCase(
       this CultureCode cultureCode,
       CultureCode other)
   {
      return string.Equals(
          cultureCode.Value,
          other.Value,
          StringComparison.OrdinalIgnoreCase);
   }

   /// <summary>
   /// Normalizes the culture code to lowercase using invariant culture rules.
   /// </summary>
   /// <param name="cultureCode">The culture code.</param>
   /// <returns>The normalized culture code.</returns>
   public static CultureCode ToLowerInvariantCode(this CultureCode cultureCode)
   {
      return new CultureCode(cultureCode.Value.ToLowerInvariant());
   }

   /// <summary>
   /// Gets the parent neutral culture code.
   /// </summary>
   /// <param name="cultureCode">The culture code.</param>
   /// <returns>
   /// The neutral parent culture code when the current culture is specific;
   /// otherwise, the original culture code.
   /// </returns>
   public static CultureCode GetParentOrSelf(this CultureCode cultureCode)
   {
      return cultureCode.IsSpecific
          ? new CultureCode(cultureCode.GetNeutralPart())
          : cultureCode;
   }
}