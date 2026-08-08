using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCategoryCatalogValidatorNullParentCategoriesCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenCategoryParentCategoriesCollectionIsNull()
    {
        ErrorCategoryCatalogValidator validator = new();

        ErrorCategoryCatalogDocument document = new()
        {
            Categories =
            [
                new ErrorCategoryDefinition
                {
                    ParentCategories = null!
                }
            ]
        };

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "CategoryParentCategoriesCollectionIsNull");

        Assert.Equal("categories[0].parentCategories", issue.Path);
        Assert.Equal(
            "Category parent categories collection is null.",
            issue.Message);
    }
}
