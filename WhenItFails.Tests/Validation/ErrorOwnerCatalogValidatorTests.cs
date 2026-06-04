using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorOwnerCatalogValidatorTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenDocumentIsNull()
    {
        ErrorOwnerCatalogValidator validator = new();

        ErrorCatalogValidationResult result = validator.Validate(null);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "OwnerCatalogDocumentIsNull");
    }

    [Fact]
    public void Validate_ShouldReturnValidResult_ForMinimalValidOwnerCatalog()
    {
        ErrorOwnerCatalogValidator validator = new();

        ErrorOwnerCatalogDocument document = CreateValidDocument();

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldReportMissingSchemaVersion()
    {
        ErrorOwnerCatalogValidator validator = new();
        ErrorOwnerCatalogDocument document = CreateValidDocument();

        document.SchemaVersion = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingSchemaVersion");
    }

    [Fact]
    public void Validate_ShouldReportMissingCatalogId()
    {
        ErrorOwnerCatalogValidator validator = new();
        ErrorOwnerCatalogDocument document = CreateValidDocument();

        document.CatalogId = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingCatalogId");
    }

    [Fact]
    public void Validate_ShouldReportMissingCatalogName_AsWarning()
    {
        ErrorOwnerCatalogValidator validator = new();
        ErrorOwnerCatalogDocument document = CreateValidDocument();

        document.CatalogName = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingCatalogName");
    }

    [Fact]
    public void Validate_ShouldReportMissingCatalogLanguage_AsWarning()
    {
        ErrorOwnerCatalogValidator validator = new();
        ErrorOwnerCatalogDocument document = CreateValidDocument();

        document.Language = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingCatalogLanguage");
    }

    [Fact]
    public void Validate_ShouldReportEmptyOwnerCatalog_AsWarning()
    {
        ErrorOwnerCatalogValidator validator = new();
        ErrorOwnerCatalogDocument document = CreateValidDocument();

        document.Owners.Clear();

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "OwnerCatalogContainsNoOwners");
    }

    [Fact]
    public void Validate_ShouldReportMissingOwnerName()
    {
        ErrorOwnerCatalogValidator validator = new();
        ErrorOwnerCatalogDocument document = CreateValidDocument();

        document.Owners[0].Name = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingOwnerName");
    }

    [Fact]
    public void Validate_ShouldReportMissingOwnerDisplayName_AsWarning()
    {
        ErrorOwnerCatalogValidator validator = new();
        ErrorOwnerCatalogDocument document = CreateValidDocument();

        document.Owners[0].DisplayName = string.Empty;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "MissingOwnerDisplayName");
    }

    [Fact]
    public void Validate_ShouldReportInvalidOwnerCodeFrom()
    {
        ErrorOwnerCatalogValidator validator = new();
        ErrorOwnerCatalogDocument document = CreateValidDocument();

        document.Owners[0].CodeFrom = -1;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "InvalidOwnerCodeFrom");
    }

    [Fact]
    public void Validate_ShouldReportInvalidOwnerCodeTo()
    {
        ErrorOwnerCatalogValidator validator = new();
        ErrorOwnerCatalogDocument document = CreateValidDocument();

        document.Owners[0].CodeTo = -1;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "InvalidOwnerCodeTo");
    }

    [Fact]
    public void Validate_ShouldReportInvalidOwnerCodeRange()
    {
        ErrorOwnerCatalogValidator validator = new();
        ErrorOwnerCatalogDocument document = CreateValidDocument();

        document.Owners[0].CodeFrom = 1000;
        document.Owners[0].CodeTo = 999;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "InvalidOwnerCodeRange");
    }

    [Fact]
    public void Validate_ShouldAllowOwnerCodeFromZero()
    {
        ErrorOwnerCatalogValidator validator = new();
        ErrorOwnerCatalogDocument document = CreateValidDocument();

        document.Owners[0].CodeFrom = 0;
        document.Owners[0].CodeTo = 999999;

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldReportDuplicateOwnerName_UsingNormalizedComparison()
    {
        ErrorOwnerCatalogValidator validator = new();
        ErrorOwnerCatalogDocument document = CreateValidDocument();

        document.Owners.Add(new ErrorOwnerDefinition
        {
            Name = "afw",
            DisplayName = "Afrowave duplicate",
            CodeFrom = 1000000,
            CodeTo = 1999999
        });

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DuplicateOwnerName");
    }

    [Fact]
    public void Validate_ShouldReportOwnerCodeRangeOverlap()
    {
        ErrorOwnerCatalogValidator validator = new();
        ErrorOwnerCatalogDocument document = CreateValidDocument();

        document.Owners.Add(new ErrorOwnerDefinition
        {
            Name = "APP",
            DisplayName = "Application",
            CodeFrom = 500000,
            CodeTo = 1500000
        });

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "OwnerCodeRangeOverlap");
    }

    [Fact]
    public void Validate_ShouldNotReportOwnerCodeRangeOverlap_WhenRangesTouchOutside()
    {
        ErrorOwnerCatalogValidator validator = new();
        ErrorOwnerCatalogDocument document = CreateValidDocument();

        document.Owners.Add(new ErrorOwnerDefinition
        {
            Name = "APP",
            DisplayName = "Application",
            CodeFrom = 1000000,
            CodeTo = 1999999
        });

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Issues, issue => issue.Code == "OwnerCodeRangeOverlap");
    }

    [Fact]
    public void Validate_ShouldReportEmptyOwnerAlias_AsWarning()
    {
        ErrorOwnerCatalogValidator validator = new();
        ErrorOwnerCatalogDocument document = CreateValidDocument();

        document.Owners[0].Aliases = ["", "Afrowave"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "EmptyOwnerAlias");
    }

    [Fact]
    public void Validate_ShouldReportDuplicateOwnerAlias_AsWarning()
    {
        ErrorOwnerCatalogValidator validator = new();
        ErrorOwnerCatalogDocument document = CreateValidDocument();

        document.Owners[0].Aliases = ["Afro wave", "afro-wave"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DuplicateOwnerAlias");
    }

    [Fact]
    public void Validate_ShouldReportDuplicateOwnerAliasAcrossCatalog_AsWarning()
    {
        ErrorOwnerCatalogValidator validator = new();
        ErrorOwnerCatalogDocument document = CreateValidDocument();

        document.Owners[0].Aliases = ["toolbox"];

        document.Owners.Add(new ErrorOwnerDefinition
        {
            Name = "APP",
            DisplayName = "Application",
            CodeFrom = 1000000,
            CodeTo = 1999999,
            Aliases = ["toolbox"]
        });

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DuplicateOwnerAliasAcrossCatalog");
    }

    [Fact]
    public void Validate_ShouldReportOwnerAliasMatchesExistingOwnerName_AsWarning()
    {
        ErrorOwnerCatalogValidator validator = new();
        ErrorOwnerCatalogDocument document = CreateValidDocument();

        document.Owners.Add(new ErrorOwnerDefinition
        {
            Name = "APP",
            DisplayName = "Application",
            CodeFrom = 1000000,
            CodeTo = 1999999
        });

        document.Owners[0].Aliases = ["app"];

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "OwnerAliasMatchesExistingOwnerName");
    }

    private static ErrorOwnerCatalogDocument CreateValidDocument()
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
                    CodeTo = 999999,
                    IsBuiltIn = true,
                    Aliases = ["AFROWAVE"]
                }
            ]
        };
    }
}