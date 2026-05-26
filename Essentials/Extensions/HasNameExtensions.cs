using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for objects carrying a name.
/// </summary>
public static class HasNameExtensions
{
   /// <summary>
   /// Determines whether the object's name matches the specified name, ignoring case.
   /// </summary>
   /// <param name="value">The object carrying a name.</param>
   /// <param name="name">The name to compare with.</param>
   /// <returns><c>true</c> if both names are equal ignoring case; otherwise, <c>false</c>.</returns>
   public static bool HasName(
       this IHasName value,
       string name)
   {
      ArgumentNullException.ThrowIfNull(value);
      ArgumentException.ThrowIfNullOrWhiteSpace(name);

      return string.Equals(
          value.Name,
          name,
          StringComparison.OrdinalIgnoreCase);
   }

   /// <summary>
   /// Determines whether the object's name matches the name of another object, ignoring case.
   /// </summary>
   /// <param name="value">The object carrying a name.</param>
   /// <param name="other">The other object carrying a name.</param>
   /// <returns><c>true</c> if both names are equal ignoring case; otherwise, <c>false</c>.</returns>
   public static bool HasSameNameAs(
       this IHasName value,
       IHasName other)
   {
      ArgumentNullException.ThrowIfNull(value);
      ArgumentNullException.ThrowIfNull(other);

      return string.Equals(
          value.Name,
          other.Name,
          StringComparison.OrdinalIgnoreCase);
   }

   /// <summary>
   /// Determines whether the object's name is null, empty, or whitespace.
   /// </summary>
   /// <param name="value">The object carrying a name.</param>
   /// <returns><c>true</c> if the name is null, empty, or whitespace; otherwise, <c>false</c>.</returns>
   public static bool HasEmptyName(this IHasName value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return string.IsNullOrWhiteSpace(value.Name);
   }
}