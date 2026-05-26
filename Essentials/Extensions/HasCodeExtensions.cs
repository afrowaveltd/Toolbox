using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for objects carrying a stable code identifier.
/// </summary>
public static class HasCodeExtensions
{
   /// <summary>
   /// Determines whether the object's code matches the specified code, ignoring case.
   /// </summary>
   /// <param name="value">The object carrying a code.</param>
   /// <param name="code">The code to compare with.</param>
   /// <returns><c>true</c> if both codes are equal ignoring case; otherwise, <c>false</c>.</returns>
   public static bool HasCode(
       this IHasCode value,
       string code)
   {
      ArgumentNullException.ThrowIfNull(value);
      ArgumentException.ThrowIfNullOrWhiteSpace(code);

      return string.Equals(
          value.Code,
          code,
          StringComparison.OrdinalIgnoreCase);
   }

   /// <summary>
   /// Determines whether the object's code matches the code of another object, ignoring case.
   /// </summary>
   /// <param name="value">The object carrying a code.</param>
   /// <param name="other">The other object carrying a code.</param>
   /// <returns><c>true</c> if both codes are equal ignoring case; otherwise, <c>false</c>.</returns>
   public static bool HasSameCodeAs(
       this IHasCode value,
       IHasCode other)
   {
      ArgumentNullException.ThrowIfNull(value);
      ArgumentNullException.ThrowIfNull(other);

      return string.Equals(
          value.Code,
          other.Code,
          StringComparison.OrdinalIgnoreCase);
   }

   /// <summary>
   /// Determines whether the object's code is null, empty, or whitespace.
   /// </summary>
   /// <param name="value">The object carrying a code.</param>
   /// <returns><c>true</c> if the code is null, empty, or whitespace; otherwise, <c>false</c>.</returns>
   public static bool HasEmptyCode(this IHasCode value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return string.IsNullOrWhiteSpace(value.Code);
   }
}