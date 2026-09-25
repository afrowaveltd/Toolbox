namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Detached operational view of the main indexed definitions, four supporting
/// catalogs and recorded cross-validation findings from one selected context.
/// </summary>
/// <remarks>
/// Does not expose the live context, raw mutable main JSON document, catalog
/// service implementation, or a transaction against in-place source mutation.
/// </remarks>
public sealed class ErrorCatalogFullSnapshot
{
    internal ErrorCatalogFullSnapshot(
        ErrorCatalogCombinedSnapshot main,
        ErrorSupportingCatalogsSnapshot supporting)
    {
        ArgumentNullException.ThrowIfNull(main);
        ArgumentNullException.ThrowIfNull(supporting);

        if (!ReferenceEquals(main.CategoryCatalog, supporting.CategoryCatalog))
        {
            throw new ArgumentException(
                "Both views must reuse the same detached category projection.",
                nameof(supporting));
        }

        Definitions = main.Definitions;
        CategoryCatalog = main.CategoryCatalog;
        OwnerCatalog = supporting.OwnerCatalog;
        CodeGroupCatalog = supporting.CodeGroupCatalog;
        ProfileCatalog = supporting.ProfileCatalog;
        Validation = main.Validation;
    }

    /// <summary>Gets detached indexed error definitions.</summary>
    public IReadOnlyList<ErrorDefinitionSnapshot> Definitions { get; }

    /// <summary>Gets the detached category catalog.</summary>
    public ErrorCategoryCatalogSnapshot CategoryCatalog { get; }

    /// <summary>Gets the detached owner catalog.</summary>
    public ErrorOwnerCatalogSnapshot OwnerCatalog { get; }

    /// <summary>Gets the detached code-group catalog.</summary>
    public ErrorCodeGroupCatalogSnapshot CodeGroupCatalog { get; }

    /// <summary>Gets the detached profile catalog.</summary>
    public ErrorProfileCatalogSnapshot ProfileCatalog { get; }

    /// <summary>Gets detached recorded cross-validation findings.</summary>
    public ErrorCatalogValidationSnapshot Validation { get; }
}
