using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Validates error category catalog documents before they are used.
/// </summary>
public interface IErrorCategoryCatalogValidator
{
    /// <summary>
    /// Validates the specified error category catalog document.
    /// </summary>
    /// <param name="document">Error category catalog document to validate.</param>
    /// <returns>Validation result containing all discovered issues.</returns>
    ErrorCatalogValidationResult Validate(ErrorCategoryCatalogDocument? document);
}