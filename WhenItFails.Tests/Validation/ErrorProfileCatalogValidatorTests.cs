using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorProfileCatalogValidatorTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenDocumentIsNull()
    {
        ErrorProfileCatalogValidator validator = new();

        ErrorCatalogValidationResult result = validator.Validate(null);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "ProfileCatalogDocumentIsNull");
    }

    [Fact]
    public void Validate_ShouldReturnValidResult_ForMinimalValidProfileCatalog()
    {
        ErrorProfileCatalogValidator validator = new();

        ErrorProfileCatalogDocument document = CreateValidDocument();

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldReportMissingSchemaVersion()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.SchemaVersion = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingSchemaVersion");
    }

    [Fact]
    public void Validate_ShouldReportMissingCatalogId()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.CatalogId = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingCatalogId");
    }

    [Fact]
    public void Validate_ShouldReportMissingCatalogName_AsWarning()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.CatalogName = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingCatalogName");
    }

    [Fact]
    public void Validate_ShouldReportMissingCatalogLanguage_AsWarning()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.Language = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingCatalogLanguage");
    }

    [Fact]
    public void Validate_ShouldReportEmptyProfileCatalog_AsWarning()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.Profiles.Clear();

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "ProfileCatalogContainsNoProfiles");
    }

    [Fact]
    public void Validate_ShouldReportMissingProfileName()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.Profiles[0].Name = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingProfileName");
    }

    [Fact]
    public void Validate_ShouldReportMissingProfileDisplayName_AsWarning()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.Profiles[0].DisplayName = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingProfileDisplayName");
    }

    [Fact]
    public void Validate_ShouldReportDuplicateProfileName_UsingNormalizedComparison()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.Profiles.Add(new ErrorProfileDefinition
        {
            Name = "web api",
            DisplayName = "Web API duplicate"
        });

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DuplicateProfileName");
    }

    [Fact]
    public void Validate_ShouldReportEmptyIncludeOwner_AsWarning()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.Profiles[0].IncludeOwners = ["", "AFW"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "EmptyIncludeOwner");
    }

    [Fact]
    public void Validate_ShouldReportDuplicateIncludeOwner_AsWarning()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.Profiles[0].IncludeOwners = ["afw", "AFW"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DuplicateIncludeOwner");
    }

    [Fact]
    public void Validate_ShouldReportEmptyIncludeCodeGroup_AsWarning()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.Profiles[0].IncludeCodeGroups = ["", "CONFIGURATION"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "EmptyIncludeCodeGroup");
    }

    [Fact]
    public void Validate_ShouldReportDuplicateIncludeCodeGroup_AsWarning()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.Profiles[0].IncludeCodeGroups = ["configuration", "CONFIGURATION"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DuplicateIncludeCodeGroup");
    }

    [Fact]
    public void Validate_ShouldReportEmptyIncludeCategory_AsWarning()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.Profiles[0].IncludeCategories = ["", "WEB"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "EmptyIncludeCategory");
    }

    [Fact]
    public void Validate_ShouldReportDuplicateIncludeCategory_AsWarning()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.Profiles[0].IncludeCategories = ["web api", "web-api"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DuplicateIncludeCategory");
    }

    [Fact]
    public void Validate_ShouldReportEmptyIncludeSubcategory_AsWarning()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.Profiles[0].IncludeSubcategories = ["", "REQUIRED_VALUE"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "EmptyIncludeSubcategory");
    }

    [Fact]
    public void Validate_ShouldReportDuplicateIncludeSubcategory_AsWarning()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.Profiles[0].IncludeSubcategories = ["required value", "required-value"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DuplicateIncludeSubcategory");
    }

    [Fact]
    public void Validate_ShouldReportEmptyIncludeTag_AsWarning()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.Profiles[0].IncludeTags = ["", "USER_VISIBLE"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "EmptyIncludeTag");
    }

    [Fact]
    public void Validate_ShouldReportDuplicateIncludeTag_AsWarning()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.Profiles[0].IncludeTags = ["user visible", "user-visible"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DuplicateIncludeTag");
    }

    [Fact]
    public void Validate_ShouldReportEmptyExcludeTag_AsWarning()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.Profiles[0].ExcludeTags = ["", "INTERNAL_ONLY"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "EmptyExcludeTag");
    }

    [Fact]
    public void Validate_ShouldReportDuplicateExcludeTag_AsWarning()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.Profiles[0].ExcludeTags = ["internal only", "internal-only"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DuplicateExcludeTag");
    }

    [Fact]
    public void Validate_ShouldReportEmptyIncludeError_AsWarning()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.Profiles[0].IncludeErrors =
        [
            "",
        "AFW-CFG-0001"
        ];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(
            result.Issues,
            issue => issue.Code == "EmptyIncludeError");
    }

    [Fact]
    public void Validate_ShouldReportDuplicateIncludeError_AsWarning()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.Profiles[0].IncludeErrors =
        [
            "afw-cfg-0001",
        "AFW-CFG-0001"
        ];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(
            result.Issues,
            issue => issue.Code == "DuplicateIncludeError");
    }

    [Fact]
    public void Validate_ShouldReportEmptyExcludeError_AsWarning()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.Profiles[0].ExcludeErrors =
        [
            "",
        "AFW-CFG-0002"
        ];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(
            result.Issues,
            issue => issue.Code == "EmptyExcludeError");
    }

    [Fact]
    public void Validate_ShouldReportDuplicateExcludeError_AsWarning()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.Profiles[0].ExcludeErrors =
        [
            "afw-cfg-0002",
        "AFW-CFG-0002"
        ];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(
            result.Issues,
            issue => issue.Code == "DuplicateExcludeError");
    }

    [Fact]
    public void Validate_ShouldReportErrorIncludedAndExcluded_AsWarning()
    {
        ErrorProfileCatalogValidator validator = new();
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.Profiles[0].IncludeErrors =
        [
            "AFW-CFG-0001"
        ];

        document.Profiles[0].ExcludeErrors =
        [
            "afw-cfg-0001"
        ];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);

        Assert.Contains(
            result.Issues,
            issue =>
                issue.Code == "ProfileErrorIncludedAndExcluded"
                && issue.Path == "profiles[0].excludeErrors[0]");
    }

    private static ErrorProfileCatalogDocument CreateValidDocument()
    {
        return new ErrorProfileCatalogDocument
        {
            SchemaVersion = "1.0",
            CatalogId = "test.profiles",
            CatalogName = "Test Profiles",
            Language = "en",
            Profiles =
            [
                new ErrorProfileDefinition
                {
                    Name = "WEB_API",
                    DisplayName = "Web API",
                    Description = "Profile for web APIs.",
                    IncludeOwners = ["AFW", "APP"],
                    IncludeCodeGroups = ["CONFIGURATION", "VALIDATION"],
                    IncludeCategories = ["WEB", "SERVER", "VALIDATION"],
                    IncludeSubcategories = ["REQUIRED_VALUE"],
                    IncludeTags = ["USER_VISIBLE"],
                    ExcludeTags = ["INTERNAL_ONLY"],
                    DefaultMappings =
                    {
                        ["WEB_PROBLEMDETAILS"] = "true",
                        ["PRODUCTION_INCLUDEEXCEPTIONDETAILS"] = "false"
                    }
                }
            ]
        };
    }
}