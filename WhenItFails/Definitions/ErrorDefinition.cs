using Afrowave.Toolbox.Essentials.Metadata;
using System.Text.Json.Serialization;

namespace Afrowave.Toolbox.WhenItFails.Definitions;

/// <summary>
/// Defines one known error in an error catalog.
/// </summary>
/// <remarks>
/// An error definition describes a stable, reusable error type.
/// It is usually loaded from a JSON catalog file and later used
/// to create runtime error descriptors.
/// </remarks>
public sealed class ErrorDefinition
{
   /// <summary>
   /// Gets or sets the stable human-readable error identifier.
   /// </summary>
   /// <example>CFG-0001</example>
   [JsonPropertyName("id")]
   public string Id { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the stable numeric error code.
   /// </summary>
   /// <example>1001</example>
   [JsonPropertyName("code")]
   public int Code { get; set; }

   /// <summary>
   /// Gets or sets the machine-friendly error name.
   /// </summary>
   /// <example>MissingConfigurationValue</example>
   [JsonPropertyName("name")]
   public string Name { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the logical error category.
   /// </summary>
   /// <example>Configuration</example>
   [JsonPropertyName("category")]
   public string Category { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the short category prefix used in the error identifier.
   /// </summary>
   /// <example>CFG</example>
   [JsonPropertyName("categoryPrefix")]
   public string CategoryPrefix { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the short human-readable title.
   /// </summary>
   /// <example>Missing configuration value</example>
   [JsonPropertyName("title")]
   public string Title { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the default human-readable message.
   /// </summary>
   /// <example>A required configuration value is missing.</example>
   [JsonPropertyName("message")]
   public string Message { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the default severity of the error.
   /// </summary>
   /// <example>Error</example>
   [JsonPropertyName("defaultSeverity")]
   public string DefaultSeverity { get; set; } = "Error";

   /// <summary>
   /// Gets or sets an optional developer-focused hint.
   /// </summary>
   [JsonPropertyName("developerHint")]
   public string? DeveloperHint { get; set; }

   /// <summary>
   /// Gets or sets an optional documentation key or documentation path.
   /// </summary>
   [JsonPropertyName("documentationKey")]
   public string? DocumentationKey { get; set; }

   /// <summary>
   /// Gets or sets optional tags used for filtering, searching and profiles.
   /// </summary>
   [JsonPropertyName("tags")]
   public List<string> Tags { get; set; } = new();

   /// <summary>
   /// Gets or sets optional metadata for advanced scenarios.
   /// </summary>
   [JsonPropertyName("metadata")]
   public MetadataBag Metadata { get; set; } = new();
}