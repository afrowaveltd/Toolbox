using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Resolution;

namespace Afrowave.Toolbox.WhenItFails.Tests.Resolution;

public sealed class ErrorProfileResolverTests
{
    [Fact]
    public void Resolve_ShouldReturnAllErrors_WhenProfileContainsNoIncludeFilters()
    {
        ErrorProfileResolver resolver = new();

        IReadOnlyList<ErrorDefinition> result = resolver.Resolve(
            CreateErrorCatalog(),
            new ErrorProfileDefinition
            {
                Name = "ALL",
                DisplayName = "All errors"
            });

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Resolve_ShouldCombineIncludeFiltersUsingOrLogic()
    {
        ErrorProfileResolver resolver = new();

        ErrorProfileDefinition profile = new()
        {
            Name = "WEB_OR_STORAGE",
            DisplayName = "Web or storage",
            IncludeCategories = ["WEB"],
            IncludeTags = ["STORAGE"]
        };

        IReadOnlyList<ErrorDefinition> result = resolver.Resolve(
            CreateErrorCatalog(),
            profile);

        Assert.Collection(
            result,
            error => Assert.Equal("AFW-WEB-0001", error.Id),
            error => Assert.Equal("AFW-DSK-0001", error.Id));
    }

    [Fact]
    public void Resolve_ShouldIncludeExplicitError_WhenOtherIncludeFiltersDoNotMatch()
    {
        ErrorProfileResolver resolver = new();

        ErrorProfileDefinition profile = new()
        {
            Name = "DATABASE_WITH_EXPLICIT_ERROR",
            DisplayName = "Database with explicit error",
            IncludeCategories = ["DATABASE"],
            IncludeErrors = ["AFW-WEB-0001"]
        };

        IReadOnlyList<ErrorDefinition> result = resolver.Resolve(
            CreateErrorCatalog(),
            profile);

        Assert.Collection(
            result,
            error => Assert.Equal("AFW-WEB-0001", error.Id),
            error => Assert.Equal("AFW-DB-0001", error.Id));
    }

    [Fact]
    public void Resolve_ShouldExcludeError_WhenItMatchesExcludedTag()
    {
        ErrorProfileResolver resolver = new();

        ErrorProfileDefinition profile = new()
        {
            Name = "VISIBLE_ERRORS",
            DisplayName = "Visible errors",
            ExcludeTags = ["INTERNAL"]
        };

        IReadOnlyList<ErrorDefinition> result = resolver.Resolve(
            CreateErrorCatalog(),
            profile);

        Assert.DoesNotContain(
            result,
            error => error.Id == "AFW-DB-0001");

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Resolve_ShouldGiveExcludeErrorsPriorityOverIncludeErrors()
    {
        ErrorProfileResolver resolver = new();

        ErrorProfileDefinition profile = new()
        {
            Name = "CONFLICT",
            DisplayName = "Conflict",
            IncludeErrors = ["AFW-WEB-0001"],
            ExcludeErrors = ["afw-web-0001"]
        };

        IReadOnlyList<ErrorDefinition> result = resolver.Resolve(
            CreateErrorCatalog(),
            profile);

        Assert.Empty(result);
    }

    [Fact]
    public void Resolve_ShouldCompareFiltersCaseInsensitively()
    {
        ErrorProfileResolver resolver = new();

        ErrorProfileDefinition profile = new()
        {
            Name = "CASE_INSENSITIVE",
            DisplayName = "Case insensitive",
            IncludeOwners = ["afw"]
        };

        IReadOnlyList<ErrorDefinition> result = resolver.Resolve(
            CreateErrorCatalog(),
            profile);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Resolve_ShouldPreserveOriginalCatalogOrder()
    {
        ErrorProfileResolver resolver = new();

        ErrorProfileDefinition profile = new()
        {
            Name = "ORDER",
            DisplayName = "Order",
            IncludeErrors =
            [
                "AFW-DB-0001",
                "AFW-WEB-0001"
            ]
        };

        IReadOnlyList<ErrorDefinition> result = resolver.Resolve(
            CreateErrorCatalog(),
            profile);

        Assert.Collection(
            result,
            error => Assert.Equal("AFW-WEB-0001", error.Id),
            error => Assert.Equal("AFW-DB-0001", error.Id));
    }

[Fact]
public void Resolve_ShouldMatchIncludedCodeGroup()
    {
        ErrorProfileResolver resolver = new();

        ErrorProfileDefinition profile = new()
        {
            Name = "STORAGE_CODE_GROUP",
            DisplayName = "Storage code group",
            IncludeCodeGroups = ["storage"]
        };

        IReadOnlyList<ErrorDefinition> result = resolver.Resolve(
            CreateErrorCatalog(),
            profile);

        ErrorDefinition error = Assert.Single(result);

        Assert.Equal("AFW-DSK-0001", error.Id);
    }

    [Fact]
    public void Resolve_ShouldMatchIncludedPrimaryCategory()
    {
        ErrorProfileResolver resolver = new();

        ErrorProfileDefinition profile = new()
        {
            Name = "DATABASE_CATEGORY",
            DisplayName = "Database category",
            IncludeCategories = ["database"]
        };

        IReadOnlyList<ErrorDefinition> result = resolver.Resolve(
            CreateErrorCatalog(),
            profile);

        ErrorDefinition error = Assert.Single(result);

        Assert.Equal("AFW-DB-0001", error.Id);
    }

    [Fact]
    public void Resolve_ShouldMatchIncludedAdditionalCategory()
    {
        ErrorProfileResolver resolver = new();

        ErrorProfileDefinition profile = new()
        {
            Name = "NETWORK_CATEGORY",
            DisplayName = "Network category",
            IncludeCategories = ["network"]
        };

        IReadOnlyList<ErrorDefinition> result = resolver.Resolve(
            CreateErrorCatalog(),
            profile);

        ErrorDefinition error = Assert.Single(result);

        Assert.Equal("AFW-WEB-0001", error.Id);
    }

    [Fact]
    public void Resolve_ShouldMatchIncludedSubcategory()
    {
        ErrorProfileResolver resolver = new();

        ErrorProfileDefinition profile = new()
        {
            Name = "READ_SUBCATEGORY",
            DisplayName = "Read subcategory",
            IncludeSubcategories = ["read"]
        };

        IReadOnlyList<ErrorDefinition> result = resolver.Resolve(
            CreateErrorCatalog(),
            profile);

        ErrorDefinition error = Assert.Single(result);

        Assert.Equal("AFW-DSK-0001", error.Id);
    }

    [Fact]
    public void Resolve_ShouldMatchIncludedTag()
    {
        ErrorProfileResolver resolver = new();

        ErrorProfileDefinition profile = new()
        {
            Name = "USER_VISIBLE_TAG",
            DisplayName = "User visible tag",
            IncludeTags = ["user-visible"]
        };

        IReadOnlyList<ErrorDefinition> result = resolver.Resolve(
            CreateErrorCatalog(),
            profile);

        ErrorDefinition error = Assert.Single(result);

        Assert.Equal("AFW-WEB-0001", error.Id);
    }


    private static ErrorCatalogDocument CreateErrorCatalog()
    {
        return new ErrorCatalogDocument
        {
            SchemaVersion = "1.0",
            CatalogId = "test.errors",
            CatalogName = "Test Errors",
            Language = "en",
            Errors =
            [
                new ErrorDefinition
                {
                    Id = "AFW-WEB-0001",
                    Code = 100_001,
                    Name = "WebRequestFailed",
                    Owner = "AFW",
                    CodeGroup = "WEB",
                    PrimaryCategory = "WEB",
                    Categories = ["WEB", "NETWORK"],
                    Subcategories = ["REQUEST"],
                    Tags = ["USER_VISIBLE"],
                    Title = "Web request failed",
                    Message = "The web request failed.",
                    DefaultSeverity = "Error"
                },
                new ErrorDefinition
                {
                    Id = "AFW-DSK-0001",
                    Code = 200_001,
                    Name = "DiskReadFailed",
                    Owner = "AFW",
                    CodeGroup = "STORAGE",
                    PrimaryCategory = "DISK",
                    Categories = ["DISK", "STORAGE"],
                    Subcategories = ["READ"],
                    Tags = ["STORAGE"],
                    Title = "Disk read failed",
                    Message = "The disk could not be read.",
                    DefaultSeverity = "Error"
                },
                new ErrorDefinition
                {
                    Id = "AFW-DB-0001",
                    Code = 300_001,
                    Name = "DatabaseConnectionFailed",
                    Owner = "AFW",
                    CodeGroup = "DATABASE",
                    PrimaryCategory = "DATABASE",
                    Categories = ["DATABASE"],
                    Subcategories = ["CONNECTION"],
                    Tags = ["INTERNAL"],
                    Title = "Database connection failed",
                    Message = "The database connection failed.",
                    DefaultSeverity = "Critical"
                }
            ]
        };
    }
}

