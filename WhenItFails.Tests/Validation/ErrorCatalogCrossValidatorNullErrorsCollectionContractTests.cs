using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogCrossValidatorNullErrorsCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenErrorCatalogErrorsCollectionIsNull()
    {
        ErrorCatalogCrossValidator validator = new();

        ErrorCatalogDocument errorCatalog = new()
        {
            Errors = null!
        };

        ErrorCatalogValidationResult result = validator.Validate(
            errorCatalog,
            new ErrorOwnerCatalogDocument(),
            new ErrorCodeGroupCatalogDocument(),
            new ErrorCategoryCatalogDocument(),
            new ErrorProfileCatalogDocument());

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
