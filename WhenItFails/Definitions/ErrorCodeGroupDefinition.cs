using Afrowave.Toolbox.Essentials.Metadata;
using System.Text.Json.Serialization;

namespace Afrowave.Toolbox.WhenItFails.Definitions;

/// <summary>
/// Defines one error code group used for numbering and identity.
/// </summary>
/// <remarks>
/// Code groups describe where an error belongs in the numeric code space.
/// They are not the same thing as categories.
/// 
/// A code group usually has a numeric range and a short code prefix.
/// </remarks>
public sealed class ErrorCodeGroupDefinition
{
   /// <summary>
   /// Gets or sets the normalized code group key.
   /// </summary>
   /// <example>CONFIGURATION</example>
   [JsonPropertyName("name")]
   public string Name { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the human-readable code group name.
   /// </summary>
   /// <example>Configuration</example>
   [JsonPropertyName("displayName")]
   public string DisplayName { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the normalized short code prefix used in error IDs.
   /// </summary>
   /// <example>CFG</example>
   [JsonPropertyName("codePrefix")]
   public string CodePrefix { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the first numeric code reserved for this group.
   /// </summary>
   /// <example>200000</example>
   [JsonPropertyName("codeFrom")]
   public int CodeFrom { get; set; }

   /// <summary>
   /// Gets or sets the last numeric code reserved for this group.
   /// </summary>
   /// <example>299999</example>
   [JsonPropertyName("codeTo")]
   public int CodeTo { get; set; }

   /// <summary>
   /// Gets or sets an optional description of this code group.
   /// </summary>
   [JsonPropertyName("description")]
   public string? Description { get; set; }

   /// <summary>
   /// Gets or sets recommended default categories for this code group.
   /// </summary>
   [JsonPropertyName("defaultCategories")]
   public List<string> DefaultCategories { get; set; } = new();

   /// <summary>
   /// Gets or sets recommended default tags for this code group.
   /// </summary>
   [JsonPropertyName("defaultTags")]
   public List<string> DefaultTags { get; set; } = new();

   /// <summary>
   /// Gets or sets optional default behavior mappings.
   /// </summary>
   [JsonPropertyName("defaultMappings")]
   public Dictionary<string, string> DefaultMappings { get; set; } = new();

   /// <summary>
   /// Gets or sets optional metadata for advanced scenarios.
   /// </summary>
   [JsonPropertyName("metadata")]
   public MetadataBag Metadata { get; set; } = new();
}