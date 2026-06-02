using Afrowave.Toolbox.Essentials.Interfaces;
using Afrowave.Toolbox.Essentials.ValueObjects;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for objects carrying a provider name.
/// </summary>
public static class HasProviderNameExtensions
{
   /// <summary>
   /// Determines whether the object's provider name matches the specified provider name, ignoring case.
   /// </summary>
   /// <param name="value">The object carrying a provider name.</param>
   /// <param name="providerName">The provider name to compare with.</param>
   /// <returns><c>true</c> if both provider names are equal ignoring case; otherwise, <c>false</c>.</returns>
   public static bool HasProviderName(
       this IHasProviderName value,
       ProviderName providerName)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.ProviderName.EqualsIgnoreCase(providerName);
   }

   /// <summary>
   /// Determines whether the object's provider name matches the specified provider name, ignoring case.
   /// </summary>
   /// <param name="value">The object carrying a provider name.</param>
   /// <param name="providerName">The provider name string to compare with.</param>
   /// <returns><c>true</c> if both provider names are equal ignoring case; otherwise, <c>false</c>.</returns>
   public static bool HasProviderName(
       this IHasProviderName value,
       string providerName)
   {
      ArgumentNullException.ThrowIfNull(value);
      ArgumentException.ThrowIfNullOrWhiteSpace(providerName);

      return value.ProviderName.EqualsIgnoreCase(new ProviderName(providerName));
   }

   /// <summary>
   /// Gets the provider name normalized to lowercase using invariant culture rules.
   /// </summary>
   /// <param name="value">The object carrying a provider name.</param>
   /// <returns>The normalized provider name.</returns>
   public static ProviderName GetNormalizedProviderName(this IHasProviderName value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.ProviderName.ToLowerInvariantName();
   }

   /// <summary>
   /// Determines whether the object has a provider name.
   /// </summary>
   /// <param name="value">The object carrying a provider name.</param>
   /// <returns><c>true</c> if the object has a provider name; otherwise, <c>false</c>.</returns>
   public static bool HasProviderName(this IHasProviderName value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return !string.IsNullOrWhiteSpace(value.ProviderName.Value);
   }
}