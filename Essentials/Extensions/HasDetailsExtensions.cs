using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for objects carrying detailed information.
/// </summary>
public static class HasDetailsExtensions
{
   /// <summary>
   /// Determines whether the object has non-empty details.
   /// </summary>
   /// <param name="value">The object carrying details.</param>
   /// <returns><c>true</c> if the details are not null, empty, or whitespace; otherwise, <c>false</c>.</returns>
   public static bool HasDetails(this IHasDetails value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return !string.IsNullOrWhiteSpace(value.Details);
   }

   /// <summary>
   /// Determines whether the object's details match the specified text, ignoring case.
   /// </summary>
   /// <param name="value">The object carrying details.</param>
   /// <param name="details">The details text to compare with.</param>
   /// <returns><c>true</c> if both details values are equal ignoring case; otherwise, <c>false</c>.</returns>
   public static bool HasDetails(
       this IHasDetails value,
       string details)
   {
      ArgumentNullException.ThrowIfNull(value);
      ArgumentException.ThrowIfNullOrWhiteSpace(details);

      return string.Equals(
          value.Details,
          details,
          StringComparison.OrdinalIgnoreCase);
   }

   /// <summary>
   /// Determines whether the object's details contain the specified text, ignoring case.
   /// </summary>
   /// <param name="value">The object carrying details.</param>
   /// <param name="text">The text to search for.</param>
   /// <returns><c>true</c> if the details contain the specified text ignoring case; otherwise, <c>false</c>.</returns>
   public static bool DetailsContains(
       this IHasDetails value,
       string text)
   {
      ArgumentNullException.ThrowIfNull(value);
      ArgumentException.ThrowIfNullOrWhiteSpace(text);

      return value.Details?.Contains(
          text,
          StringComparison.OrdinalIgnoreCase) == true;
   }
}