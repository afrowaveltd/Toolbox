using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCategoryCatalogValidatorNullCategoryDefinitionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenCategoryCollectionContainsNullDefinition()
    {
        ErrorCategoryCatalogValidator validator = new();

        ErrorCategoryCatalogDocument document = new()
        {
            Categories = [null!]
        };

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "CategoryDefinitionIsNull");

        Assert.Equal("categories[0]", issue.Path);
        Assert.Equal(
            "Category catalog contains a null category definition.",
            issue.Message);
    }
}
