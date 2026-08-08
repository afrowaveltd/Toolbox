using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogValidatorNullErrorDefinitionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenErrorCollectionContainsNullDefinition()
    {
        ErrorCatalogValidator validator = new();

        ErrorCatalogDocument document = new()
        {
            Errors = [null!]
        };

        ErrorCatalogValidationResult result = validator.Validate(document);

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
