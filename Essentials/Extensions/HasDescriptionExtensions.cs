using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for objects carrying a description.
/// </summary>
public static class HasDescriptionExtensions
{
   /// <summary>
   /// Determines whether the object has a non-empty description.
   /// </summary>
   /// <param name="value">The object carrying a description.</param>
   /// <returns><c>true</c> if the description is not null, empty, or whitespace; otherwise, <c>false</c>.</returns>
   public static bool HasDescription(this IHasDescription value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return !string.IsNullOrWhiteSpace(value.Description);
   }

   /// <summary>
   /// Determines whether the object's description matches the specified text, ignoring case.
   /// </summary>
   /// <param name="value">The object carrying a description.</param>
   /// <param name="description">The description text to compare with.</param>
   /// <returns><c>true</c> if both description values are equal ignoring case; otherwise, <c>false</c>.</returns>
   public static bool HasDescription(
       this IHasDescription value,
       string description)
   {
      ArgumentNullException.ThrowIfNull(value);
      ArgumentException.ThrowIfNullOrWhiteSpace(description);

      return string.Equals(
          value.Description,
          description,
          StringComparison.OrdinalIgnoreCase);
   }

   /// <summary>
   /// Determines whether the object's description contains the specified text, ignoring case.
   /// </summary>
   /// <param name="value">The object carrying a description.</param>
   /// <param name="text">The text to search for.</param>
   /// <returns><c>true</c> if the description contains the specified text ignoring case; otherwise, <c>false</c>.</returns>
   public static bool DescriptionContains(
       this IHasDescription value,
       string text)
   {
      ArgumentNullException.ThrowIfNull(value);
      ArgumentException.ThrowIfNullOrWhiteSpace(text);

      return value.Description?.Contains(
          text,
          StringComparison.OrdinalIgnoreCase) == true;
   }
}