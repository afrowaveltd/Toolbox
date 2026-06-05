using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Catalog;

/// <summary>
/// Contains data produced by the error category catalog provider.
/// </summary>
public sealed class ErrorCategoryCatalogProviderPayload
{
   /// <summary>
   /// Gets or sets the normalized error category catalog document.
   /// </summary>
   public ErrorCategoryCatalogDocument Document { get; set; } = null!;

   /// <summary>
   /// Gets or sets the validation result produced before returning the document.
   /// </summary>
   public ErrorCatalogValidationResult ValidationResult { get; set; } = null!;
}