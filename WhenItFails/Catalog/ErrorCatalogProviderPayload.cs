using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Catalog;

/// <summary>
/// Contains data produced by the error catalog provider.
/// </summary>
public sealed class ErrorCatalogProviderPayload
{
   /// <summary>
   /// Gets or sets the created runtime error catalog.
   /// </summary>
   public IErrorCatalog Catalog { get; set; } = null!;

   /// <summary>
   /// Gets or sets the validation result produced before catalog creation.
   /// </summary>
   public ErrorCatalogValidationResult ValidationResult { get; set; } = null!;
}