using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for objects carrying a message.
/// </summary>
public static class HasMessageExtensions
{
   /// <summary>
   /// Determines whether the object has a non-empty message.
   /// </summary>
   /// <param name="value">The object carrying a message.</param>
   /// <returns><c>true</c> if the message is not null, empty, or whitespace; otherwise, <c>false</c>.</returns>
   public static bool HasMessage(this IHasMessage value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return !string.IsNullOrWhiteSpace(value.Message);
   }

   /// <summary>
   /// Determines whether the object's message matches the specified message, ignoring case.
   /// </summary>
   /// <param name="value">The object carrying a message.</param>
   /// <param name="message">The message to compare with.</param>
   /// <returns><c>true</c> if both messages are equal ignoring case; otherwise, <c>false</c>.</returns>
   public static bool HasMessage(
       this IHasMessage value,
       string message)
   {
      ArgumentNullException.ThrowIfNull(value);
      ArgumentException.ThrowIfNullOrWhiteSpace(message);

      return string.Equals(
          value.Message,
          message,
          StringComparison.OrdinalIgnoreCase);
   }

   /// <summary>
   /// Determines whether the object's message contains the specified text, ignoring case.
   /// </summary>
   /// <param name="value">The object carrying a message.</param>
   /// <param name="text">The text to search for.</param>
   /// <returns><c>true</c> if the message contains the specified text ignoring case; otherwise, <c>false</c>.</returns>
   public static bool MessageContains(
       this IHasMessage value,
       string text)
   {
      ArgumentNullException.ThrowIfNull(value);
      ArgumentException.ThrowIfNullOrWhiteSpace(text);

      return value.Message.Contains(
          text,
          StringComparison.OrdinalIgnoreCase);
   }
}