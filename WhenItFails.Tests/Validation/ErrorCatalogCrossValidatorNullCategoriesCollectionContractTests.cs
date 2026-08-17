using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogCrossValidatorNullCategoriesCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenCategoryCatalogCategoriesCollectionIsNull()
    {
        ErrorCatalogCrossValidator validator = new();

        ErrorCategoryCatalogDocument categoryCatalog = new()
        {
            Categories = null!
        };

        ErrorCatalogValidationResult result = validator.Validate(
            new ErrorCatalogDocument(),
            new ErrorOwnerCatalogDocument(),
            new ErrorCodeGroupCatalogDocument(),
            categoryCatalog,
            new ErrorProfileCatalogDocument());

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "CategoryCatalogCategoriesCollectionIsNull");

        Assert.Equal("categoryCatalog.categories", issue.Path);
        Assert.Equal(
            "Category catalog categories collection is null.",
            issue.Message);
    }
}
