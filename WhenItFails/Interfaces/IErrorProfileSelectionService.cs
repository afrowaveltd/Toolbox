using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Selects error definitions from a loaded catalog context
/// using a named error profile.
/// </summary>
public interface IErrorProfileSelectionService
{
    /// <summary>
    /// Finds a profile by name and resolves its effective error selection.
    /// </summary>
    /// <param name="context">Loaded error catalog context.</param>
    /// <param name="profileName">Raw or normalized profile name.</param>
    /// <returns>
    /// Response containing errors selected by the requested profile.
    /// </returns>
    Response<IReadOnlyList<ErrorDefinition>> ResolveByProfileName(
        ErrorCatalogContext? context,
        string profileName);
}

