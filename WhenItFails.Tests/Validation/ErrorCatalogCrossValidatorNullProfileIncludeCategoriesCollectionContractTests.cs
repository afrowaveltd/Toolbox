using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogCrossValidatorNullProfileIncludeCategoriesCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenProfileIncludeCategoriesCollectionIsNull()
    {
        ErrorCatalogCrossValidator validator = new();

        ErrorCategoryCatalogDocument categoryCatalog = new()
        {
            Categories =
            [
                new ErrorCategoryDefinition
                {
                    Name = "GENERAL"
                }
            ]
        };

        ErrorProfileCatalogDocument profileCatalog = new()
        {
            Profiles =
            [
                new ErrorProfileDefinition
                {
                    IncludeCategories = null!
                }
            ]
        };

        ErrorCatalogValidationResult result = validator.Validate(
            new ErrorCatalogDocument(),
            new ErrorOwnerCatalogDocument(),
            new ErrorCodeGroupCatalogDocument(),
            categoryCatalog,
            profileCatalog);

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "ProfileIncludeCategoriesCollectionIsNull");

        Assert.Equal("profiles[0].includeCategories", issue.Path);
        Assert.Equal(
            "Profile include categories collection is null.",
            issue.Message);
    }
}
