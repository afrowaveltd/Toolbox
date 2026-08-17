using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCategoryCatalogValidatorNullDefaultTagsCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenCategoryDefaultTagsCollectionIsNull()
    {
        ErrorCategoryCatalogValidator validator = new();

        ErrorCategoryCatalogDocument document = new()
        {
            Categories =
            [
                new ErrorCategoryDefinition
                {
                    DefaultTags = null!
                }
            ]
        };

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "CategoryDefaultTagsCollectionIsNull");

        Assert.Equal("categories[0].defaultTags", issue.Path);
        Assert.Equal(
            "Category default tags collection is null.",
            issue.Message);
    }
}
