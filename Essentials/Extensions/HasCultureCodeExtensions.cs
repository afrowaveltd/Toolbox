using Afrowave.Toolbox.Essentials.Interfaces;
using Afrowave.Toolbox.Essentials.ValueObjects;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for objects carrying a culture code.
/// </summary>
public static class HasCultureCodeExtensions
{
   /// <summary>
   /// Determines whether the object's culture code matches the specified culture code exactly, ignoring case.
   /// </summary>
   /// <param name="value">The object carrying a culture code.</param>
   /// <param name="cultureCode">The culture code to compare with.</param>
   /// <returns><c>true</c> if both culture codes are equal ignoring case; otherwise, <c>false</c>.</returns>
   public static bool HasCultureCode(
       this IHasCultureCode value,
       CultureCode cultureCode)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.CultureCode.EqualsIgnoreCase(cultureCode);
   }

   /// <summary>
   /// Determines whether the object's culture code matches the specified culture code exactly, ignoring case.
   /// </summary>
   /// <param name="value">The object carrying a culture code.</param>
   /// <param name="cultureCode">The culture code string to compare with.</param>
   /// <returns><c>true</c> if both culture codes are equal ignoring case; otherwise, <c>false</c>.</returns>
   public static bool HasCultureCode(
       this IHasCultureCode value,
       string cultureCode)
   {
      ArgumentNullException.ThrowIfNull(value);
      ArgumentException.ThrowIfNullOrWhiteSpace(cultureCode);

      return value.CultureCode.EqualsIgnoreCase(new CultureCode(cultureCode));
   }

   /// <summary>
   /// Determines whether the object's culture code has the same neutral culture part as the specified culture code.
   /// </summary>
   /// <param name="value">The object carrying a culture code.</param>
   /// <param name="cultureCode">The culture code to compare with.</param>
   /// <returns><c>true</c> if both culture codes have the same neutral culture part; otherwise, <c>false</c>.</returns>
   public static bool HasSameNeutralCultureAs(
       this IHasCultureCode value,
       CultureCode cultureCode)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.CultureCode.HasSameNeutralPartAs(cultureCode);
   }

   /// <summary>
   /// Determines whether the object's culture code is neutral, such as "en" or "cs".
   /// </summary>
   /// <param name="value">The object carrying a culture code.</param>
   /// <returns><c>true</c> if the culture code is neutral; otherwise, <c>false</c>.</returns>
   public static bool HasNeutralCultureCode(this IHasCultureCode value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.CultureCode.IsNeutral;
   }

   /// <summary>
   /// Determines whether the object's culture code is specific, such as "en-US" or "cs-CZ".
   /// </summary>
   /// <param name="value">The object carrying a culture code.</param>
   /// <returns><c>true</c> if the culture code is specific; otherwise, <c>false</c>.</returns>
   public static bool HasSpecificCultureCode(this IHasCultureCode value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.CultureCode.IsSpecific;
   }
}