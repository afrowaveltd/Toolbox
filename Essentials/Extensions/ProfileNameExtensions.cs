using Afrowave.Toolbox.Essentials.ValueObjects;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for working with <see cref="ProfileName"/> values.
/// </summary>
public static class ProfileNameExtensions
{
   /// <summary>
   /// Determines whether the profile name matches another profile name, ignoring case.
   /// </summary>
   /// <param name="profileName">The profile name.</param>
   /// <param name="other">The other profile name.</param>
   /// <returns><c>true</c> if both profile names are equal ignoring case; otherwise, <c>false</c>.</returns>
   public static bool EqualsIgnoreCase(
       this ProfileName profileName,
       ProfileName other)
   {
      return string.Equals(
          profileName.Value,
          other.Value,
          StringComparison.OrdinalIgnoreCase);
   }

   /// <summary>
   /// Normalizes the profile name to lowercase using invariant culture rules.
   /// </summary>
   /// <param name="profileName">The profile name.</param>
   /// <returns>The normalized profile name.</returns>
   public static ProfileName ToLowerInvariantName(this ProfileName profileName)
   {
      return new ProfileName(profileName.Value.ToLowerInvariant());
   }
}