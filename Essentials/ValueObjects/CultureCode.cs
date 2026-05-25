using Afrowave.Toolbox.Essentials.Guards;

namespace Afrowave.Toolbox.Essentials.ValueObjects;

/// <summary>
/// Represents a culture code, such as "en", "en-US", "cs", or "cs-CZ".
/// </summary>
public readonly record struct CultureCode
{
   /// <summary>
   /// Initializes a new instance of the <see cref="CultureCode"/> struct.
   /// </summary>
   /// <param name="value">The culture code value.</param>
   public CultureCode(string value)
   {
      Value = Guard.NotNullOrWhiteSpace(value, nameof(value)).Trim();
   }

   /// <summary>
   /// Gets the culture code value.
   /// </summary>
   public string Value { get; }

   /// <summary>
   /// Gets a value indicating whether this culture code is neutral, such as "en" or "cs".
   /// </summary>
   public bool IsNeutral => !Value.Contains('-', StringComparison.Ordinal);

   /// <summary>
   /// Gets a value indicating whether this culture code is specific, such as "en-US" or "cs-CZ".
   /// </summary>
   public bool IsSpecific => Value.Contains('-', StringComparison.Ordinal);

   /// <summary>
   /// Gets the neutral culture part.
   /// </summary>
   /// <returns>The neutral culture part.</returns>
   public string GetNeutralPart()
   {
      var index = Value.IndexOf('-', StringComparison.Ordinal);
      return index < 0 ? Value : Value[..index];
   }

   /// <summary>
   /// Returns the culture code value.
   /// </summary>
   /// <returns>The culture code value.</returns>
   public override string ToString()
   {
      return Value;
   }

   /// <summary>
   /// Creates a culture code from a string value.
   /// </summary>
   /// <param name="value">The culture code value.</param>
   /// <returns>The created culture code.</returns>
   public static CultureCode From(string value)
   {
      return new CultureCode(value);
   }

   /// <summary>
   /// Converts a culture code to its string value.
   /// </summary>
   /// <param name="cultureCode">The culture code.</param>
   public static implicit operator string(CultureCode cultureCode)
   {
      return cultureCode.Value;
   }

   /// <summary>
   /// Converts a string value to a culture code.
   /// </summary>
   /// <param name="value">The culture code value.</param>
   public static explicit operator CultureCode(string value)
   {
      return new CultureCode(value);
   }
}