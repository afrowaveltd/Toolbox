using System.Text.Json.Serialization;
using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.WhenItFails.Definitions;

/// <summary>
/// Defines one error handling profile.
/// </summary>
/// <remarks>
/// Profiles describe scenario-specific views or behavior recommendations
/// for error catalogs.
///
/// Examples:
/// WEB, API, DESKTOP, CLI, DATABASE, DEVELOPMENT, PRODUCTION.
///
/// A profile should not replace the catalog. It selects, filters or adapts
/// catalog definitions for a specific scenario.
/// </remarks>
public sealed class ErrorProfileDefinition
{
    /// <summary>
    /// Gets or sets the normalized profile key.
    /// </summary>
    /// <example>WEB</example>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable profile name.
    /// </summary>
    /// <example>Web</example>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional profile description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets owners included by this profile.
    /// </summary>
    /// <remarks>
    /// Empty list means no owner filter is applied by this profile.
    /// </remarks>
    [JsonPropertyName("includeOwners")]
    public List<string> IncludeOwners { get; set; } = new();

    /// <summary>
    /// Gets or sets code groups included by this profile.
    /// </summary>
    /// <remarks>
    /// Empty list means no code group filter is applied by this profile.
    /// </remarks>
    [JsonPropertyName("includeCodeGroups")]
    public List<string> IncludeCodeGroups { get; set; } = new();

    /// <summary>
    /// Gets or sets categories included by this profile.
    /// </summary>
    /// <remarks>
    /// Empty list means no category filter is applied by this profile.
    /// </remarks>
    [JsonPropertyName("includeCategories")]
    public List<string> IncludeCategories { get; set; } = new();

    /// <summary>
    /// Gets or sets subcategories included by this profile.
    /// </summary>
    /// <remarks>
    /// Empty list means no subcategory filter is applied by this profile.
    /// </remarks>
    [JsonPropertyName("includeSubcategories")]
    public List<string> IncludeSubcategories { get; set; } = new();

    /// <summary>
    /// Gets or sets tags included by this profile.
    /// </summary>
    /// <remarks>
    /// Empty list means no include-tag filter is applied by this profile.
    /// </remarks>
    [JsonPropertyName("includeTags")]
    public List<string> IncludeTags { get; set; } = new();

    /// <summary>
    /// Gets or sets tags excluded by this profile.
    /// </summary>
    [JsonPropertyName("excludeTags")]
    public List<string> ExcludeTags { get; set; } = new();

    /// <summary>
    /// Gets or sets optional default behavior mappings for this profile.
    /// </summary>
    /// <remarks>
    /// Examples:
    /// web.problemDetails = true
    /// web.includeTraceId = true
    /// production.includeExceptionDetails = false
    /// </remarks>
    [JsonPropertyName("defaultMappings")]
    public Dictionary<string, string> DefaultMappings { get; set; } = new();

    /// <summary>
    /// Gets or sets optional metadata for advanced scenarios.
    /// </summary>
    [JsonPropertyName("metadata")]
    public MetadataBag Metadata { get; set; } = new();
}