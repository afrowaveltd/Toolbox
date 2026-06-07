using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Catalog;

/// <summary>
/// Contains the loaded catalog state used by WhenItFails services.
/// </summary>
/// <remarks>
/// This context groups together the main searchable error catalog and all supporting catalogs.
/// It is intended to be built once during startup or initialization and then reused
/// by higher-level services such as descriptor factories, profile-aware selectors,
/// or FireError helpers.
/// </remarks>
public sealed class ErrorCatalogContext
{
    /// <summary>
    /// Gets or sets the runtime searchable error catalog.
    /// </summary>
    public IErrorCatalog ErrorCatalog { get; set; } = null!;

    /// <summary>
    /// Gets or sets the normalized error catalog document.
    /// </summary>
    public ErrorCatalogDocument ErrorCatalogDocument { get; set; } = null!;
    /// <summary>
    /// Gets or sets the cross-validation result produced when all catalog documents
    /// were checked together.
    /// </summary>
    /// <remarks>
    /// If the context exists, cross-validation had no blocking errors.
    /// The result may still contain warnings or informational issues useful for
    /// diagnostics, UI, reports or bootstrap output.
    /// </remarks>
    public ErrorCatalogValidationResult CrossValidationResult { get; set; } = null!;

    /// <summary>
    /// Gets or sets the normalized category catalog document.
    /// </summary>
    public ErrorCategoryCatalogDocument CategoryCatalog { get; set; } = null!;

    /// <summary>
    /// Gets or sets the normalized code group catalog document.
    /// </summary>
    public ErrorCodeGroupCatalogDocument CodeGroupCatalog { get; set; } = null!;

    /// <summary>
    /// Gets or sets the normalized owner catalog document.
    /// </summary>
    public ErrorOwnerCatalogDocument OwnerCatalog { get; set; } = null!;

    /// <summary>
    /// Gets or sets the normalized profile catalog document.
    /// </summary>
    public ErrorProfileCatalogDocument ProfileCatalog { get; set; } = null!;
}