using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Validates error code group catalog documents before they are used.
/// </summary>
public interface IErrorCodeGroupCatalogValidator
{
    /// <summary>
    /// Validates the specified error code group catalog document.
    /// </summary>
    /// <param name="document">Error code group catalog document to validate.</param>
    /// <returns>Validation result containing all discovered issues.</returns>
    ErrorCatalogValidationResult Validate(ErrorCodeGroupCatalogDocument? document);
}