using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Catalog;

/// <summary>
/// Contains data produced by the error code group catalog provider.
/// </summary>
public sealed class ErrorCodeGroupCatalogProviderPayload
{
   /// <summary>
   /// Gets or sets the normalized error code group catalog document.
   /// </summary>
   public ErrorCodeGroupCatalogDocument Document { get; set; } = null!;

   /// <summary>
   /// Gets or sets the validation result produced before returning the document.
   /// </summary>
   public ErrorCatalogValidationResult ValidationResult { get; set; } = null!;
}