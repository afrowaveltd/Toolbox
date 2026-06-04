using Afrowave.Toolbox.Essentials.Metadata;
using System.Text.Json.Serialization;

namespace Afrowave.Toolbox.WhenItFails.Definitions;

/// <summary>
/// Defines one owner of error definitions.
/// </summary>
/// <remarks>
/// Owners separate built-in, application, plugin, user or integration error spaces.
/// This helps avoid collisions between official Afrowave errors and project-specific errors.
/// </remarks>
public sealed class ErrorOwnerDefinition
{
   /// <summary>
   /// Gets or sets the normalized owner key.
   /// </summary>
   /// <example>AFW</example>
   [JsonPropertyName("name")]
   public string Name { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the human-readable owner name.
   /// </summary>
   /// <example>Afrowave</example>
   [JsonPropertyName("displayName")]
   public string DisplayName { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets an optional description of this owner.
   /// </summary>
   [JsonPropertyName("description")]
   public string? Description { get; set; }

   /// <summary>
   /// Gets or sets the first numeric code reserved for this owner.
   /// </summary>
   /// <example>0</example>
   [JsonPropertyName("codeFrom")]
   public int CodeFrom { get; set; }

   /// <summary>
   /// Gets or sets the last numeric code reserved for this owner.
   /// </summary>
   /// <example>999999</example>
   [JsonPropertyName("codeTo")]
   public int CodeTo { get; set; }

   /// <summary>
   /// Gets or sets a value indicating whether this owner represents a built-in read-only source.
   /// </summary>
   [JsonPropertyName("isBuiltIn")]
   public bool IsBuiltIn { get; set; }

   /// <summary>
   /// Gets or sets optional aliases that should resolve to this owner.
   /// </summary>
   [JsonPropertyName("aliases")]
   public List<string> Aliases { get; set; } = new();

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