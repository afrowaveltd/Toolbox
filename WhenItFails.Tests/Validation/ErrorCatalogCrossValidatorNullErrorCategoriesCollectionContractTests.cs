using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogCrossValidatorNullErrorCategoriesCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenErrorCategoriesCollectionIsNull()
    {
        ErrorCatalogCrossValidator validator = new();

        ErrorCatalogDocument errorCatalog = new()
        {
            Errors =
            [
                new ErrorDefinition
                {
                    Id = "TEST-ERR-0001",
                    Name = "TestError",
                    PrimaryCategory = "GENERAL",
                    Categories = null!
                }
            ]
        };

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

        ErrorCatalogValidationResult result = validator.Validate(
            errorCatalog,
            new ErrorOwnerCatalogDocument(),
            new ErrorCodeGroupCatalogDocument(),
            categoryCatalog,
            new ErrorProfileCatalogDocument());

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
