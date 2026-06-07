using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogValidatorTests
{
   [Fact]
   public void Validate_ShouldReturnInvalidResult_WhenDocumentIsNull()
   {
      ErrorCatalogValidator validator = new();

      ErrorCatalogValidationResult result = validator.Validate(null);

      Assert.False(result.IsValid);
      Assert.Contains(result.Issues, issue => issue.Code == "CatalogDocumentIsNull");
   }

   [Fact]
   public void Validate_ShouldReturnValidResult_ForMinimalValidCatalog()
   {
      ErrorCatalogValidator validator = new();

      ErrorCatalogValidationResult result = validator.Validate(CreateValidDocument());

      Assert.True(result.IsValid);
   }

   [Theory]
   [InlineData("SchemaVersion", "MissingSchemaVersion")]
   [InlineData("CatalogId", "MissingCatalogId")]
   public void Validate_ShouldReportMissingRequiredDocumentValue(
       string propertyName,
       string expectedIssueCode)
   {
      ErrorCatalogValidator validator = new();
      ErrorCatalogDocument document = CreateValidDocument();

      typeof(ErrorCatalogDocument).GetProperty(propertyName)!.SetValue(document, string.Empty);

      ErrorCatalogValidationResult result = validator.Validate(document);

      Assert.False(result.IsValid);
      Assert.Contains(result.Issues, issue => issue.Code == expectedIssueCode);
   }

   [Theory]
   [InlineData("CatalogName", "MissingCatalogName")]
   [InlineData("Language", "MissingCatalogLanguage")]
   public void Validate_ShouldReportMissingOptionalDocumentValue_AsWarning(
       string propertyName,
       string expectedIssueCode)
   {
      ErrorCatalogValidator validator = new();
      ErrorCatalogDocument document = CreateValidDocument();

      typeof(ErrorCatalogDocument).GetProperty(propertyName)!.SetValue(document, string.Empty);

      ErrorCatalogValidationResult result = validator.Validate(document);

      Assert.True(result.IsValid);
      Assert.Contains(result.Issues, issue => issue.Code == expectedIssueCode);
   }

   [Fact]
   public void Validate_ShouldReportEmptyCatalog_AsWarning()
   {
      ErrorCatalogValidator validator = new();
      ErrorCatalogDocument document = CreateValidDocument();
      document.Errors.Clear();

      ErrorCatalogValidationResult result = validator.Validate(document);

      Assert.True(result.IsValid);
      Assert.Contains(result.Issues, issue => issue.Code == "CatalogContainsNoErrors");
   }

   [Theory]
   [InlineData("Id", "MissingErrorId")]
   [InlineData("Name", "MissingErrorName")]
   [InlineData("Owner", "MissingErrorOwner")]
   [InlineData("CodePrefix", "MissingErrorCodePrefix")]
   [InlineData("CodeGroup", "MissingErrorCodeGroup")]
   [InlineData("PrimaryCategory", "MissingErrorPrimaryCategory")]
   [InlineData("Message", "MissingErrorMessage")]
   [InlineData("DefaultSeverity", "MissingDefaultSeverity")]
   public void Validate_ShouldReportMissingRequiredErrorValue(
       string propertyName,
       string expectedIssueCode)
   {
      ErrorCatalogValidator validator = new();
      ErrorCatalogDocument document = CreateValidDocument();

      typeof(ErrorDefinition).GetProperty(propertyName)!.SetValue(document.Errors[0], string.Empty);

      ErrorCatalogValidationResult result = validator.Validate(document);

      Assert.False(result.IsValid);
      Assert.Contains(result.Issues, issue => issue.Code == expectedIssueCode);
   }

   [Fact]
   public void Validate_ShouldReportInvalidErrorCode()
   {
      ErrorCatalogValidator validator = new();
      ErrorCatalogDocument document = CreateValidDocument();
      document.Errors[0].Code = 0;

      ErrorCatalogValidationResult result = validator.Validate(document);

      Assert.False(result.IsValid);
      Assert.Contains(result.Issues, issue => issue.Code == "InvalidErrorCode");
   }

   [Fact]
   public void Validate_ShouldReportUnknownDefaultSeverity()
   {
      ErrorCatalogValidator validator = new();
      ErrorCatalogDocument document = CreateValidDocument();
      document.Errors[0].DefaultSeverity = "Unexpected";

      ErrorCatalogValidationResult result = validator.Validate(document);

      Assert.False(result.IsValid);
      Assert.Contains(result.Issues, issue => issue.Code == "UnknownDefaultSeverity");
   }

   [Theory]
   [InlineData("Id", "afw cfg 0001", "DuplicateErrorId")]
   [InlineData("Name", "missingconfigurationvalue", "DuplicateErrorName")]
   public void Validate_ShouldReportDuplicateErrorValue_UsingNormalizedComparison(
       string propertyName,
       string duplicateValue,
       string expectedIssueCode)
   {
      ErrorCatalogValidator validator = new();
      ErrorCatalogDocument document = CreateValidDocument();
      ErrorDefinition duplicate = CreateValidError();
      duplicate.Code = 200002;
      typeof(ErrorDefinition).GetProperty(propertyName)!.SetValue(duplicate, duplicateValue);
      document.Errors.Add(duplicate);

      ErrorCatalogValidationResult result = validator.Validate(document);

      Assert.False(result.IsValid);
      Assert.Contains(result.Issues, issue => issue.Code == expectedIssueCode);
   }

   [Fact]
   public void Validate_ShouldReportDuplicateErrorCode()
   {
      ErrorCatalogValidator validator = new();
      ErrorCatalogDocument document = CreateValidDocument();
      ErrorDefinition duplicate = CreateValidError();
      duplicate.Id = "AFW-CFG-0002";
      duplicate.Name = "AnotherConfigurationError";
      document.Errors.Add(duplicate);

      ErrorCatalogValidationResult result = validator.Validate(document);

      Assert.False(result.IsValid);
      Assert.Contains(result.Issues, issue => issue.Code == "DuplicateErrorCode");
   }

   [Fact]
   public void Validate_ShouldReportErrorIdThatDoesNotMatchOwnerAndPrefix()
   {
      ErrorCatalogValidator validator = new();
      ErrorCatalogDocument document = CreateValidDocument();
      document.Errors[0].Id = "APP-CFG-0001";

      ErrorCatalogValidationResult result = validator.Validate(document);

      Assert.False(result.IsValid);
      Assert.Contains(
          result.Issues,
          issue => issue.Code == "ErrorIdDoesNotMatchOwnerAndCodePrefix");
   }

   [Theory]
   [InlineData("Categories", "DuplicateCategory")]
   [InlineData("Subcategories", "DuplicateSubcategory")]
   [InlineData("Tags", "DuplicateTag")]
   public void Validate_ShouldReportDuplicateCollectionValue_AsWarning(
       string propertyName,
       string expectedIssueCode)
   {
      ErrorCatalogValidator validator = new();
      ErrorCatalogDocument document = CreateValidDocument();

      List<string> values =
          (List<string>)typeof(ErrorDefinition).GetProperty(propertyName)!.GetValue(document.Errors[0])!;
      values.Add(values[0].ToLowerInvariant());

      ErrorCatalogValidationResult result = validator.Validate(document);

      Assert.True(result.IsValid);
      Assert.Contains(result.Issues, issue => issue.Code == expectedIssueCode);
   }

   private static ErrorCatalogDocument CreateValidDocument()
   {
      return new ErrorCatalogDocument
      {
         SchemaVersion = "1.0",
         CatalogId = "test.errors",
         CatalogName = "Test Errors",
         Language = "en",
         Errors = [CreateValidError()]
      };
   }

   private static ErrorDefinition CreateValidError()
   {
      return new ErrorDefinition
      {
         Id = "AFW-CFG-0001",
         Code = 200001,
         Name = "MissingConfigurationValue",
         Owner = "AFW",
         CodePrefix = "CFG",
         CodeGroup = "Configuration",
         PrimaryCategory = "Configuration",
         Categories = ["Configuration"],
         Subcategories = ["RequiredValue"],
         Title = "Missing configuration value",
         Message = "A required configuration value is missing.",
         DefaultSeverity = "Error",
         Tags = ["configuration"]
      };
   }
}
