namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Detached projections of three related views selected from one active context reference.
/// </summary>
/// <remarks>
/// Captures main indexed definitions, supporting categories and recorded
/// cross-validation findings. This is not a snapshot of every supporting
/// catalog, a runtime activation-generation identifier or a transaction
/// against external in-place mutation of an already published context.
/// </remarks>
public sealed class ErrorCatalogCombinedSnapshot
{
    internal ErrorCatalogCombinedSnapshot(
        IReadOnlyList<ErrorDefinitionSnapshot> definitions,
        ErrorCategoryCatalogSnapshot categoryCatalog,
        ErrorCatalogValidationSnapshot validation)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(categoryCatalog);
        ArgumentNullException.ThrowIfNull(validation);

        Definitions = definitions;
        CategoryCatalog = categoryCatalog;
        Validation = validation;
    }

    /// <summary>Gets detached main error definitions captured from the selected context.</summary>
    public IReadOnlyList<ErrorDefinitionSnapshot> Definitions { get; }

    /// <summary>Gets the detached supporting category catalog.</summary>
    public ErrorCategoryCatalogSnapshot CategoryCatalog { get; }

    /// <summary>Gets recorded cross-validation findings from the selected context.</summary>
    public ErrorCatalogValidationSnapshot Validation { get; }
}
