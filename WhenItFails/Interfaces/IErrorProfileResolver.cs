using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Resolves the effective collection of errors selected by an error profile.
/// </summary>
public interface IErrorProfileResolver
{
    /// <summary>
    /// Resolves errors from the specified catalog using the supplied profile.
    /// </summary>
    /// <param name="errorCatalog">
    /// Error catalog containing source error definitions.
    /// </param>
    /// <param name="profile">
    /// Profile describing include and exclude filters.
    /// </param>
    /// <returns>
    /// Errors matching the profile, preserving their original catalog order.
    /// </returns>
    IReadOnlyList<ErrorDefinition> Resolve(
        ErrorCatalogDocument errorCatalog,
        ErrorProfileDefinition profile);
}
