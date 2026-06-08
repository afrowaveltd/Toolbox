using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogCrossValidatorTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenErrorCatalogIsNull()
    {
        ErrorCatalogCrossValidator validator = new();

        ErrorCatalogValidationResult result = validator.Validate(
            null,
            CreateValidOwnerCatalog(),
            CreateValidCodeGroupCatalog(),
            CreateValidCategoryCatalog());

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "CatalogDocumentIsNull");
    }

    [Fact]
    public void Validate_ShouldReportMissingOwnerCatalog()
    {
        ErrorCatalogCrossValidator validator = new();

        ErrorCatalogValidationResult result = validator.Validate(
            CreateValidErrorCatalog(),
            null,
            CreateValidCodeGroupCatalog(),
            CreateValidCategoryCatalog());

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "OwnerCatalogDocumentIsNull");
    }

    [Fact]
    public void Validate_ShouldReportMissingCodeGroupCatalog()
    {
        ErrorCatalogCrossValidator validator = new();

        ErrorCatalogValidationResult result = validator.Validate(
            CreateValidErrorCatalog(),
            CreateValidOwnerCatalog(),
            null,
            CreateValidCategoryCatalog());

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "CodeGroupCatalogDocumentIsNull");
    }

    [Fact]
    public void Validate_ShouldReportMissingCategoryCatalog_AsWarning()
    {
        ErrorCatalogCrossValidator validator = new();

        ErrorCatalogValidationResult result = validator.Validate(
            CreateValidErrorCatalog(),
            CreateValidOwnerCatalog(),
            CreateValidCodeGroupCatalog(),
            null);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "CategoryCatalogDocumentIsNull");
    }

    [Fact]
    public void Validate_ShouldReturnValidResult_ForMatchingCatalogs()
    {
        ErrorCatalogCrossValidator validator = new();

        ErrorCatalogValidationResult result = validator.Validate(
            CreateValidErrorCatalog(),
            CreateValidOwnerCatalog(),
            CreateValidCodeGroupCatalog(),
            CreateValidCategoryCatalog());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldReportUnknownOwner()
    {
        ErrorCatalogCrossValidator validator = new();
        ErrorCatalogDocument errorCatalog = CreateValidErrorCatalog();

        errorCatalog.Errors[0].Owner = "APP";
        errorCatalog.Errors[0].Id = "APP-CFG-0001";

        ErrorCatalogValidationResult result = validator.Validate(
            errorCatalog,
            CreateValidOwnerCatalog(),
            CreateValidCodeGroupCatalog(),
            CreateValidCategoryCatalog());

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "UnknownErrorOwner");
    }

    [Fact]
    public void Validate_ShouldResolveOwnerAlias()
    {
        ErrorCatalogCrossValidator validator = new();
        ErrorCatalogDocument errorCatalog = CreateValidErrorCatalog();

        errorCatalog.Errors[0].Owner = "Afrowave";
        errorCatalog.Errors[0].Id = "Afrowave-CFG-0001";

        ErrorCatalogValidationResult result = validator.Validate(
            errorCatalog,
            CreateValidOwnerCatalog(),
            CreateValidCodeGroupCatalog(),
            CreateValidCategoryCatalog());

        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Issues, issue => issue.Code == "UnknownErrorOwner");
    }

    [Fact]
    public void Validate_ShouldReportUnknownCodeGroup()
    {
        ErrorCatalogCrossValidator validator = new();
        ErrorCatalogDocument errorCatalog = CreateValidErrorCatalog();

        errorCatalog.Errors[0].CodeGroup = "Storage";

        ErrorCatalogValidationResult result = validator.Validate(
            errorCatalog,
            CreateValidOwnerCatalog(),
            CreateValidCodeGroupCatalog(),
            CreateValidCategoryCatalog());

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "UnknownErrorCodeGroup");
    }

    [Fact]
    public void Validate_ShouldReportErrorCodeOutsideOwnerRange()
    {
        ErrorCatalogCrossValidator validator = new();
        ErrorCatalogDocument errorCatalog = CreateValidErrorCatalog();

        errorCatalog.Errors[0].Code = 1_500_000;

        ErrorCatalogValidationResult result = validator.Validate(
            errorCatalog,
            CreateValidOwnerCatalog(),
            CreateValidCodeGroupCatalogForLargeRange(),
            CreateValidCategoryCatalog());

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "ErrorCodeOutsideOwnerRange");
    }

    [Fact]
    public void Validate_ShouldReportErrorCodeOutsideCodeGroupRange()
    {
        ErrorCatalogCrossValidator validator = new();
        ErrorCatalogDocument errorCatalog = CreateValidErrorCatalog();

        errorCatalog.Errors[0].Code = 300_000;

        ErrorCatalogValidationResult result = validator.Validate(
            errorCatalog,
            CreateValidOwnerCatalog(),
            CreateValidCodeGroupCatalog(),
            CreateValidCategoryCatalog());

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "ErrorCodeOutsideCodeGroupRange");
    }

    [Fact]
    public void Validate_ShouldReportCodePrefixMismatch()
    {
        ErrorCatalogCrossValidator validator = new();
        ErrorCatalogDocument errorCatalog = CreateValidErrorCatalog();

        errorCatalog.Errors[0].CodePrefix = "VAL";
        errorCatalog.Errors[0].Id = "AFW-VAL-0001";

        ErrorCatalogValidationResult result = validator.Validate(
            errorCatalog,
            CreateValidOwnerCatalog(),
            CreateValidCodeGroupCatalog(),
            CreateValidCategoryCatalog());

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "ErrorCodePrefixDoesNotMatchCodeGroup");
    }

    [Fact]
    public void Validate_ShouldReportUnknownPrimaryCategory()
    {
        ErrorCatalogCrossValidator validator = new();
        ErrorCatalogDocument errorCatalog = CreateValidErrorCatalog();

        errorCatalog.Errors[0].PrimaryCategory = "Storage";
        errorCatalog.Errors[0].Categories = ["Storage"];

        ErrorCatalogValidationResult result = validator.Validate(
            errorCatalog,
            CreateValidOwnerCatalog(),
            CreateValidCodeGroupCatalog(),
            CreateValidCategoryCatalog());

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "UnknownPrimaryCategory");
    }

    [Fact]
    public void Validate_ShouldResolveCategoryAlias_ForPrimaryCategory()
    {
        ErrorCatalogCrossValidator validator = new();
        ErrorCatalogDocument errorCatalog = CreateValidErrorCatalog();

        errorCatalog.Errors[0].PrimaryCategory = "Settings";
        errorCatalog.Errors[0].Categories = ["Settings"];

        ErrorCatalogValidationResult result = validator.Validate(
            errorCatalog,
            CreateValidOwnerCatalog(),
            CreateValidCodeGroupCatalog(),
            CreateValidCategoryCatalog());

        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Issues, issue => issue.Code == "UnknownPrimaryCategory");
    }

    [Fact]
    public void Validate_ShouldReportUnknownAdditionalCategory_AsWarning()
    {
        ErrorCatalogCrossValidator validator = new();
        ErrorCatalogDocument errorCatalog = CreateValidErrorCatalog();

        errorCatalog.Errors[0].Categories.Add("Storage");

        ErrorCatalogValidationResult result = validator.Validate(
            errorCatalog,
            CreateValidOwnerCatalog(),
            CreateValidCodeGroupCatalog(),
            CreateValidCategoryCatalog());

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "UnknownAdditionalCategory");
    }

    [Fact]
    public void Validate_ShouldReportPrimaryCategoryNotListedInCategories_AsInformation()
    {
        ErrorCatalogCrossValidator validator = new();
        ErrorCatalogDocument errorCatalog = CreateValidErrorCatalog();

        errorCatalog.Errors[0].Categories = ["General"];

        ErrorCatalogValidationResult result = validator.Validate(
            errorCatalog,
            CreateValidOwnerCatalog(),
            CreateValidCodeGroupCatalog(),
            CreateValidCategoryCatalog());

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "PrimaryCategoryNotListedInCategories");
    }

    [Fact]
    public void Validate_ShouldReportUnknownProfileIncludeOwner()
    {
        ErrorCatalogCrossValidator validator = new();
        ErrorProfileCatalogDocument profileCatalog = CreateValidProfileCatalog();

        profileCatalog.Profiles[0].IncludeOwners = ["UNKNOWN"];

        ErrorCatalogValidationResult result = validator.Validate(
            CreateValidErrorCatalog(),
            CreateValidOwnerCatalog(),
            CreateValidCodeGroupCatalog(),
            CreateValidCategoryCatalog(),
            profileCatalog);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "UnknownProfileIncludeOwner");
    }

    [Fact]
    public void Validate_ShouldReportUnknownProfileIncludeCodeGroup()
    {
        ErrorCatalogCrossValidator validator = new();
        ErrorProfileCatalogDocument profileCatalog = CreateValidProfileCatalog();

        profileCatalog.Profiles[0].IncludeCodeGroups = ["UNKNOWN_GROUP"];

        ErrorCatalogValidationResult result = validator.Validate(
            CreateValidErrorCatalog(),
            CreateValidOwnerCatalog(),
            CreateValidCodeGroupCatalog(),
            CreateValidCategoryCatalog(),
            profileCatalog);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "UnknownProfileIncludeCodeGroup");
    }

    [Fact]
    public void Validate_ShouldReportUnknownProfileIncludeCategory()
    {
        ErrorCatalogCrossValidator validator = new();
        ErrorProfileCatalogDocument profileCatalog = CreateValidProfileCatalog();

        profileCatalog.Profiles[0].IncludeCategories = ["UNKNOWN_CATEGORY"];

        ErrorCatalogValidationResult result = validator.Validate(
            CreateValidErrorCatalog(),
            CreateValidOwnerCatalog(),
            CreateValidCodeGroupCatalog(),
            CreateValidCategoryCatalog(),
            profileCatalog);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "UnknownProfileIncludeCategory");
    }

    [Fact]
    public void Validate_ShouldNotReportProfileIssues_WhenProfileReferencesAreKnown()
    {
        ErrorCatalogCrossValidator validator = new();

        ErrorCatalogValidationResult result = validator.Validate(
            CreateValidErrorCatalog(),
            CreateValidOwnerCatalog(),
            CreateValidCodeGroupCatalog(),
            CreateValidCategoryCatalog(),
            CreateValidProfileCatalog());

        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Issues, issue => issue.Code == "UnknownProfileIncludeOwner");
        Assert.DoesNotContain(result.Issues, issue => issue.Code == "UnknownProfileIncludeCodeGroup");
        Assert.DoesNotContain(result.Issues, issue => issue.Code == "UnknownProfileIncludeCategory");
    }

    private static ErrorCatalogDocument CreateValidErrorCatalog()
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

    private static ErrorProfileCatalogDocument CreateValidProfileCatalog()
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
            IncludeOwners = ["AFW"],
            IncludeCodeGroups = ["CONFIGURATION"],
            IncludeCategories = ["CONFIGURATION"],
            IncludeSubcategories = ["RequiredValue"],
            IncludeTags = ["configuration"],
            ExcludeTags = ["internal"]
         }
           ]
        };
    }
    private static ErrorDefinition CreateValidError()
    {
        return new ErrorDefinition
        {
            Id = "AFW-CFG-0001",
            Code = 200_001,
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

    private static ErrorOwnerCatalogDocument CreateValidOwnerCatalog()
    {
        return new ErrorOwnerCatalogDocument
        {
            SchemaVersion = "1.0",
            CatalogId = "test.owners",
            CatalogName = "Test Owners",
            Language = "en",
            Owners =
           [
              new ErrorOwnerDefinition
            {
               Name = "AFW",
               DisplayName = "Afrowave",
               Description = "Built-in Afrowave owner.",
               CodeFrom = 0,
               CodeTo = 999_999,
               IsBuiltIn = true,
               Aliases = ["AFROWAVE"]
            }
           ]
        };
    }

    private static ErrorCodeGroupCatalogDocument CreateValidCodeGroupCatalog()
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
               CodeFrom = 200_000,
               CodeTo = 299_999,
               Description = "Errors related to configuration.",
               DefaultCategories = ["CONFIGURATION"],
               DefaultTags = ["SETTINGS"]
            }
           ]
        };
    }

    private static ErrorCodeGroupCatalogDocument CreateValidCodeGroupCatalogForLargeRange()
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
               CodeFrom = 1_500_000,
               CodeTo = 1_599_999,
               Description = "Errors related to configuration.",
               DefaultCategories = ["CONFIGURATION"],
               DefaultTags = ["SETTINGS"]
            }
           ]
        };
    }

    private static ErrorCategoryCatalogDocument CreateValidCategoryCatalog()
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
               Name = "CONFIGURATION",
               DisplayName = "Configuration",
               Description = "Errors related to configuration.",
               Aliases = ["SETTINGS"],
               ParentCategories = ["GENERAL"],
               DefaultTags = ["CONFIGURATION"]
            },
            new ErrorCategoryDefinition
            {
               Name = "GENERAL",
               DisplayName = "General",
               Description = "General errors."
            }
           ]
        };
    }
}