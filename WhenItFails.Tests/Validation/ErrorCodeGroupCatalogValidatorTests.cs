using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCodeGroupCatalogValidatorTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenDocumentIsNull()
    {
        ErrorCodeGroupCatalogValidator validator = new();

        ErrorCatalogValidationResult result = validator.Validate(null);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "CodeGroupCatalogDocumentIsNull");
    }

    [Fact]
    public void Validate_ShouldReturnValidResult_ForMinimalValidCodeGroupCatalog()
    {
        ErrorCodeGroupCatalogValidator validator = new();

        ErrorCodeGroupCatalogDocument document = CreateValidDocument();

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldReportMissingSchemaVersion()
    {
        ErrorCodeGroupCatalogValidator validator = new();
        ErrorCodeGroupCatalogDocument document = CreateValidDocument();

        document.SchemaVersion = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingSchemaVersion");
    }

    [Fact]
    public void Validate_ShouldReportMissingCatalogId()
    {
        ErrorCodeGroupCatalogValidator validator = new();
        ErrorCodeGroupCatalogDocument document = CreateValidDocument();

        document.CatalogId = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingCatalogId");
    }

    [Fact]
    public void Validate_ShouldReportMissingCatalogName_AsWarning()
    {
        ErrorCodeGroupCatalogValidator validator = new();
        ErrorCodeGroupCatalogDocument document = CreateValidDocument();

        document.CatalogName = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingCatalogName");
    }

    [Fact]
    public void Validate_ShouldReportMissingCatalogLanguage_AsWarning()
    {
        ErrorCodeGroupCatalogValidator validator = new();
        ErrorCodeGroupCatalogDocument document = CreateValidDocument();

        document.Language = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingCatalogLanguage");
    }

    [Fact]
    public void Validate_ShouldReportEmptyCodeGroupCatalog_AsWarning()
    {
        ErrorCodeGroupCatalogValidator validator = new();
        ErrorCodeGroupCatalogDocument document = CreateValidDocument();

        document.CodeGroups.Clear();

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "CodeGroupCatalogContainsNoCodeGroups");
    }

    [Fact]
    public void Validate_ShouldReportMissingCodeGroupName()
    {
        ErrorCodeGroupCatalogValidator validator = new();
        ErrorCodeGroupCatalogDocument document = CreateValidDocument();

        document.CodeGroups[0].Name = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingCodeGroupName");
    }

    [Fact]
    public void Validate_ShouldReportMissingCodeGroupDisplayName_AsWarning()
    {
        ErrorCodeGroupCatalogValidator validator = new();
        ErrorCodeGroupCatalogDocument document = CreateValidDocument();

        document.CodeGroups[0].DisplayName = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingCodeGroupDisplayName");
    }

    [Fact]
    public void Validate_ShouldReportMissingCodeGroupPrefix()
    {
        ErrorCodeGroupCatalogValidator validator = new();
        ErrorCodeGroupCatalogDocument document = CreateValidDocument();

        document.CodeGroups[0].CodePrefix = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingCodeGroupPrefix");
    }

    [Fact]
    public void Validate_ShouldReportInvalidCodeGroupCodeFrom()
    {
        ErrorCodeGroupCatalogValidator validator = new();
        ErrorCodeGroupCatalogDocument document = CreateValidDocument();

        document.CodeGroups[0].CodeFrom = 0;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "InvalidCodeGroupCodeFrom");
    }

    [Fact]
    public void Validate_ShouldReportInvalidCodeGroupCodeTo()
    {
        ErrorCodeGroupCatalogValidator validator = new();
        ErrorCodeGroupCatalogDocument document = CreateValidDocument();

        document.CodeGroups[0].CodeTo = 0;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "InvalidCodeGroupCodeTo");
    }

    [Fact]
    public void Validate_ShouldReportInvalidCodeGroupRange()
    {
        ErrorCodeGroupCatalogValidator validator = new();
        ErrorCodeGroupCatalogDocument document = CreateValidDocument();

        document.CodeGroups[0].CodeFrom = 299999;
        document.CodeGroups[0].CodeTo = 200000;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "InvalidCodeGroupRange");
    }

    [Fact]
    public void Validate_ShouldReportDuplicateCodeGroupName_UsingNormalizedComparison()
    {
        ErrorCodeGroupCatalogValidator validator = new();
        ErrorCodeGroupCatalogDocument document = CreateValidDocument();

        document.CodeGroups.Add(new ErrorCodeGroupDefinition
        {
            Name = "configuration",
            DisplayName = "Configuration duplicate",
            CodePrefix = "CFG2",
            CodeFrom = 300000,
            CodeTo = 399999
        });

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DuplicateCodeGroupName");
    }

    [Fact]
    public void Validate_ShouldReportDuplicateCodeGroupPrefix_UsingNormalizedComparison()
    {
        ErrorCodeGroupCatalogValidator validator = new();
        ErrorCodeGroupCatalogDocument document = CreateValidDocument();

        document.CodeGroups.Add(new ErrorCodeGroupDefinition
        {
            Name = "VALIDATION",
            DisplayName = "Validation",
            CodePrefix = "cfg",
            CodeFrom = 300000,
            CodeTo = 399999
        });

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DuplicateCodeGroupPrefix");
    }

    [Fact]
    public void Validate_ShouldReportCodeGroupRangeOverlap()
    {
        ErrorCodeGroupCatalogValidator validator = new();
        ErrorCodeGroupCatalogDocument document = CreateValidDocument();

        document.CodeGroups.Add(new ErrorCodeGroupDefinition
        {
            Name = "VALIDATION",
            DisplayName = "Validation",
            CodePrefix = "VAL",
            CodeFrom = 250000,
            CodeTo = 350000
        });

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "CodeGroupRangeOverlap");
    }

    [Fact]
    public void Validate_ShouldNotReportCodeGroupRangeOverlap_WhenRangesTouchOutside()
    {
        ErrorCodeGroupCatalogValidator validator = new();
        ErrorCodeGroupCatalogDocument document = CreateValidDocument();

        document.CodeGroups.Add(new ErrorCodeGroupDefinition
        {
            Name = "VALIDATION",
            DisplayName = "Validation",
            CodePrefix = "VAL",
            CodeFrom = 300000,
            CodeTo = 399999
        });

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Issues, issue => issue.Code == "CodeGroupRangeOverlap");
    }

    [Fact]
    public void Validate_ShouldReportEmptyDefaultCategory_AsWarning()
    {
        ErrorCodeGroupCatalogValidator validator = new();
        ErrorCodeGroupCatalogDocument document = CreateValidDocument();

        document.CodeGroups[0].DefaultCategories = ["", "CONFIGURATION"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "EmptyDefaultCategory");
    }

    [Fact]
    public void Validate_ShouldReportDuplicateDefaultCategory_AsWarning()
    {
        ErrorCodeGroupCatalogValidator validator = new();
        ErrorCodeGroupCatalogDocument document = CreateValidDocument();

        document.CodeGroups[0].DefaultCategories = ["configuration", "configuration"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DuplicateDefaultCategory");
    }

    [Fact]
    public void Validate_ShouldReportEmptyDefaultTag_AsWarning()
    {
        ErrorCodeGroupCatalogValidator validator = new();
        ErrorCodeGroupCatalogDocument document = CreateValidDocument();

        document.CodeGroups[0].DefaultTags = ["", "SETTINGS"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "EmptyDefaultTag");
    }

    [Fact]
    public void Validate_ShouldReportDuplicateDefaultTag_AsWarning()
    {
        ErrorCodeGroupCatalogValidator validator = new();
        ErrorCodeGroupCatalogDocument document = CreateValidDocument();

        document.CodeGroups[0].DefaultTags = ["user visible", "user-visible"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DuplicateDefaultTag");
    }

    private static ErrorCodeGroupCatalogDocument CreateValidDocument()
    {
        return new ErrorCodeGroupCatalogDocument
        {
            SchemaVersion = "1.0",
            CatalogId = "test.code-groups",
            CatalogName = "Test Code Groups",
            Language = "en",
            CodeGroups =
            [
                new ErrorCodeGroupDefinition
                {
                    Name = "CONFIGURATION",
                    DisplayName = "Configuration",
                    CodePrefix = "CFG",
                    CodeFrom = 200000,
                    CodeTo = 299999,
                    Description = "Errors related to configuration.",
                    DefaultCategories = ["CONFIGURATION"],
                    DefaultTags = ["SETTINGS"]
                }
            ]
        };
    }
}