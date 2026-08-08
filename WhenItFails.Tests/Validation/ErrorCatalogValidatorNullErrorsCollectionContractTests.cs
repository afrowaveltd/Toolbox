using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogValidatorNullErrorsCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenErrorCollectionIsNull()
    {
        ErrorCatalogValidator validator = new();

        ErrorCatalogDocument document = new()
        {
            Errors = null!
        };

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "CatalogErrorsCollectionIsNull");

        Assert.Equal("errors", issue.Path);
        Assert.Equal(
            "Error catalog errors collection is null.",
            issue.Message);
    }
}
