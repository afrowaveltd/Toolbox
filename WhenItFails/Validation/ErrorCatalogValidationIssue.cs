using Afrowave.Toolbox.WhenItFails.Enums;

namespace Afrowave.Toolbox.WhenItFails.Validation;

/// <summary>
/// Represents one validation issue found in an error catalog document.
/// </summary>
public sealed class ErrorCatalogValidationIssue
{
   /// <summary>
   /// Gets or sets validation issue severity.
   /// </summary>
   public ErrorCatalogValidationSeverity Severity { get; set; } = ErrorCatalogValidationSeverity.Error;

   /// <summary>
   /// Gets or sets machine-friendly validation issue code.
   /// </summary>
   /// <example>DuplicateErrorId</example>
   public string Code { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets human-readable validation issue message.
   /// </summary>
   public string Message { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets optional error identifier related to this issue.
   /// </summary>
   /// <example>CFG-0001</example>
   public string? ErrorId { get; set; }

   /// <summary>
   /// Gets or sets optional error name related to this issue.
   /// </summary>
   /// <example>MissingConfigurationValue</example>
   public string? ErrorName { get; set; }

   /// <summary>
   /// Gets or sets optional JSON/property path related to this issue.
   /// </summary>
   /// <example>errors[3].id</example>
   public string? Path { get; set; }
}