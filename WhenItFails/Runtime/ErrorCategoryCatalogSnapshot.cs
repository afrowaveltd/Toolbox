using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Detached, read-only projection of the active category catalog document.
/// </summary>
public sealed class ErrorCategoryCatalogSnapshot
{
    internal ErrorCategoryCatalogSnapshot(ErrorCategoryCatalogDocument source)
    {
        ArgumentNullException.ThrowIfNull(source);

        SchemaVersion = source.SchemaVersion;
        CatalogId = source.CatalogId;
        CatalogName = source.CatalogName;
        Description = source.Description;
        Language = source.Language;
        SourceCatalogId = source.SourceCatalogId;
        SourceCatalogVersion = source.SourceCatalogVersion;
        IsShadowCopy = source.IsShadowCopy;
        Tags = Array.AsReadOnly(source.Tags.ToArray());
        Metadata = ErrorCategoryDefinitionSnapshot.CopyMetadata(source.Metadata);
        Categories = Array.AsReadOnly(
            source.Categories
                .Select(category => new ErrorCategoryDefinitionSnapshot(category))
                .ToArray());
    }

    /// <summary>Gets the captured catalog schema version.</summary>
    public string SchemaVersion { get; }

    /// <summary>Gets the captured catalog identifier.</summary>
    public string CatalogId { get; }

    /// <summary>Gets the captured catalog name.</summary>
    public string CatalogName { get; }

    /// <summary>Gets the captured optional description.</summary>
    public string? Description { get; }

    /// <summary>Gets the captured catalog language.</summary>
    public string Language { get; }

    /// <summary>Gets the captured optional source catalog identifier.</summary>
    public string? SourceCatalogId { get; }

    /// <summary>Gets the captured optional source catalog version.</summary>
    public string? SourceCatalogVersion { get; }

    /// <summary>Gets the captured shadow-copy flag.</summary>
    public bool IsShadowCopy { get; }

    /// <summary>Gets detached, read-only catalog tags.</summary>
    public IReadOnlyList<string> Tags { get; }

    /// <summary>Gets detached, read-only catalog metadata.</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }

    /// <summary>Gets detached, read-only category definitions.</summary>
    public IReadOnlyList<ErrorCategoryDefinitionSnapshot> Categories { get; }
}
