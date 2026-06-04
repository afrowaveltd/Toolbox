using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Validates error owner catalog documents before they are used.
/// </summary>
public interface IErrorOwnerCatalogValidator
{
    /// <summary>
    /// Validates the specified error owner catalog document.
    /// </summary>
    /// <param name="document">Error owner catalog document to validate.</param>
    /// <returns>Validation result containing all discovered issues.</returns>
    ErrorCatalogValidationResult Validate(ErrorOwnerCatalogDocument? document);
}