using Afrowave.Toolbox.Essentials.Interfaces;
using Afrowave.Toolbox.Essentials.ValueObjects;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for objects implementing <see cref="IHasProfileName"/>.
/// </summary>
public static class HasProfileNameExtensions
{
    /// <summary>
    /// Determines whether the object has a profile name.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <returns><c>true</c> if the object has a profile name; otherwise, <c>false</c>.</returns>
    public static bool HasProfileName(this IHasProfileName source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return !string.IsNullOrWhiteSpace(source.ProfileName.Value);
    }

    /// <summary>
    /// Determines whether the object has the specified profile name.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="profileName">The profile name to compare with.</param>
    /// <returns><c>true</c> if the profile name matches; otherwise, <c>false</c>.</returns>
    public static bool HasProfileName(
     this IHasProfileName source,
     ProfileName profileName)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.ProfileName.EqualsIgnoreCase(profileName);
    }

    /// <summary>
    /// Gets the normalized profile name from the source object.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <returns>The normalized profile name.</returns>
    public static ProfileName GetNormalizedProfileName(this IHasProfileName source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.ProfileName.ToLowerInvariantName();
    }

    /// <summary>
    /// Determines whether the object has the specified profile name.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="profileName">The profile name string to compare with.</param>
    /// <returns><c>true</c> if the profile name matches; otherwise, <c>false</c>.</returns>
    public static bool HasProfileName(
        this IHasProfileName source,
        string profileName)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(profileName);

        return source.ProfileName.EqualsIgnoreCase(new ProfileName(profileName));
    }
}