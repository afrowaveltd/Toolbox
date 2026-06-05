using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Catalog;

/// <summary>
/// Contains data produced by the error owner catalog provider.
/// </summary>
public sealed class ErrorOwnerCatalogProviderPayload
{
   /// <summary>
   /// Gets or sets the normalized error owner catalog document.
   /// </summary>
   public ErrorOwnerCatalogDocument Document { get; set; } = null!;

   /// <summary>
   /// Gets or sets the validation result produced before returning the document.
   /// </summary>
   public ErrorCatalogValidationResult ValidationResult { get; set; } = null!;
}