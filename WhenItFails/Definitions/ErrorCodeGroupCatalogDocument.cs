using System.Text.Json.Serialization;
using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.WhenItFails.Definitions;

/// <summary>
/// Represents a complete catalog document containing error code group definitions.
/// </summary>
/// <remarks>
/// Code group documents define numeric code ranges, prefixes and recommended
/// defaults for groups of errors.
/// </remarks>
public sealed class ErrorCodeGroupCatalogDocument
{
    /// <summary>
    /// Gets or sets the catalog schema version.
    /// </summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; set; } = "1.0";

    /// <summary>
    /// Gets or sets the stable catalog identifier.
    /// </summary>
    [JsonPropertyName("catalogId")]
    public string CatalogId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable catalog name.
    /// </summary>
    [JsonPropertyName("catalogName")]
    public string CatalogName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional catalog description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the default language of human-facing text values.
    /// </summary>
    [JsonPropertyName("language")]
    public string Language { get; set; } = "en";

    /// <summary>
    /// Gets or sets the optional source catalog identifier when this document
    /// was created as a shadow/custom copy.
    /// </summary>
    [JsonPropertyName("sourceCatalogId")]
    public string? SourceCatalogId { get; set; }

    /// <summary>
    /// Gets or sets the optional source catalog version when this document
    /// was created as a shadow/custom copy.
    /// </summary>
    [JsonPropertyName("sourceCatalogVersion")]
    public string? SourceCatalogVersion { get; set; }

    /// <summary>
    /// Gets or sets whether this catalog is intended to be an editable shadow copy.
    /// </summary>
    [JsonPropertyName("isShadowCopy")]
    public bool IsShadowCopy { get; set; }

    /// <summary>
    /// Gets or sets optional catalog-level tags.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Gets or sets optional catalog-level metadata.
    /// </summary>
    [JsonPropertyName("metadata")]
    public MetadataBag Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets code group definitions contained in this catalog.
    /// </summary>
    [JsonPropertyName("codeGroups")]
    public List<ErrorCodeGroupDefinition> CodeGroups { get; set; } = new();
}