using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogValidatorNullSubcategoriesCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenErrorSubcategoriesCollectionIsNull()
    {
        ErrorCatalogValidator validator = new();

        ErrorCatalogDocument document = new()
        {
            Errors =
            [
                new ErrorDefinition
                {
                    Subcategories = null!
                }
            ]
        };

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "ErrorSubcategoriesCollectionIsNull");

        Assert.Equal("errors[0].subcategories", issue.Path);
        Assert.Equal(
            "Error subcategories collection is null.",
            issue.Message);
    }
}
