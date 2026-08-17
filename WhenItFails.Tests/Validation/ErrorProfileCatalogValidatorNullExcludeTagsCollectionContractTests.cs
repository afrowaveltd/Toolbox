using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorProfileCatalogValidatorNullExcludeTagsCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenExcludeTagsCollectionIsNull()
    {
        ErrorProfileCatalogValidator validator = new();

        ErrorProfileCatalogDocument document = new()
        {
            Profiles =
            [
                new ErrorProfileDefinition
                {
                    ExcludeTags = null!
                }
            ]
        };

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "ProfileExcludeTagsCollectionIsNull");

        Assert.Equal("profiles[0].excludeTags", issue.Path);
        Assert.Equal(
            "Profile exclude tags collection is null.",
            issue.Message);
    }
}
