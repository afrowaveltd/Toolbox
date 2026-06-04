using Afrowave.Toolbox.Essentials.Metadata;
using System.Text.Json.Serialization;

namespace Afrowave.Toolbox.WhenItFails.Definitions;

/// <summary>
/// Defines one logical error category.
/// </summary>
/// <remarks>
/// Categories behave like a structured tag system. They are used for searching,
/// filtering, profiles, documentation and default behavior recommendations.
/// 
/// The <see cref="Name"/> property should contain a normalized stable key,
/// for example <c>NETWORK</c> or <c>EXTERNAL_SERVICE</c>.
/// 
/// The <see cref="DisplayName"/> property should contain the human-facing name,
/// for example <c>Network</c> or <c>External service</c>.
/// </remarks>
public sealed class ErrorCategoryDefinition
{
   /// <summary>
   /// Gets or sets the normalized category key.
   /// </summary>
   /// <example>NETWORK</example>
   [JsonPropertyName("name")]
   public string Name { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the human-readable category name.
   /// </summary>
   /// <example>Network</example>
   [JsonPropertyName("displayName")]
   public string DisplayName { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets an optional category description.
   /// </summary>
   [JsonPropertyName("description")]
   public string? Description { get; set; }

   /// <summary>
   /// Gets or sets optional aliases that should resolve to this category.
   /// </summary>
   /// <remarks>
   /// Aliases should also be normalized before use.
   /// Example: <c>NETWORKING</c>, <c>COMMUNICATION</c>.
   /// </remarks>
   [JsonPropertyName("aliases")]
   public List<string> Aliases { get; set; } = new();

   /// <summary>
   /// Gets or sets parent categories.
   /// </summary>
   /// <remarks>
   /// Parent categories allow category trees or graphs.
   /// This is intentionally flexible and not limited to one parent.
   /// </remarks>
   [JsonPropertyName("parentCategories")]
   public List<string> ParentCategories { get; set; } = new();

   /// <summary>
   /// Gets or sets default tags recommended for errors in this category.
   /// </summary>
   [JsonPropertyName("defaultTags")]
   public List<string> DefaultTags { get; set; } = new();

   /// <summary>
   /// Gets or sets optional default behavior mappings.
   /// </summary>
   /// <remarks>
   /// These mappings are recommendations, not hard rules.
   /// Examples:
   /// <c>web.httpStatusCode</c> = <c>503</c>,
   /// <c>defaultSeverity</c> = <c>Error</c>.
   /// </remarks>
   [JsonPropertyName("defaultMappings")]
   public Dictionary<string, string> DefaultMappings { get; set; } = new();

   /// <summary>
   /// Gets or sets optional metadata for advanced scenarios.
   /// </summary>
   [JsonPropertyName("metadata")]
   public MetadataBag Metadata { get; set; } = new();
}