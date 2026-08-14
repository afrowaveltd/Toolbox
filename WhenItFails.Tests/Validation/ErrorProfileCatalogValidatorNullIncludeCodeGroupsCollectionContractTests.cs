using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorProfileCatalogValidatorNullIncludeCodeGroupsCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenIncludeCodeGroupsCollectionIsNull()
    {
        ErrorProfileCatalogValidator validator = new();

        ErrorProfileCatalogDocument document = new()
        {
            Profiles =
            [
                new ErrorProfileDefinition
                {
                    IncludeCodeGroups = null!
                }
            ]
        };

        ErrorCatalogValidationResult result = validator.Validate(document);

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
