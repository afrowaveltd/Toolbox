using Afrowave.Toolbox.Essentials.Guards;

namespace Afrowave.Toolbox.Essentials.ValueObjects;

/// <summary>
/// Represents a provider name, such as "ollama-local", "libretranslate-main", or "sqlite-default".
/// </summary>
public readonly record struct ProviderName
{
   /// <summary>
   /// Initializes a new instance of the <see cref="ProviderName"/> struct.
   /// </summary>
   /// <param name="value">The provider name value.</param>
   public ProviderName(string value)
   {
      Value = Guard.NotNullOrWhiteSpace(value, nameof(value)).Trim();
   }

   /// <summary>
   /// Gets the provider name value.
   /// </summary>
   public string Value { get; }

   /// <summary>
   /// Returns the provider name value.
   /// </summary>
   /// <returns>The provider name value.</returns>
   public override string ToString()
   {
      return Value;
   }

   /// <summary>
   /// Creates a provider name from a string value.
   /// </summary>
   /// <param name="value">The provider name value.</param>
   /// <returns>The created provider name.</returns>
   public static ProviderName From(string value)
   {
      return new ProviderName(value);
   }

   /// <summary>
   /// Converts a provider name to its string value.
   /// </summary>
   /// <param name="providerName">The provider name.</param>
   public static implicit operator string(ProviderName providerName)
   {
      return providerName.Value;
   }

   /// <summary>
   /// Converts a string value to a provider name.
   /// </summary>
   /// <param name="value">The provider name value.</param>
   public static explicit operator ProviderName(string value)
   {
      return new ProviderName(value);
   }
}