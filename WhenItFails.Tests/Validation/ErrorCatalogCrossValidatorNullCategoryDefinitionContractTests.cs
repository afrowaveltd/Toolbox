using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogCrossValidatorNullCategoryDefinitionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenCategoryCatalogContainsNullDefinition()
    {
        ErrorCatalogCrossValidator validator = new();

        ErrorCategoryCatalogDocument categoryCatalog = new()
        {
            Categories = [null!]
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
            issue => issue.Code == "CategoryDefinitionIsNull");

        Assert.Equal("categoryCatalog.categories[0]", issue.Path);
        Assert.Equal(
            "Category catalog contains a null category definition.",
            issue.Message);
    }
}
