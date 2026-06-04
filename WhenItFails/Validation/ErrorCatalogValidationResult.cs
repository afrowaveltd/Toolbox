using Afrowave.Toolbox.WhenItFails.Enums;

namespace Afrowave.Toolbox.WhenItFails.Validation;

/// <summary>
/// Represents the result of validating an error catalog document.
/// </summary>
public sealed class ErrorCatalogValidationResult
{
   private readonly List<ErrorCatalogValidationIssue> _issues = new();

   /// <summary>
   /// Gets all validation issues.
   /// </summary>
   public IReadOnlyList<ErrorCatalogValidationIssue> Issues => _issues;

   /// <summary>
   /// Gets a value indicating whether the catalog passed validation.
   /// </summary>
   public bool IsValid => !_issues.Any(issue => issue.Severity == ErrorCatalogValidationSeverity.Error);

   /// <summary>
   /// Adds a validation issue.
   /// </summary>
   /// <param name="issue">Validation issue to add.</param>
   public void AddIssue(ErrorCatalogValidationIssue issue)
   {
      ArgumentNullException.ThrowIfNull(issue);

      _issues.Add(issue);
   }

   /// <summary>
   /// Adds an error validation issue.
   /// </summary>
   public void AddError(
       string code,
       string message,
       string? errorId = null,
       string? errorName = null,
       string? path = null)
   {
      AddIssue(new ErrorCatalogValidationIssue
      {
         Severity = ErrorCatalogValidationSeverity.Error,
         Code = code,
         Message = message,
         ErrorId = errorId,
         ErrorName = errorName,
         Path = path
      });
   }

   /// <summary>
   /// Adds a warning validation issue.
   /// </summary>
   public void AddWarning(
       string code,
       string message,
       string? errorId = null,
       string? errorName = null,
       string? path = null)
   {
      AddIssue(new ErrorCatalogValidationIssue
      {
         Severity = ErrorCatalogValidationSeverity.Warning,
         Code = code,
         Message = message,
         ErrorId = errorId,
         ErrorName = errorName,
         Path = path
      });
   }

   /// <summary>
   /// Adds an informational validation issue.
   /// </summary>
   public void AddInformation(
       string code,
       string message,
       string? errorId = null,
       string? errorName = null,
       string? path = null)
   {
      AddIssue(new ErrorCatalogValidationIssue
      {
         Severity = ErrorCatalogValidationSeverity.Information,
         Code = code,
         Message = message,
         ErrorId = errorId,
         ErrorName = errorName,
         Path = path
      });
   }
}