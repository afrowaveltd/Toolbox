using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCategoryCatalogValidatorTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenDocumentIsNull()
    {
        ErrorCategoryCatalogValidator validator = new();

        ErrorCatalogValidationResult result = validator.Validate(null);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "CategoryCatalogDocumentIsNull");
    }

    [Fact]
    public void Validate_ShouldReturnValidResult_ForMinimalValidCategoryCatalog()
    {
        ErrorCategoryCatalogValidator validator = new();

        ErrorCategoryCatalogDocument document = CreateValidDocument();

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldReportMissingSchemaVersion()
    {
        ErrorCategoryCatalogValidator validator = new();
        ErrorCategoryCatalogDocument document = CreateValidDocument();

        document.SchemaVersion = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingSchemaVersion");
    }

    [Fact]
    public void Validate_ShouldReportMissingCatalogId()
    {
        ErrorCategoryCatalogValidator validator = new();
        ErrorCategoryCatalogDocument document = CreateValidDocument();

        document.CatalogId = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingCatalogId");
    }

    [Fact]
    public void Validate_ShouldReportMissingCatalogName_AsWarning()
    {
        ErrorCategoryCatalogValidator validator = new();
        ErrorCategoryCatalogDocument document = CreateValidDocument();

        document.CatalogName = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingCatalogName");
    }

    [Fact]
    public void Validate_ShouldReportMissingCatalogLanguage_AsWarning()
    {
        ErrorCategoryCatalogValidator validator = new();
        ErrorCategoryCatalogDocument document = CreateValidDocument();

        document.Language = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingCatalogLanguage");
    }

    [Fact]
    public void Validate_ShouldReportEmptyCategoryCatalog_AsWarning()
    {
        ErrorCategoryCatalogValidator validator = new();
        ErrorCategoryCatalogDocument document = CreateValidDocument();

        document.Categories.Clear();

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "CategoryCatalogContainsNoCategories");
    }

    [Fact]
    public void Validate_ShouldReportMissingCategoryName()
    {
        ErrorCategoryCatalogValidator validator = new();
        ErrorCategoryCatalogDocument document = CreateValidDocument();

        document.Categories[0].Name = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingCategoryName");
    }

    [Fact]
    public void Validate_ShouldReportMissingCategoryDisplayName_AsWarning()
    {
        ErrorCategoryCatalogValidator validator = new();
        ErrorCategoryCatalogDocument document = CreateValidDocument();

        document.Categories[0].DisplayName = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingCategoryDisplayName");
    }

    [Fact]
    public void Validate_ShouldReportDuplicateCategoryName_UsingNormalizedComparison()
    {
        ErrorCategoryCatalogValidator validator = new();
        ErrorCategoryCatalogDocument document = CreateValidDocument();

        document.Categories.Add(new ErrorCategoryDefinition
        {
            Name = "network",
            DisplayName = "Network duplicate"
        });

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DuplicateCategoryName");
    }

    [Fact]
    public void Validate_ShouldReportEmptyCategoryAlias_AsWarning()
    {
        ErrorCategoryCatalogValidator validator = new();
        ErrorCategoryCatalogDocument document = CreateValidDocument();

        document.Categories[0].Aliases = ["", "networking"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "EmptyCategoryAlias");
    }

    [Fact]
    public void Validate_ShouldReportDuplicateCategoryAlias_AsWarning()
    {
        ErrorCategoryCatalogValidator validator = new();
        ErrorCategoryCatalogDocument document = CreateValidDocument();

        document.Categories[0].Aliases = ["network error", "network-error"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DuplicateCategoryAlias");
    }

    [Fact]
    public void Validate_ShouldReportDuplicateCategoryAliasAcrossCatalog_AsWarning()
    {
        ErrorCategoryCatalogValidator validator = new();
        ErrorCategoryCatalogDocument document = CreateValidDocument();

        document.Categories[0].Aliases = ["communication"];

        document.Categories.Add(new ErrorCategoryDefinition
        {
            Name = "SERVER",
            DisplayName = "Server",
            Aliases = ["communication"]
        });

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DuplicateCategoryAliasAcrossCatalog");
    }

    [Fact]
    public void Validate_ShouldReportCategoryAliasMatchesExistingCategoryName_AsWarning()
    {
        ErrorCategoryCatalogValidator validator = new();
        ErrorCategoryCatalogDocument document = CreateValidDocument();

        document.Categories.Add(new ErrorCategoryDefinition
        {
            Name = "SERVER",
            DisplayName = "Server"
        });

        document.Categories[0].Aliases = ["server"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "CategoryAliasMatchesExistingCategoryName");
    }

    [Fact]
    public void Validate_ShouldReportEmptyParentCategory_AsWarning()
    {
        ErrorCategoryCatalogValidator validator = new();
        ErrorCategoryCatalogDocument document = CreateValidDocument();

        document.Categories[0].ParentCategories = ["", "external communication"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "EmptyParentCategory");
    }

    [Fact]
    public void Validate_ShouldReportDuplicateParentCategory_AsWarning()
    {
        ErrorCategoryCatalogValidator validator = new();
        ErrorCategoryCatalogDocument document = CreateValidDocument();

        document.Categories[0].ParentCategories = ["external communication", "external-communication"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DuplicateParentCategory");
    }

    [Fact]
    public void Validate_ShouldReportEmptyDefaultTag_AsWarning()
    {
        ErrorCategoryCatalogValidator validator = new();
        ErrorCategoryCatalogDocument document = CreateValidDocument();

        document.Categories[0].DefaultTags = ["", "retryable"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "EmptyDefaultTag");
    }

    [Fact]
    public void Validate_ShouldReportDuplicateDefaultTag_AsWarning()
    {
        ErrorCategoryCatalogValidator validator = new();
        ErrorCategoryCatalogDocument document = CreateValidDocument();

        document.Categories[0].DefaultTags = ["retryable candidate", "retryable-candidate"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DuplicateDefaultTag");
    }

    private static ErrorCategoryCatalogDocument CreateValidDocument()
    {
        return new ErrorCategoryCatalogDocument
        {
            SchemaVersion = "1.0",
            CatalogId = "test.categories",
            CatalogName = "Test Categories",
            Language = "en",
            Categories =
            [
                new ErrorCategoryDefinition
                {
                    Name = "NETWORK",
                    DisplayName = "Network",
                    Description = "Errors related to network communication.",
                    Aliases = ["NETWORKING"],
                    ParentCategories = ["EXTERNAL_COMMUNICATION"],
                    DefaultTags = ["RETRYABLE_CANDIDATE"]
                }
            ]
        };
    }
}