using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Models;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ErrorsCommandProfileFilterTests
{
    [Fact]
    public void ApplyErrorFilters_WithMultipleProfileIncludes_UsesOrLogic()
    {
        WhenItFailsWorkspaceSummary summary = CreateSummary(
            new ErrorProfileDefinition
            {
                Name = "WEB_OR_STORAGE",
                DisplayName = "Web or storage",
                IncludeCategories = ["WEB"],
                IncludeTags = ["STORAGE"]
            });

        ErrorListOptions options = new()
        {
            Profile = "WEB_OR_STORAGE"
        };

        IReadOnlyList<ErrorDefinition> result = ErrorsCommand
            .ApplyErrorFilters(summary, options)
            .ToList();

        Assert.Collection(
            result,
            error => Assert.Equal("AFW-WEB-0001", error.Id),
            error => Assert.Equal("AFW-DSK-0001", error.Id));
    }

    [Fact]
    public void ApplyErrorFilters_WithExcludedTag_AppliesExclusionAsVeto()
    {
        WhenItFailsWorkspaceSummary summary = CreateSummary(
            new ErrorProfileDefinition
            {
                Name = "VISIBLE",
                DisplayName = "Visible",
                IncludeOwners = ["AFW"],
                ExcludeTags = ["INTERNAL"]
            });

        ErrorListOptions options = new()
        {
            Profile = "VISIBLE"
        };

        IReadOnlyList<ErrorDefinition> result = ErrorsCommand
            .ApplyErrorFilters(summary, options)
            .ToList();

        Assert.Collection(
            result,
            error => Assert.Equal("AFW-WEB-0001", error.Id),
            error => Assert.Equal("AFW-DSK-0001", error.Id));
    }

    [Fact]
    public void ApplyErrorFilters_WithExplicitIncludeAndExclude_ExclusionWins()
    {
        WhenItFailsWorkspaceSummary summary = CreateSummary(
            new ErrorProfileDefinition
            {
                Name = "CONFLICT",
                DisplayName = "Conflict",
                IncludeErrors = ["AFW-WEB-0001"],
                ExcludeErrors = ["afw-web-0001"]
            });

        ErrorListOptions options = new()
        {
            Profile = "CONFLICT"
        };

        IReadOnlyList<ErrorDefinition> result = ErrorsCommand
            .ApplyErrorFilters(summary, options)
            .ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void ApplyErrorFilters_AppliesAdditionalCliFiltersAfterProfileResolution()
    {
        WhenItFailsWorkspaceSummary summary = CreateSummary(
            new ErrorProfileDefinition
            {
                Name = "ALL_AFW",
                DisplayName = "All Afrowave",
                IncludeOwners = ["AFW"]
            });

        ErrorListOptions options = new()
        {
            Profile = "ALL_AFW",
            Severity = "Critical"
        };

        ErrorDefinition result = Assert.Single(
            ErrorsCommand.ApplyErrorFilters(summary, options));

        Assert.Equal("AFW-DB-0001", result.Id);
    }

    [Fact]
    public void ApplyErrorFilters_WithProfileDisplayName_UsesResolvedProfile()
    {
        WhenItFailsWorkspaceSummary summary = CreateSummary(
            new ErrorProfileDefinition
            {
                Name = "STORAGE_ONLY",
                DisplayName = "Storage only",
                IncludeSubcategories = ["READ"]
            });

        ErrorListOptions options = new()
        {
            Profile = "storage only"
        };

        ErrorDefinition result = Assert.Single(
            ErrorsCommand.ApplyErrorFilters(summary, options));

        Assert.Equal("AFW-DSK-0001", result.Id);
    }

    private static WhenItFailsWorkspaceSummary CreateSummary(
        ErrorProfileDefinition profile)
    {
        return new WhenItFailsWorkspaceSummary
        {
            ErrorCatalog = new ErrorCatalogDocument
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
            },
            ProfileCatalog = new ErrorProfileCatalogDocument
            {
                SchemaVersion = "1.0",
                Profiles = [profile]
            }
        };
    }
}
