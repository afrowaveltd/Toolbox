using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for objects carrying an optional numeric identifier.
/// </summary>
public static class HasNumberExtensions
{
   /// <summary>
   /// Determines whether the object has a numeric identifier.
   /// </summary>
   /// <param name="value">The object carrying a numeric identifier.</param>
   /// <returns><c>true</c> if the numeric identifier is set; otherwise, <c>false</c>.</returns>
   public static bool HasNumber(this IHasNumber value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Number.HasValue;
   }

   /// <summary>
   /// Determines whether the object's numeric identifier matches the specified number.
   /// </summary>
   /// <param name="value">The object carrying a numeric identifier.</param>
   /// <param name="number">The number to compare with.</param>
   /// <returns><c>true</c> if the numeric identifier matches the specified number; otherwise, <c>false</c>.</returns>
   public static bool HasNumber(
       this IHasNumber value,
       int number)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.Number == number;
   }

   /// <summary>
   /// Determines whether the object's numeric identifier matches the numeric identifier of another object.
   /// </summary>
   /// <param name="value">The object carrying a numeric identifier.</param>
   /// <param name="other">The other object carrying a numeric identifier.</param>
   /// <returns><c>true</c> if both numeric identifiers are equal; otherwise, <c>false</c>.</returns>
   public static bool HasSameNumberAs(
       this IHasNumber value,
       IHasNumber other)
   {
      ArgumentNullException.ThrowIfNull(value);
      ArgumentNullException.ThrowIfNull(other);

      return value.Number == other.Number;
   }
}