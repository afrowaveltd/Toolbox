using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogCrossValidatorNullProfileIncludeCodeGroupsCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenProfileIncludeCodeGroupsCollectionIsNull()
    {
        ErrorCatalogCrossValidator validator = new();

        ErrorCodeGroupCatalogDocument codeGroupCatalog = new()
        {
            CodeGroups =
            [
                new ErrorCodeGroupDefinition
                {
                    Name = "CORE"
                }
            ]
        };

        ErrorProfileCatalogDocument profileCatalog = new()
        {
            Profiles =
            [
                new ErrorProfileDefinition
                {
                    IncludeCodeGroups = null!
                }
            ]
        };

        ErrorCatalogValidationResult result = validator.Validate(
            new ErrorCatalogDocument(),
            new ErrorOwnerCatalogDocument(),
            codeGroupCatalog,
            new ErrorCategoryCatalogDocument(),
            profileCatalog);

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "ProfileIncludeCodeGroupsCollectionIsNull");

        Assert.Equal("profiles[0].includeCodeGroups", issue.Path);
        Assert.Equal(
            "Profile include code groups collection is null.",
            issue.Message);
    }
}
