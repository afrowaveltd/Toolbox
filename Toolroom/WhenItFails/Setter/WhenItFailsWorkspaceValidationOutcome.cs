using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter;

/// <summary>
/// Represents validation outcome produced by the Setter tool.
/// </summary>
internal sealed class WhenItFailsWorkspaceValidationOutcome
{
   /// <summary>
   /// Gets or sets the resolved package directory path.
   /// </summary>
   public string PackageDirectoryPath { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the validation result.
   /// </summary>
   public ErrorCatalogValidationResult ValidationResult { get; set; } = new();
}
