using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogCrossValidatorNullCategoryAliasesCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenCategoryAliasesCollectionIsNull()
    {
        ErrorCatalogCrossValidator validator = new();

        ErrorCategoryCatalogDocument categoryCatalog = new()
        {
            Categories =
            [
                new ErrorCategoryDefinition
                {
                    Name = "GENERAL",
                    Aliases = null!
                }
            ]
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
            issue => issue.Code == "CategoryAliasesCollectionIsNull");

        Assert.Equal("categoryCatalog.categories[0].aliases", issue.Path);
        Assert.Equal(
            "Category aliases collection is null.",
            issue.Message);
    }
}
