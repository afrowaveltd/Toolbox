using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogValidationResultTests
{
   [Fact]
   public void IsValid_ShouldReturnTrue_WhenThereAreNoIssues()
   {
      ErrorCatalogValidationResult result = new();

      Assert.True(result.IsValid);
      Assert.Empty(result.Issues);
   }

   [Fact]
   public void IsValid_ShouldReturnTrue_WhenThereAreOnlyWarnings()
   {
      ErrorCatalogValidationResult result = new();

      result.AddWarning(
          code: "TestWarning",
          message: "This is only a warning.");

      Assert.True(result.IsValid);
   }

   [Fact]
   public void IsValid_ShouldReturnTrue_WhenThereAreOnlyInformationMessages()
   {
      ErrorCatalogValidationResult result = new();

      result.AddInformation(
          code: "TestInformation",
          message: "This is only information.");

      Assert.True(result.IsValid);
   }

   [Fact]
   public void IsValid_ShouldReturnFalse_WhenThereIsAnError()
   {
      ErrorCatalogValidationResult result = new();

      result.AddError(
          code: "TestError",
          message: "This is an error.");

      Assert.False(result.IsValid);
   }

   [Fact]
   public void AddIssue_ShouldThrowArgumentNullException_WhenIssueIsNull()
   {
      ErrorCatalogValidationResult result = new();

      Assert.Throws<ArgumentNullException>(() => result.AddIssue(null!));
   }

   [Fact]
   public void AddIssue_ShouldPreserveTheExactIssueInstance()
   {
      ErrorCatalogValidationResult result = new();
      ErrorCatalogValidationIssue issue = new()
      {
         Severity = ErrorCatalogValidationSeverity.Warning,
         Code = "PreservedIssue",
         Message = "The exact issue instance should be retained."
      };

      result.AddIssue(issue);

      Assert.Same(issue, Assert.Single(result.Issues));
   }

   [Fact]
   public void Issues_ShouldRemainALiveReadOnlyView()
   {
      ErrorCatalogValidationResult result = new();
      IReadOnlyList<ErrorCatalogValidationIssue> issues = result.Issues;

      result.AddInformation(
          code: "CatalogChecked",
          message: "Catalog was checked.");

      Assert.Same(issues, result.Issues);
      Assert.Single(issues);
      Assert.Equal("CatalogChecked", issues[0].Code);
   }

   [Fact]
   public void AddError_ShouldStoreIssueWithErrorSeverity()
   {
      ErrorCatalogValidationResult result = new();

      result.AddError(
          code: "MissingId",
          message: "Error id is missing.",
          errorId: null,
          errorName: "SomeError",
          path: "errors[0].id");

      ErrorCatalogValidationIssue issue = Assert.Single(result.Issues);

      Assert.Equal(ErrorCatalogValidationSeverity.Error, issue.Severity);
      Assert.Equal("MissingId", issue.Code);
      Assert.Equal("Error id is missing.", issue.Message);
      Assert.Equal("SomeError", issue.ErrorName);
      Assert.Equal("errors[0].id", issue.Path);
   }

   [Fact]
   public void AddWarning_ShouldStoreIssueWithWarningSeverity()
   {
      ErrorCatalogValidationResult result = new();

      result.AddWarning(
          code: "MissingHint",
          message: "Developer hint is missing.");

      ErrorCatalogValidationIssue issue = Assert.Single(result.Issues);

      Assert.Equal(ErrorCatalogValidationSeverity.Warning, issue.Severity);
   }

   [Fact]
   public void AddInformation_ShouldStoreIssueWithInformationSeverity()
   {
      ErrorCatalogValidationResult result = new();

      result.AddInformation(
          code: "CatalogChecked",
          message: "Catalog was checked.");

      ErrorCatalogValidationIssue issue = Assert.Single(result.Issues);

      Assert.Equal(ErrorCatalogValidationSeverity.Information, issue.Severity);
   }
}
