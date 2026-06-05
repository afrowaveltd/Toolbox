using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Validates error profile catalog documents before they are used.
/// </summary>
public interface IErrorProfileCatalogValidator
{
    /// <summary>
    /// Validates the specified error profile catalog document.
    /// </summary>
    /// <param name="document">Error profile catalog document to validate.</param>
    /// <returns>Validation result containing all discovered issues.</returns>
    ErrorCatalogValidationResult Validate(ErrorProfileCatalogDocument? document);
}