using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogValidatorNullCategoriesCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenErrorCategoriesCollectionIsNull()
    {
        ErrorCatalogValidator validator = new();

        ErrorCatalogDocument document = new()
        {
            Errors =
            [
                new ErrorDefinition
                {
                    Categories = null!
                }
            ]
        };

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "ErrorCategoriesCollectionIsNull");

        Assert.Equal("errors[0].categories", issue.Path);
        Assert.Equal(
            "Error categories collection is null.",
            issue.Message);
    }
}
