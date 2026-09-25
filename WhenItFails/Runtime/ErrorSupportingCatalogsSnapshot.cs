namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Detached read-only projections of the four supporting catalogs selected
/// from one active context reference.
/// </summary>
/// <remarks>
/// This type does not include main error definitions, recorded validation,
/// runtime status or a store publication identity. It cannot make concurrent
/// in-place mutation of the selected context transactional.
/// </remarks>
public sealed class ErrorSupportingCatalogsSnapshot
{
    internal ErrorSupportingCatalogsSnapshot(
        ErrorCategoryCatalogSnapshot categoryCatalog,
        ErrorOwnerCatalogSnapshot ownerCatalog,
        ErrorCodeGroupCatalogSnapshot codeGroupCatalog,
        ErrorProfileCatalogSnapshot profileCatalog)
    {
        ArgumentNullException.ThrowIfNull(categoryCatalog);
        ArgumentNullException.ThrowIfNull(ownerCatalog);
        ArgumentNullException.ThrowIfNull(codeGroupCatalog);
        ArgumentNullException.ThrowIfNull(profileCatalog);

        CategoryCatalog = categoryCatalog;
        OwnerCatalog = ownerCatalog;
        CodeGroupCatalog = codeGroupCatalog;
        ProfileCatalog = profileCatalog;
    }

    /// <summary>Gets independently copied category catalog data.</summary>
    public ErrorCategoryCatalogSnapshot CategoryCatalog { get; }

    /// <summary>Gets independently copied owner catalog data.</summary>
    public ErrorOwnerCatalogSnapshot OwnerCatalog { get; }

    /// <summary>Gets independently copied code group catalog data.</summary>
    public ErrorCodeGroupCatalogSnapshot CodeGroupCatalog { get; }

    /// <summary>Gets independently copied profile catalog data.</summary>
    public ErrorProfileCatalogSnapshot ProfileCatalog { get; }
}
