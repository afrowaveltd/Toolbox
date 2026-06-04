using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Validates error catalog documents before they are used to build runtime catalogs.
/// </summary>
public interface IErrorCatalogValidator
{
   /// <summary>
   /// Validates the specified error catalog document.
   /// </summary>
   /// <param name="document">Error catalog document to validate.</param>
   /// <returns>Validation result containing all discovered issues.</returns>
   ErrorCatalogValidationResult Validate(ErrorCatalogDocument? document);
}