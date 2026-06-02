using Afrowave.Toolbox.Essentials.ValueObjects;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for working with <see cref="ProviderName"/> values.
/// </summary>
public static class ProviderNameExtensions
{
   /// <summary>
   /// Determines whether the provider name matches another provider name, ignoring case.
   /// </summary>
   /// <param name="providerName">The provider name.</param>
   /// <param name="other">The other provider name.</param>
   /// <returns><c>true</c> if both provider names are equal ignoring case; otherwise, <c>false</c>.</returns>
   public static bool EqualsIgnoreCase(
       this ProviderName providerName,
       ProviderName other)
   {
      return string.Equals(
          providerName.Value,
          other.Value,
          StringComparison.OrdinalIgnoreCase);
   }

   /// <summary>
   /// Normalizes the provider name to lowercase using invariant culture rules.
   /// </summary>
   /// <param name="providerName">The provider name.</param>
   /// <returns>The normalized provider name.</returns>
   public static ProviderName ToLowerInvariantName(this ProviderName providerName)
   {
      return string.IsNullOrWhiteSpace(providerName.Value)
         ? providerName
         : new ProviderName(providerName.Value.ToLowerInvariant());
   }
}