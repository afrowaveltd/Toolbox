using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCodeGroupCatalogValidatorNullDefaultTagsCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenDefaultTagsCollectionIsNull()
    {
        ErrorCodeGroupCatalogValidator validator = new();

        ErrorCodeGroupCatalogDocument document = new()
        {
            CodeGroups =
            [
                new ErrorCodeGroupDefinition
                {
                    DefaultTags = null!
                }
            ]
        };

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "CodeGroupDefaultTagsCollectionIsNull");

        Assert.Equal("codeGroups[0].defaultTags", issue.Path);
        Assert.Equal(
            "Code group default tags collection is null.",
            issue.Message);
    }
}
