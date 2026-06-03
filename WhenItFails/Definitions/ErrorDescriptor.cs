using Afrowave.Toolbox.Essentials.Metadata;
using System.Text.Json.Serialization;

namespace Afrowave.Toolbox.WhenItFails.Descriptors;

/// <summary>
/// Represents one concrete runtime error occurrence.
/// </summary>
/// <remarks>
/// An error descriptor is created when a known error definition is used
/// in a real situation. It may contain additional runtime detail,
/// exception information and metadata.
/// </remarks>
public class ErrorDescriptor
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
   /// Gets or sets the human-readable message for this error occurrence.
   /// </summary>
   /// <example>A required configuration value is missing.</example>
   [JsonPropertyName("message")]
   public string Message { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the severity of this concrete error occurrence.
   /// </summary>
   /// <remarks>
   /// The value usually starts as the default severity from the error definition,
   /// but may be changed for a specific runtime situation.
   /// </remarks>
   /// <example>Error</example>
   [JsonPropertyName("severity")]
   public string Severity { get; set; } = "Error";

   /// <summary>
   /// Gets or sets optional runtime detail.
   /// </summary>
   /// <remarks>
   /// This should describe the concrete occurrence, not the general error type.
   /// </remarks>
   [JsonPropertyName("detail")]
   public string? Detail { get; set; }

   /// <summary>
   /// Gets or sets the optional operation name where the error occurred.
   /// </summary>
   /// <example>LoadConfiguration</example>
   [JsonPropertyName("operationName")]
   public string? OperationName { get; set; }

   /// <summary>
   /// Gets or sets the optional component name where the error occurred.
   /// </summary>
   /// <example>JsonConfigurationLoader</example>
   [JsonPropertyName("componentName")]
   public string? ComponentName { get; set; }

   /// <summary>
   /// Gets or sets the optional source name related to this error.
   /// </summary>
   /// <example>appsettings.json</example>
   [JsonPropertyName("sourceName")]
   public string? SourceName { get; set; }

   /// <summary>
   /// Gets or sets optional developer-focused hint.
   /// </summary>
   [JsonPropertyName("developerHint")]
   public string? DeveloperHint { get; set; }

   /// <summary>
   /// Gets or sets an optional documentation key or documentation path.
   /// </summary>
   [JsonPropertyName("documentationKey")]
   public string? DocumentationKey { get; set; }

   /// <summary>
   /// Gets or sets optional tags copied from definition or added at runtime.
   /// </summary>
   [JsonPropertyName("tags")]
   public List<string> Tags { get; set; } = new();

   /// <summary>
   /// Gets or sets optional runtime metadata.
   /// </summary>
   [JsonPropertyName("metadata")]
   public MetadataBag Metadata { get; set; } = new();

   /// <summary>
   /// Gets or sets the original exception related to this error occurrence.
   /// </summary>
   /// <remarks>
   /// This property is ignored during JSON serialization because exceptions
   /// are runtime objects and are usually not safely serializable.
   /// </remarks>
   [JsonIgnore]
   public Exception? Exception { get; set; }
}