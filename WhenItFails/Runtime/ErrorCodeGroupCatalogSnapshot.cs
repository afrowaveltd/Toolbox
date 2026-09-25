using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>Detached, getter-only projection of the active code group catalog.</summary>
public sealed class ErrorCodeGroupCatalogSnapshot
{
    internal ErrorCodeGroupCatalogSnapshot(ErrorCodeGroupCatalogDocument source)
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
        CodeGroups = Array.AsReadOnly(source.CodeGroups
            .Select(group => new ErrorCodeGroupDefinitionSnapshot(group))
            .ToArray());
    }

    /// <summary>Gets the captured document schema version.</summary>
    public string SchemaVersion { get; }

    /// <summary>Gets the captured catalog identifier.</summary>
    public string CatalogId { get; }

    /// <summary>Gets the captured catalog display name.</summary>
    public string CatalogName { get; }

    /// <summary>Gets the optional captured description.</summary>
    public string? Description { get; }

    /// <summary>Gets the captured language.</summary>
    public string Language { get; }

    /// <summary>Gets the optional source catalog ID.</summary>
    public string? SourceCatalogId { get; }

    /// <summary>Gets the optional source catalog version.</summary>
    public string? SourceCatalogVersion { get; }

    /// <summary>Gets the captured shadow-copy flag.</summary>
    public bool IsShadowCopy { get; }

    /// <summary>Gets independently copied, read-only document tags.</summary>
    public IReadOnlyList<string> Tags { get; }

    /// <summary>Gets independently copied, read-only document metadata.</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }

    /// <summary>Gets independently copied, read-only code group definitions.</summary>
    public IReadOnlyList<ErrorCodeGroupDefinitionSnapshot> CodeGroups { get; }
}
