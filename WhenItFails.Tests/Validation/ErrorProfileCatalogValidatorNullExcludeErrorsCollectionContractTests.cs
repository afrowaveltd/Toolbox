using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorProfileCatalogValidatorNullExcludeErrorsCollectionContractTests
{
    [Fact]
    public void Validate_ShouldReturnInvalidResult_WhenProfileExcludeErrorsCollectionIsNull()
    {
        ErrorProfileCatalogValidator validator = new();

        ErrorProfileCatalogDocument document = new()
        {
            Profiles =
            [
                new ErrorProfileDefinition
                {
                    ExcludeErrors = null!
                }
            ]
        };

        ErrorCatalogValidationResult result = validator.Validate(document);

        Assert.False(result.IsValid);

        var issue = Assert.Single(
            result.Issues,
            issue => issue.Code == "ProfileExcludeErrorsCollectionIsNull");

        Assert.Equal("profiles[0].excludeErrors", issue.Path);
        Assert.Equal(
            "Profile exclude errors collection is null.",
            issue.Message);
    }
}
