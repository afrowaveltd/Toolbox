using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCategoryCatalogValidatorNullAliasesCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenCategoryAliasesCollectionIsNull()
    {
        ErrorCategoryCatalogValidator validator = new();

        ErrorCategoryCatalogDocument document = new()
        {
            Categories =
            [
                new ErrorCategoryDefinition
                {
                    Aliases = null!
                }
            ]
        };

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "CategoryAliasesCollectionIsNull");

        Assert.Equal("categories[0].aliases", issue.Path);
        Assert.Equal(
            "Category aliases collection is null.",
            issue.Message);
    }
}
