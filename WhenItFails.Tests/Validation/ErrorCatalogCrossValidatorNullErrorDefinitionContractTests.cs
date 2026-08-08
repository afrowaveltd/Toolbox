using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogCrossValidatorNullErrorDefinitionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenErrorCatalogContainsNullDefinition()
    {
        ErrorCatalogCrossValidator validator = new();

        ErrorCatalogDocument errorCatalog = new()
        {
            Errors = [null!]
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
            issue => issue.Code == "ErrorDefinitionIsNull");

        Assert.Equal("errors[0]", issue.Path);
        Assert.Equal(
            "Error catalog contains a null error definition.",
            issue.Message);
    }
}
