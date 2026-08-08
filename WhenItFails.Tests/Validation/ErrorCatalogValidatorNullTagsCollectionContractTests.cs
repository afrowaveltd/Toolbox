using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogValidatorNullTagsCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenErrorTagsCollectionIsNull()
    {
        ErrorCatalogValidator validator = new();

        ErrorCatalogDocument document = new()
        {
            Errors =
            [
                new ErrorDefinition
                {
                    Tags = null!
                }
            ]
        };

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "ErrorTagsCollectionIsNull");

        Assert.Equal("errors[0].tags", issue.Path);
        Assert.Equal(
            "Error tags collection is null.",
            issue.Message);
    }
}
