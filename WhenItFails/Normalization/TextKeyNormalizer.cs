using System.Text;

namespace Afrowave.Toolbox.WhenItFails.Normalization;

/// <summary>
/// Provides Afrowave-style normalization for flexible text keys.
/// </summary>
public static class TextKeyNormalizer
{
   /// <summary>
   /// Normalizes a text value into a stable uppercase key.
   /// </summary>
   /// <remarks>
   /// Rules:
   /// - trims surrounding whitespace,
   /// - converts letters to uppercase invariant,
   /// - replaces every group of non-alphanumeric characters with one underscore,
   /// - removes leading and trailing underscores.
   ///
   /// Examples:
   /// <c>Network error</c> becomes <c>NETWORK_ERROR</c>.
   /// <c>network-error</c> becomes <c>NETWORK_ERROR</c>.
   /// <c>HTTP 404</c> becomes <c>HTTP_404</c>.
   /// </remarks>
   /// <param name="value">Input value.</param>
   /// <returns>Normalized key, or an empty string when input is null or whitespace.</returns>
   public static string NormalizeKey(string? value)
   {
      if(string.IsNullOrWhiteSpace(value))
      {
         return string.Empty;
      }

      string trimmedValue = value.Trim();
      StringBuilder normalizedBuilder = new();
      bool previousCharacterWasSeparator = false;

      foreach(char character in trimmedValue)
      {
         if(char.IsLetterOrDigit(character))
         {
            normalizedBuilder.Append(char.ToUpperInvariant(character));
            previousCharacterWasSeparator = false;
            continue;
         }

         if(!previousCharacterWasSeparator && normalizedBuilder.Length > 0)
         {
            normalizedBuilder.Append('_');
            previousCharacterWasSeparator = true;
         }
      }

      return normalizedBuilder.ToString().Trim('_');
   }

   /// <summary>
   /// Normalizes a display value without changing its casing.
   /// </summary>
   /// <param name="value">Input display value.</param>
   /// <returns>Trimmed display value, or an empty string when input is null or whitespace.</returns>
   public static string NormalizeDisplayName(string? value)
   {
      return string.IsNullOrWhiteSpace(value)
          ? string.Empty
          : value.Trim();
   }
}