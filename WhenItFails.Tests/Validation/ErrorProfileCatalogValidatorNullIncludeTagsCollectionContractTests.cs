using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorProfileCatalogValidatorNullIncludeTagsCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenIncludeTagsCollectionIsNull()
    {
        ErrorProfileCatalogValidator validator = new();

        ErrorProfileCatalogDocument document = new()
        {
            Profiles =
            [
                new ErrorProfileDefinition
                {
                    IncludeTags = null!
                }
            ]
        };

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "ProfileIncludeTagsCollectionIsNull");

        Assert.Equal("profiles[0].includeTags", issue.Path);
        Assert.Equal(
            "Profile include tags collection is null.",
            issue.Message);
    }
}
