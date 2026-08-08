using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Resolution;

namespace Afrowave.Toolbox.WhenItFails.Tests.Resolution;

public sealed class ErrorProfileResolverNormalizationContractTests
{
    [Theory]
    [InlineData("error-id")]
    [InlineData("owner")]
    [InlineData("code-group")]
    [InlineData("category")]
    [InlineData("subcategory")]
    [InlineData("tag")]
    public void Resolve_ShouldNormalizeFlexibleIncludeFilters(string filter)
    {
        ErrorProfileDefinition profile = new()
        {
            Name = "NORMALIZED_INCLUDE",
            DisplayName = "Normalized include"
        };

        switch (filter)
        {
            case "error-id":
                profile.IncludeErrors = ["  afw err-0001  "];
                break;
            case "owner":
                profile.IncludeOwners = ["  afw-team  "];
                break;
            case "code-group":
                profile.IncludeCodeGroups = [" network io "];
                break;
            case "category":
                profile.IncludeCategories = [" web-api "];
                break;
            case "subcategory":
                profile.IncludeSubcategories = [" read_error "];
                break;
            case "tag":
                profile.IncludeTags = [" disk failure "];
                break;
        }

        IReadOnlyList<ErrorDefinition> result =
            new ErrorProfileResolver().Resolve(CreateCatalog(), profile);

        ErrorDefinition error = Assert.Single(result);
        Assert.Equal("AFW_ERR_0001", error.Id);
    }

    [Theory]
    [InlineData("error-id")]
    [InlineData("tag")]
    public void Resolve_ShouldNormalizeFlexibleExcludeFilters(string filter)
    {
        ErrorProfileDefinition profile = new()
        {
            Name = "NORMALIZED_EXCLUDE",
            DisplayName = "Normalized exclude"
        };

        if (filter == "error-id")
        {
            profile.ExcludeErrors = ["  afw err-0001  "];
        }
        else
        {
            profile.ExcludeTags = [" disk failure "];
        }

        IReadOnlyList<ErrorDefinition> result =
            new ErrorProfileResolver().Resolve(CreateCatalog(), profile);

        Assert.Empty(result);
    }

    private static ErrorCatalogDocument CreateCatalog()
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
                    Id = "AFW_ERR_0001",
                    Code = 100001,
                    Name = "DiskReadFailed",
                    Owner = "AFW_TEAM",
                    CodeGroup = "NETWORK_IO",
                    PrimaryCategory = "WEB_API",
                    Categories = ["WEB_API"],
                    Subcategories = ["READ_ERROR"],
                    Tags = ["DISK_FAILURE"],
                    Title = "Disk read failed",
                    Message = "The disk read failed.",
                    DefaultSeverity = "Error"
                }
            ]
        };
    }
}
