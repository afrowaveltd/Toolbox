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
///
/// Developers normally do not create full error definitions manually
/// in everyday application code. They usually use presets or factory
/// methods such as FireError(...), while the catalog provides this
/// structured information.
/// </remarks>
public sealed class ErrorDefinition
{
   /// <summary>
   /// Gets or sets the stable human-readable error identifier.
   /// </summary>
   /// <remarks>
   /// Recommended format: OWNER-PREFIX-NNNN.
   /// Example: AFW-CFG-0001.
   /// </remarks>
   [JsonPropertyName("id")]
   public string Id { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the stable numeric error code.
   /// </summary>
   /// <remarks>
   /// Numeric codes should be assigned from documented code ranges.
   /// Built-in Afrowave errors and project/user errors should use
   /// separate ranges to avoid collisions.
   /// </remarks>
   [JsonPropertyName("code")]
   public int Code { get; set; }

   /// <summary>
   /// Gets or sets the machine-friendly error name.
   /// </summary>
   /// <remarks>
   /// This name is intended for developers and code-level references.
   /// Example: MissingConfigurationValue.
   /// </remarks>
   [JsonPropertyName("name")]
   public string Name { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the owner of this error definition.
   /// </summary>
   /// <remarks>
   /// Examples: Afrowave, Application, Plugin, Customer.
   /// </remarks>
   [JsonPropertyName("owner")]
   public string Owner { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the short code prefix used in the error identifier.
   /// </summary>
   /// <remarks>
   /// This is not the same thing as a category. It describes the
   /// identity/code family of the error.
   /// Examples: GEN, CFG, VAL, NET, DB.
   /// </remarks>
   [JsonPropertyName("codePrefix")]
   public string CodePrefix { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the logical code group used for numbering and identity.
   /// </summary>
   /// <remarks>
   /// Examples: General, Configuration, Validation, Network, Database.
   /// </remarks>
   [JsonPropertyName("codeGroup")]
   public string CodeGroup { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the primary logical category of the error.
   /// </summary>
   /// <remarks>
   /// The primary category is the most important human-facing category.
   /// The error may also belong to additional categories.
   /// </remarks>
   [JsonPropertyName("primaryCategory")]
   public string PrimaryCategory { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets additional logical categories.
   /// </summary>
   /// <remarks>
   /// Categories behave similarly to a structured tag system.
   /// One error may belong to multiple categories.
   /// </remarks>
   [JsonPropertyName("categories")]
   public List<string> Categories { get; set; } = new();

   /// <summary>
   /// Gets or sets more detailed subcategories.
   /// </summary>
   [JsonPropertyName("subcategories")]
   public List<string> Subcategories { get; set; } = new();

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