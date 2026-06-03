using Afrowave.Toolbox.Essentials.Metadata;
using System.Text.Json.Serialization;

namespace Afrowave.Toolbox.WhenItFails.Definitions;

/// <summary>
/// Represents a complete error catalog document.
/// </summary>
/// <remarks>
/// This object is usually loaded from a JSON file. It contains catalog-level
/// information and a list of known error definitions.
/// </remarks>
public sealed class ErrorCatalogDocument
{
   /// <summary>
   /// Gets or sets the catalog schema version.
   /// </summary>
   /// <example>1.0</example>
   [JsonPropertyName("schemaVersion")]
   public string SchemaVersion { get; set; } = "1.0";

   /// <summary>
   /// Gets or sets the stable catalog identifier.
   /// </summary>
   /// <example>afrowave.toolbox.whenitfails.default</example>
   [JsonPropertyName("catalogId")]
   public string CatalogId { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the human-readable catalog name.
   /// </summary>
   /// <example>Afrowave Toolbox Default Error Catalog</example>
   [JsonPropertyName("catalogName")]
   public string CatalogName { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets an optional catalog description.
   /// </summary>
   [JsonPropertyName("description")]
   public string? Description { get; set; }

   /// <summary>
   /// Gets or sets the default language of catalog text values.
   /// </summary>
   /// <remarks>
   /// This does not make WhenItFails a localization system. It only describes
   /// the language of the text stored in this catalog document.
   /// </remarks>
   /// <example>en</example>
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
   /// Gets or sets optional catalog-level tags used for searching and filtering.
   /// </summary>
   [JsonPropertyName("tags")]
   public List<string> Tags { get; set; } = new();

   /// <summary>
   /// Gets or sets optional catalog-level metadata.
   /// </summary>
   [JsonPropertyName("metadata")]
   public MetadataBag Metadata { get; set; } = new();

   /// <summary>
   /// Gets or sets the error definitions contained in this catalog.
   /// </summary>
   [JsonPropertyName("errors")]
   public List<ErrorDefinition> Errors { get; set; } = new();
}