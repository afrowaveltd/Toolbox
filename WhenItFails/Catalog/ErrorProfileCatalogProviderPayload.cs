using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Catalog;

/// <summary>
/// Contains data produced by the error profile catalog provider.
/// </summary>
public sealed class ErrorProfileCatalogProviderPayload
{
    /// <summary>
    /// Gets or sets the normalized error profile catalog document.
    /// </summary>
    public ErrorProfileCatalogDocument Document { get; set; } = null!;

    /// <summary>
    /// Gets or sets the validation result produced before returning the document.
    /// </summary>
    public ErrorCatalogValidationResult ValidationResult { get; set; } = null!;
}